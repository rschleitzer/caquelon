using Antlr4.Runtime;
using Fire.Cql.Grammar;

namespace Fire.Cql;

public class CqlInterpreter : cqlBaseVisitor<object?>
{
    public static object? Evaluate(string expression)
    {
        var input = new AntlrInputStream(expression);
        var lexer = new cqlLexer(input);
        var tokens = new CommonTokenStream(lexer);
        var parser = new cqlParser(tokens);
        var tree = parser.expression();
        return new CqlInterpreter().Visit(tree);
    }

    // Literals

    public override object? VisitBooleanLiteral(cqlParser.BooleanLiteralContext context)
        => context.GetText() == "true";

    public override object? VisitNullLiteral(cqlParser.NullLiteralContext context)
        => null;

    public override object? VisitStringLiteral(cqlParser.StringLiteralContext context)
    {
        var text = context.STRING().GetText();
        return UnescapeCqlString(text[1..^1]);
    }

    public override object? VisitNumberLiteral(cqlParser.NumberLiteralContext context)
    {
        var text = context.NUMBER().GetText();
        if (text.Contains('.'))
            return decimal.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
        return int.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
    }

    public override object? VisitLongNumberLiteral(cqlParser.LongNumberLiteralContext context)
    {
        var text = context.LONGNUMBER().GetText();
        return long.Parse(text[..^1], System.Globalization.CultureInfo.InvariantCulture);
    }

    public override object? VisitQuantityLiteral(cqlParser.QuantityLiteralContext context)
    {
        var quantity = context.quantity();
        var value = decimal.Parse(quantity.NUMBER().GetText(), System.Globalization.CultureInfo.InvariantCulture);
        var unit = quantity.unit();
        var unitStr = unit is null ? "1" : unit.STRING()?.GetText() is string s ? s[1..^1] : unit.GetText();
        return new CqlQuantity(value, unitStr);
    }

    public override object? VisitDateTimeLiteral(cqlParser.DateTimeLiteralContext context)
    {
        var text = context.DATETIME().GetText(); // @YYYY-MM-DDThh:mm:ss.fff
        return ParseDateTime(text[1..]); // skip @
    }

    public override object? VisitDateLiteral(cqlParser.DateLiteralContext context)
    {
        var text = context.DATE().GetText(); // @YYYY-MM-DD
        return ParseDate(text[1..]); // skip @
    }

    public override object? VisitTimeLiteral(cqlParser.TimeLiteralContext context)
    {
        var text = context.TIME().GetText(); // @Thh:mm:ss.fff
        return ParseTime(text[2..]); // skip @T
    }

    // Selectors

    public override object? VisitIntervalSelectorTerm(cqlParser.IntervalSelectorTermContext context)
    {
        var selector = context.intervalSelector();
        var low = Visit(selector.expression(0));
        var high = Visit(selector.expression(1));
        var lowClosed = selector.GetChild(1).GetText() == "[";
        var highClosed = selector.GetChild(selector.ChildCount - 1).GetText() == "]";
        return new CqlInterval(low, high, lowClosed, highClosed);
    }

    public override object? VisitListSelectorTerm(cqlParser.ListSelectorTermContext context)
    {
        var selector = context.listSelector();
        var list = new List<object?>();
        foreach (var expr in selector.expression())
            list.Add(Visit(expr));
        return list;
    }

    public override object? VisitTupleSelectorTerm(cqlParser.TupleSelectorTermContext context)
    {
        var selector = context.tupleSelector();
        var tuple = new CqlTuple();
        foreach (var elem in selector.tupleElementSelector())
        {
            var name = elem.referentialIdentifier().GetText();
            var value = Visit(elem.expression());
            tuple.Elements[name] = value;
        }
        return tuple;
    }

    public override object? VisitParenthesizedTerm(cqlParser.ParenthesizedTermContext context)
        => Visit(context.expression());

    // Arithmetic

    public override object? VisitAdditionExpressionTerm(cqlParser.AdditionExpressionTermContext context)
    {
        var left = Visit(context.expressionTerm(0));
        var right = Visit(context.expressionTerm(1));
        var op = context.GetChild(1).GetText();

        if (op == "&")
        {
            var ls = left as string ?? "";
            var rs = right as string ?? "";
            return ls + rs;
        }

        if (left is null || right is null) return null;

        return op switch
        {
            "+" => Add(left, right),
            "-" => Subtract(left, right),
            _ => throw new NotSupportedException($"Unknown addition operator: {op}")
        };
    }

    public override object? VisitMultiplicationExpressionTerm(cqlParser.MultiplicationExpressionTermContext context)
    {
        var left = Visit(context.expressionTerm(0));
        var right = Visit(context.expressionTerm(1));
        if (left is null || right is null) return null;
        var op = context.GetChild(1).GetText();
        return op switch
        {
            "*" => Multiply(left, right),
            "/" => Divide(left, right),
            "div" => TruncatedDivide(left, right),
            "mod" => Modulo(left, right),
            _ => throw new NotSupportedException($"Unknown multiplication operator: {op}")
        };
    }

    public override object? VisitPowerExpressionTerm(cqlParser.PowerExpressionTermContext context)
    {
        var left = Visit(context.expressionTerm(0));
        var right = Visit(context.expressionTerm(1));
        if (left is null || right is null) return null;
        return Power(left, right);
    }

    public override object? VisitPolarityExpressionTerm(cqlParser.PolarityExpressionTermContext context)
    {
        var value = Visit(context.expressionTerm());
        if (value is null) return null;
        var op = context.GetChild(0).GetText();
        if (op == "+") return value;
        return value switch
        {
            int i => -i,
            long l => -l,
            decimal d => -d,
            CqlQuantity q => new CqlQuantity(-q.Value, q.Unit),
            _ => throw new NotSupportedException($"Cannot negate {value.GetType().Name}")
        };
    }

    // Comparison

    public override object? VisitEqualityExpression(cqlParser.EqualityExpressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));
        var op = context.GetChild(1).GetText();
        return op switch
        {
            "=" => EqualWithNullPropagation(left, right),
            "!=" => NegateNullable(EqualWithNullPropagation(left, right)),
            "~" => Equivalent(left, right),
            "!~" => !Equivalent(left, right),
            _ => throw new NotSupportedException($"Unknown equality operator: {op}")
        };
    }

    public override object? VisitInequalityExpression(cqlParser.InequalityExpressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));
        var cmp = CqlComparison.Compare(left, right);
        if (cmp is null) return null;
        var op = context.GetChild(1).GetText();
        return op switch
        {
            "<" => cmp < 0,
            ">" => cmp > 0,
            "<=" => cmp <= 0,
            ">=" => cmp >= 0,
            _ => throw new NotSupportedException($"Unknown inequality operator: {op}")
        };
    }

    // Boolean

    public override object? VisitAndExpression(cqlParser.AndExpressionContext context)
    {
        var left = Visit(context.expression(0)) as bool?;
        var right = Visit(context.expression(1)) as bool?;
        if (left == false || right == false) return false;
        if (left == true && right == true) return true;
        return null;
    }

    public override object? VisitOrExpression(cqlParser.OrExpressionContext context)
    {
        var left = Visit(context.expression(0)) as bool?;
        var right = Visit(context.expression(1)) as bool?;
        var op = context.GetChild(1).GetText();
        if (op == "xor")
        {
            if (left is null || right is null) return null;
            return left.Value ^ right.Value;
        }
        // or
        if (left == true || right == true) return true;
        if (left == false && right == false) return false;
        return null;
    }

    public override object? VisitNotExpression(cqlParser.NotExpressionContext context)
    {
        var value = Visit(context.expression()) as bool?;
        if (value is null) return null;
        return !value.Value;
    }

    public override object? VisitImpliesExpression(cqlParser.ImpliesExpressionContext context)
    {
        var left = Visit(context.expression(0)) as bool?;
        var right = Visit(context.expression(1)) as bool?;
        if (left == false) return true;
        if (left == true && right == true) return true;
        if (left == true && right == false) return false;
        return null;
    }

    // Boolean expression: is null, is not null, is true, is false, etc.
    public override object? VisitBooleanExpression(cqlParser.BooleanExpressionContext context)
    {
        var value = Visit(context.expression());
        // Children after the expression: 'is' 'not'? ('null'|'true'|'false')
        var children = Enumerable.Range(1, context.ChildCount - 1)
            .Select(i => context.GetChild(i).GetText()).ToList();
        bool negate = children.Contains("not");
        var keyword = children.Last();

        bool result = keyword switch
        {
            "null" => value is null,
            "true" => value is true,
            "false" => value is false,
            _ => false,
        };
        return negate ? !result : result;
    }

    // Type expression: is / as
    public override object? VisitTypeExpression(cqlParser.TypeExpressionContext context)
    {
        var value = Visit(context.expression());
        var typeName = context.typeSpecifier().GetText();
        var isOp = context.GetChild(1).GetText() == "is";

        if (isOp)
        {
            return typeName switch
            {
                "Integer" => value is int,
                "Long" => value is long,
                "Decimal" => value is decimal,
                "Boolean" => value is bool,
                "String" => value is string,
                "DateTime" => value is CqlDateTime,
                "Date" => value is CqlDate,
                "Time" => value is CqlTime,
                "Quantity" => value is CqlQuantity,
                _ => false,
            };
        }

        // as — cast, returns null if not the right type
        return typeName switch
        {
            "Integer" => value is int ? value : null,
            "Long" => value is long ? value : null,
            "Decimal" => value is decimal ? value : null,
            "Boolean" => value is bool ? value : null,
            "String" => value is string ? value : null,
            "DateTime" => value is CqlDateTime ? value : null,
            "Date" => value is CqlDate ? value : null,
            "Time" => value is CqlTime ? value : null,
            "Quantity" => value is CqlQuantity ? value : null,
            _ => null,
        };
    }

    // If-then-else
    public override object? VisitIfThenElseExpressionTerm(cqlParser.IfThenElseExpressionTermContext context)
    {
        var condition = Visit(context.expression(0)) as bool?;
        if (condition == true)
            return Visit(context.expression(1));
        return Visit(context.expression(2));
    }

    // Case
    public override object? VisitCaseExpressionTerm(cqlParser.CaseExpressionTermContext context)
    {
        var items = context.caseExpressionItem();
        // context.expression() includes: optional comparand + else expression
        // If there are more expressions than case items + 1 (else), there's a comparand
        var expressions = context.expression();
        bool hasComparand = expressions.Length > 1; // 1 for else, more means comparand present
        var comparand = hasComparand ? Visit(expressions[0]) : null;

        foreach (var item in items)
        {
            if (hasComparand)
            {
                var when = Visit(item.expression(0));
                if (CqlComparison.Equal(comparand, when))
                    return Visit(item.expression(1));
            }
            else
            {
                var when = Visit(item.expression(0)) as bool?;
                if (when == true)
                    return Visit(item.expression(1));
            }
        }
        // else is the last expression in context
        return Visit(expressions[^1]);
    }

    // Exists
    public override object? VisitExistenceExpression(cqlParser.ExistenceExpressionContext context)
    {
        var value = Visit(context.expression());
        if (value is null) return false;
        if (value is List<object?> list) return list.Count > 0;
        return true;
    }

    // Between
    public override object? VisitBetweenExpression(cqlParser.BetweenExpressionContext context)
    {
        var value = Visit(context.expression());
        var low = Visit(context.expressionTerm(0));
        var high = Visit(context.expressionTerm(1));
        var cmpLow = CqlComparison.Compare(value, low);
        var cmpHigh = CqlComparison.Compare(value, high);
        if (cmpLow is null || cmpHigh is null) return null;
        bool properly = context.GetText().Contains("properly");
        if (properly)
            return cmpLow > 0 && cmpHigh < 0;
        return cmpLow >= 0 && cmpHigh <= 0;
    }

    // Set operations
    public override object? VisitInFixSetExpression(cqlParser.InFixSetExpressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));
        var op = context.GetChild(1).GetText();

        var leftList = ToList(left);
        var rightList = ToList(right);

        return op switch
        {
            "|" or "union" => Union(leftList, rightList),
            "intersect" => Intersect(leftList, rightList),
            "except" => Except(leftList, rightList),
            _ => throw new NotSupportedException($"Unknown set operator: {op}")
        };
    }

    // Membership: in / contains
    public override object? VisitMembershipExpression(cqlParser.MembershipExpressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));
        var op = context.GetChild(1).GetText();

        if (op == "contains")
            (left, right) = (right, left);

        // element in list
        if (right is List<object?> list)
        {
            if (left is null) return null;
            foreach (var item in list)
                if (CqlComparison.Equal(left, item)) return true;
            return false;
        }

        // point in interval
        if (right is CqlInterval interval)
        {
            if (left is null) return null;
            var cmpLow = interval.Low is null ? null : CqlComparison.Compare(left, interval.Low);
            var cmpHigh = interval.High is null ? null : CqlComparison.Compare(left, interval.High);
            bool? lowOk = interval.Low is null ? true : cmpLow is null ? null : interval.LowClosed ? cmpLow >= 0 : cmpLow > 0;
            bool? highOk = interval.High is null ? true : cmpHigh is null ? null : interval.HighClosed ? cmpHigh <= 0 : cmpHigh < 0;
            if (lowOk == false || highOk == false) return false;
            if (lowOk == true && highOk == true) return true;
            return null;
        }

        return null;
    }

    // Duration between
    public override object? VisitDurationBetweenExpression(cqlParser.DurationBetweenExpressionContext context)
    {
        var left = Visit(context.expressionTerm(0));
        var right = Visit(context.expressionTerm(1));
        var precision = context.pluralDateTimePrecision().GetText();
        return DurationBetween(left, right, precision);
    }

    // Difference between
    public override object? VisitDifferenceBetweenExpression(cqlParser.DifferenceBetweenExpressionContext context)
    {
        var left = Visit(context.expressionTerm(0));
        var right = Visit(context.expressionTerm(1));
        var precision = context.pluralDateTimePrecision().GetText();
        return DurationBetween(left, right, precision);
    }

    // Component extraction: year from, month from, day from, hour from, etc.
    public override object? VisitTimeUnitExpressionTerm(cqlParser.TimeUnitExpressionTermContext context)
    {
        var value = Visit(context.expressionTerm());
        var component = context.dateTimeComponent().GetText();
        return ExtractComponent(value, component);
    }

    // Functions
    public override object? VisitFunctionInvocation(cqlParser.FunctionInvocationContext context)
    {
        var func = context.function();
        var name = func.referentialIdentifier().GetText();
        var args = func.paramList()?.expression().Select(Visit).ToArray() ?? [];
        return EvalFunction(name, args);
    }

    public override object? VisitQualifiedFunctionInvocation(cqlParser.QualifiedFunctionInvocationContext context)
    {
        var func = context.qualifiedFunction();
        var name = func.identifierOrFunctionIdentifier().GetText();
        var args = func.paramList()?.expression().Select(Visit).ToArray() ?? [];
        return EvalFunction(name, args);
    }

    // Invocation on expression: expr.member or expr.function()
    public override object? VisitInvocationExpressionTerm(cqlParser.InvocationExpressionTermContext context)
    {
        var target = Visit(context.expressionTerm());
        var invocation = context.qualifiedInvocation();

        // member access on tuple
        if (invocation is cqlParser.QualifiedMemberInvocationContext member)
        {
            var memberName = member.referentialIdentifier().GetText();
            if (target is CqlTuple tuple && tuple.Elements.TryGetValue(memberName, out var val))
                return val;
            return null;
        }

        // qualified function invocation (e.g., value.toString())
        if (invocation is cqlParser.QualifiedFunctionInvocationContext funcCtx)
        {
            var func = funcCtx.qualifiedFunction();
            var name = func.identifierOrFunctionIdentifier().GetText();
            var args = func.paramList()?.expression().Select(Visit).ToArray() ?? [];
            return EvalMethodOnTarget(target, name, args);
        }

        return null;
    }

    // Aggregate expression: distinct, flatten
    public override object? VisitAggregateExpressionTerm(cqlParser.AggregateExpressionTermContext context)
    {
        var value = Visit(context.expression());
        var op = context.GetChild(0).GetText();
        if (op == "flatten" && value is List<object?> list)
        {
            var result = new List<object?>();
            foreach (var item in list)
            {
                if (item is List<object?> inner)
                    result.AddRange(inner);
                else
                    result.Add(item);
            }
            return result;
        }
        if (op == "distinct" && value is List<object?> dList)
        {
            var result = new List<object?>();
            foreach (var item in dList)
            {
                bool found = false;
                foreach (var existing in result)
                    if (CqlComparison.Equal(item, existing)) { found = true; break; }
                if (!found) result.Add(item);
            }
            return result;
        }
        return value;
    }

    // Indexer: expr[index]
    public override object? VisitIndexedExpressionTerm(cqlParser.IndexedExpressionTermContext context)
    {
        var target = Visit(context.expressionTerm());
        var index = Visit(context.expression());
        if (target is List<object?> list && index is int i)
        {
            if (i < 0 || i >= list.Count) return null;
            return list[i];
        }
        if (target is string s && index is int si)
        {
            if (si < 0 || si >= s.Length) return null;
            return s[si].ToString();
        }
        return null;
    }

    // String escaping
    static string UnescapeCqlString(string s)
    {
        var sb = new System.Text.StringBuilder(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '\\' && i + 1 < s.Length)
            {
                i++;
                sb.Append(s[i] switch
                {
                    'n' => '\n',
                    'r' => '\r',
                    't' => '\t',
                    'f' => '\f',
                    '\\' => '\\',
                    '\'' => '\'',
                    '"' => '"',
                    '/' => '/',
                    '`' => '`',
                    'u' when i + 4 < s.Length => (char)int.Parse(s.AsSpan(i + 1, 4),
                        System.Globalization.NumberStyles.HexNumber),
                    _ => s[i],
                });
                if (s[i - 1] == '\\' && i < s.Length && s[i] == 'u')
                    i += 4;
            }
            else
            {
                sb.Append(s[i]);
            }
        }
        return sb.ToString();
    }

    // Parsing helpers

    static CqlDateTime ParseDateTime(string text)
    {
        // format: YYYY-MM-DDThh:mm:ss.fff or partial
        var parts = text.Split('T');
        var datePart = parts[0];
        var timePart = parts.Length > 1 ? parts[1] : null;

        var datePieces = datePart.Split('-');
        int year = int.Parse(datePieces[0]);
        int? month = datePieces.Length > 1 ? int.Parse(datePieces[1]) : null;
        int? day = datePieces.Length > 2 ? int.Parse(datePieces[2]) : null;

        if (string.IsNullOrEmpty(timePart))
            return new CqlDateTime(year, month, day);

        var timePieces = timePart.Split(':');
        int? hour = timePieces.Length > 0 && timePieces[0].Length > 0 ? int.Parse(timePieces[0]) : null;
        int? minute = timePieces.Length > 1 ? int.Parse(timePieces[1]) : null;
        int? second = null;
        int? ms = null;
        if (timePieces.Length > 2)
        {
            var secParts = timePieces[2].Split('.');
            second = int.Parse(secParts[0]);
            if (secParts.Length > 1)
                ms = int.Parse(secParts[1].PadRight(3, '0')[..3]);
        }
        return new CqlDateTime(year, month, day, hour, minute, second, ms);
    }

    static CqlDate ParseDate(string text)
    {
        var pieces = text.Split('-');
        int year = int.Parse(pieces[0]);
        int? month = pieces.Length > 1 ? int.Parse(pieces[1]) : null;
        int? day = pieces.Length > 2 ? int.Parse(pieces[2]) : null;
        return new CqlDate(year, month, day);
    }

    static CqlTime ParseTime(string text)
    {
        var pieces = text.Split(':');
        int hour = int.Parse(pieces[0]);
        int? minute = pieces.Length > 1 ? int.Parse(pieces[1]) : null;
        int? second = null;
        int? ms = null;
        if (pieces.Length > 2)
        {
            var secParts = pieces[2].Split('.');
            second = int.Parse(secParts[0]);
            if (secParts.Length > 1)
                ms = int.Parse(secParts[1].PadRight(3, '0')[..3]);
        }
        return new CqlTime(hour, minute, second, ms);
    }

    // Arithmetic helpers

    static object? Add(object left, object right) => (left, right) switch
    {
        (int a, int b) => a + b,
        (long a, long b) => a + b,
        (int a, long b) => a + b,
        (long a, int b) => a + b,
        (decimal a, decimal b) => a + b,
        (string a, string b) => a + b,
        (CqlQuantity a, CqlQuantity b) when a.Unit == b.Unit => new CqlQuantity(a.Value + b.Value, a.Unit),
        _ => null,
    };

    static object? Subtract(object left, object right) => (left, right) switch
    {
        (int a, int b) => a - b,
        (long a, long b) => a - b,
        (int a, long b) => a - b,
        (long a, int b) => a - b,
        (decimal a, decimal b) => a - b,
        (CqlQuantity a, CqlQuantity b) when a.Unit == b.Unit => new CqlQuantity(a.Value - b.Value, a.Unit),
        _ => null,
    };

    static object? Multiply(object left, object right) => (left, right) switch
    {
        (int a, int b) => a * b,
        (long a, long b) => a * b,
        (decimal a, decimal b) => a * b,
        (decimal a, CqlQuantity b) => new CqlQuantity(a * b.Value, b.Unit),
        (CqlQuantity a, decimal b) => new CqlQuantity(a.Value * b, a.Unit),
        _ => null,
    };

    static object? Divide(object left, object right) => (left, right) switch
    {
        (int a, int b) when b != 0 => (decimal)a / b,
        (long a, long b) when b != 0 => (decimal)a / b,
        (decimal a, decimal b) when b != 0m => a / b,
        (CqlQuantity a, decimal b) when b != 0m => new CqlQuantity(a.Value / b, a.Unit),
        _ => null,
    };

    static object? TruncatedDivide(object left, object right) => (left, right) switch
    {
        (int a, int b) when b != 0 => a / b,
        (long a, long b) when b != 0 => a / b,
        (decimal a, decimal b) when b != 0m => (int)Math.Truncate(a / b),
        _ => null,
    };

    static object? Modulo(object left, object right) => (left, right) switch
    {
        (int a, int b) when b != 0 => a % b,
        (long a, long b) when b != 0 => a % b,
        (decimal a, decimal b) when b != 0m => a % b,
        _ => null,
    };

    static object? Power(object left, object right)
    {
        double Pow(double b, double e) => Math.Pow(b, e);
        return (left, right) switch
        {
            (int a, int b) => (int)Pow(a, b),
            (long a, long b) => (long)Pow(a, b),
            (decimal a, decimal b) => (decimal)Pow((double)a, (double)b),
            (int a, decimal b) => (decimal)Pow(a, (double)b),
            (decimal a, int b) => (decimal)Pow((double)a, b),
            _ => null,
        };
    }

    static object? EqualWithNullPropagation(object? left, object? right)
    {
        if (left is null || right is null) return null;
        if (left is CqlTuple lt && right is CqlTuple rt)
            return CqlTuple.TupleEqual(lt, rt);
        return CqlComparison.Equal(left, right);
    }

    static object? NegateNullable(object? value)
    {
        if (value is null) return null;
        if (value is bool b) return !b;
        return null;
    }

    // Equivalence (differs from equality in null handling)
    static bool Equivalent(object? left, object? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        if (left is string ls && right is string rs)
            return string.Equals(ls, rs, StringComparison.OrdinalIgnoreCase);
        return CqlComparison.Equal(left, right);
    }

    // List helpers

    static List<object?> ToList(object? value)
    {
        if (value is List<object?> list) return list;
        if (value is null) return [];
        return [value];
    }

    static List<object?> Union(List<object?> left, List<object?> right)
    {
        var result = new List<object?>(left);
        foreach (var item in right)
        {
            bool found = false;
            foreach (var existing in result)
                if (CqlComparison.Equal(item, existing)) { found = true; break; }
            if (!found) result.Add(item);
        }
        return result;
    }

    static List<object?> Intersect(List<object?> left, List<object?> right)
    {
        var result = new List<object?>();
        foreach (var item in left)
        {
            foreach (var r in right)
            {
                if (CqlComparison.Equal(item, r))
                {
                    bool found = false;
                    foreach (var existing in result)
                        if (CqlComparison.Equal(item, existing)) { found = true; break; }
                    if (!found) result.Add(item);
                    break;
                }
            }
        }
        return result;
    }

    static List<object?> Except(List<object?> left, List<object?> right)
    {
        var result = new List<object?>();
        foreach (var item in left)
        {
            bool found = false;
            foreach (var r in right)
                if (CqlComparison.Equal(item, r)) { found = true; break; }
            if (!found) result.Add(item);
        }
        return result;
    }

    // Duration between
    static object? DurationBetween(object? left, object? right, string precision)
    {
        if (left is null || right is null) return null;

        if (left is CqlDateTime ldt && right is CqlDateTime rdt)
        {
            try
            {
                var ld = ToSystemDateTime(ldt);
                var rd = ToSystemDateTime(rdt);
                return precision switch
                {
                    "years" => rd.Year - ld.Year - (rd.DayOfYear < ld.DayOfYear ? 1 : 0),
                    "months" => (rd.Year - ld.Year) * 12 + rd.Month - ld.Month - (rd.Day < ld.Day ? 1 : 0),
                    "weeks" => (int)(rd - ld).TotalDays / 7,
                    "days" => (int)(rd - ld).TotalDays,
                    "hours" => (int)(rd - ld).TotalHours,
                    "minutes" => (int)(rd - ld).TotalMinutes,
                    "seconds" => (int)(rd - ld).TotalSeconds,
                    "milliseconds" => (int)(rd - ld).TotalMilliseconds,
                    _ => null,
                };
            }
            catch { return null; }
        }

        return null;
    }

    static System.DateTime ToSystemDateTime(CqlDateTime dt)
        => new(dt.Year, dt.Month ?? 1, dt.Day ?? 1, dt.Hour ?? 0, dt.Minute ?? 0, dt.Second ?? 0, dt.Millisecond ?? 0);

    // Component extraction
    static object? ExtractComponent(object? value, string component)
    {
        if (value is null) return null;
        return (value, component) switch
        {
            (CqlDateTime dt, "year") => dt.Year,
            (CqlDateTime dt, "month") => dt.Month,
            (CqlDateTime dt, "day") => dt.Day,
            (CqlDateTime dt, "hour") => dt.Hour,
            (CqlDateTime dt, "minute") => dt.Minute,
            (CqlDateTime dt, "second") => dt.Second,
            (CqlDateTime dt, "millisecond") => dt.Millisecond,
            (CqlDateTime dt, "date") => new CqlDate(dt.Year, dt.Month, dt.Day),
            (CqlDateTime dt, "time") => dt.Hour is not null ? new CqlTime(dt.Hour.Value, dt.Minute, dt.Second, dt.Millisecond) : null,
            (CqlDate d, "year") => d.Year,
            (CqlDate d, "month") => d.Month,
            (CqlDate d, "day") => d.Day,
            (CqlTime t, "hour") => t.Hour,
            (CqlTime t, "minute") => t.Minute,
            (CqlTime t, "second") => t.Second,
            (CqlTime t, "millisecond") => t.Millisecond,
            _ => null,
        };
    }

    // Built-in functions
    object? EvalFunction(string name, object?[] args)
    {
        return name switch
        {
            "Abs" => Abs(args),
            "Ceiling" => Ceiling(args),
            "Floor" => Floor(args),
            "Truncate" => Truncate(args),
            "Round" => Round(args),
            "Ln" => Ln(args),
            "Log" => Log(args),
            "Exp" => Exp(args),
            "Power" => args.Length == 2 && args[0] is not null && args[1] is not null ? Power(args[0]!, args[1]!) : null,
            "Successor" => Successor(args),
            "Predecessor" => Predecessor(args),
            "MinValue" => MinValue(args),
            "MaxValue" => MaxValue(args),
            "DateTime" => DateTimeFunc(args),
            "Date" => DateFunc(args),
            "Time" => TimeFunc(args),
            "Now" => new CqlDateTime(2025, 1, 1, 0, 0, 0, 0),
            "Today" => new CqlDate(2025, 1, 1),
            "TimeOfDay" => new CqlTime(0, 0, 0, 0),
            "Length" => LengthFunc(args),
            "Upper" => args.Length == 1 && args[0] is string s ? s.ToUpperInvariant() : null,
            "Lower" => args.Length == 1 && args[0] is string s2 ? s2.ToLowerInvariant() : null,
            "Combine" => CombineFunc(args),
            "Split" => SplitFunc(args),
            "Substring" => SubstringFunc(args),
            "StartsWith" => args.Length == 2 && args[0] is string a && args[1] is string b ? a.StartsWith(b, StringComparison.Ordinal) : null,
            "EndsWith" => args.Length == 2 && args[0] is string c && args[1] is string d ? c.EndsWith(d, StringComparison.Ordinal) : null,
            "Matches" => args.Length == 2 && args[0] is string e && args[1] is string f
                ? System.Text.RegularExpressions.Regex.IsMatch(e, $"^{f}$") : null,
            "ReplaceMatches" => args.Length == 3 && args[0] is string g && args[1] is string h && args[2] is string i
                ? System.Text.RegularExpressions.Regex.Replace(g, h, i) : null,
            "PositionOf" => PositionOfFunc(args),
            "LastPositionOf" => LastPositionOfFunc(args),
            "ToString" => ToStringFunc(args),
            "ToBoolean" => ToBooleanFunc(args),
            "ToInteger" => ToIntegerFunc(args),
            "ToDecimal" => ToDecimalFunc(args),
            "ToQuantity" => ToQuantityFunc(args),
            "ToDateTime" => ToDateTimeFunc(args),
            "ToDate" => ToDateFunc(args),
            "ToTime" => ToTimeFunc(args),
            "Coalesce" => args.FirstOrDefault(a => a is not null),
            "Count" => args.Length == 1 && args[0] is List<object?> list ? list.Count(x => x is not null) : 0,
            "Sum" => SumFunc(args),
            "Min" => MinFunc(args),
            "Max" => MaxFunc(args),
            "Avg" => AvgFunc(args),
            "First" => args.Length == 1 && args[0] is List<object?> fl ? (fl.Count > 0 ? fl[0] : null) : null,
            "Last" => args.Length == 1 && args[0] is List<object?> ll ? (ll.Count > 0 ? ll[^1] : null) : null,
            "IndexOf" => IndexOfFunc(args),
            "Distinct" => DistinctFunc(args),
            "Flatten" => FlattenFunc(args),
            "SingletonFrom" => args.Length == 1 && args[0] is List<object?> sl ? (sl.Count == 1 ? sl[0] : null) : args[0],
            "AllTrue" => args.Length == 1 && args[0] is List<object?> atl ? atl.All(x => x is true) : null,
            "AnyTrue" => args.Length == 1 && args[0] is List<object?> anl ? anl.Any(x => x is true) : null,
            "Exists" => args.Length == 1 && args[0] is List<object?> el ? el.Count > 0 : args[0] is not null,
            "IsNull" => args.Length == 1 && args[0] is null,
            "IsTrue" => args.Length == 1 && args[0] is true,
            "IsFalse" => args.Length == 1 && args[0] is false,
            "Negate" => args.Length == 1 ? Negate(args[0]) : null,
            "LowBoundary" => LowBoundary(args),
            "HighBoundary" => HighBoundary(args),
            "Precision" => PrecisionFunc(args),
            "Size" => args.Length == 1 && args[0] is CqlInterval iv ? Subtract(iv.High!, iv.Low!) : null,
            "Null" => null,
            _ => throw new NotSupportedException($"Unknown function: {name}")
        };
    }

    object? EvalMethodOnTarget(object? target, string name, object?[] args)
    {
        var allArgs = new object?[] { target }.Concat(args).ToArray();
        return name switch
        {
            "toString" or "ToString" => ToStringFunc([target]),
            "toInteger" or "ToInteger" => ToIntegerFunc([target]),
            "toDecimal" or "ToDecimal" => ToDecimalFunc([target]),
            "toBoolean" or "ToBoolean" => ToBooleanFunc([target]),
            "toDateTime" or "ToDateTime" => ToDateTimeFunc([target]),
            "toDate" or "ToDate" => ToDateFunc([target]),
            "toTime" or "ToTime" => ToTimeFunc([target]),
            "startsWith" or "StartsWith" => target is string s && args.Length == 1 && args[0] is string p
                ? s.StartsWith(p, StringComparison.Ordinal) : null,
            "endsWith" or "EndsWith" => target is string s2 && args.Length == 1 && args[0] is string p2
                ? s2.EndsWith(p2, StringComparison.Ordinal) : null,
            "contains" => target is string s3 && args.Length == 1 && args[0] is string p3
                ? s3.Contains(p3, StringComparison.Ordinal) : null,
            "length" => target is string s4 ? s4.Length : target is List<object?> l ? l.Count : null,
            "upper" => target is string s5 ? s5.ToUpperInvariant() : null,
            "lower" => target is string s6 ? s6.ToLowerInvariant() : null,
            "matches" => target is string s7 && args.Length == 1 && args[0] is string r
                ? System.Text.RegularExpressions.Regex.IsMatch(s7, $"^{r}$") : null,
            "replaceMatches" => target is string s8 && args.Length == 2 && args[0] is string r2 && args[1] is string rep
                ? System.Text.RegularExpressions.Regex.Replace(s8, r2, rep) : null,
            "substring" => SubstringFunc(allArgs),
            "indexOf" => target is string si && args.Length == 1 && args[0] is string pat
                ? si.IndexOf(pat, StringComparison.Ordinal) : null,
            "lastIndexOf" => target is string sli && args.Length == 1 && args[0] is string pat2
                ? sli.LastIndexOf(pat2, StringComparison.Ordinal) : null,
            "abs" or "Abs" => Abs([target]),
            "ceiling" or "Ceiling" => Ceiling([target]),
            "floor" or "Floor" => Floor([target]),
            "truncate" or "Truncate" => Truncate([target]),
            "round" or "Round" => Round(allArgs),
            "ln" or "Ln" => Ln([target]),
            "exp" or "Exp" => Exp([target]),
            "successor" or "Successor" => Successor([target]),
            "predecessor" or "Predecessor" => Predecessor([target]),
            "first" => target is List<object?> fl ? (fl.Count > 0 ? fl[0] : null) : null,
            "last" => target is List<object?> ll ? (ll.Count > 0 ? ll[^1] : null) : null,
            "count" => target is List<object?> cl ? cl.Count(x => x is not null) : 0,
            "distinct" => DistinctFunc([target]),
            "flatten" => FlattenFunc([target]),
            "exists" => target is List<object?> el ? el.Count > 0 : target is not null,
            "where" or "select" or "all" or "repeat" or "aggregate" or
            "ofType" or "single" or "skip" or "take" or "tail" or
            "convertsToBoolean" or "convertsToInteger" or "convertsToDecimal" or
            "convertsToString" or "convertsToDateTime" or "convertsToDate" or
            "convertsToTime" or "convertsToQuantity"
                => throw new NotSupportedException($"Method '{name}' not yet implemented"),
            _ => EvalFunction(name, allArgs),
        };
    }

    // Individual function implementations

    static object? Abs(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        return args[0] switch
        {
            int i => Math.Abs(i),
            long l => Math.Abs(l),
            decimal d => Math.Abs(d),
            CqlQuantity q => new CqlQuantity(Math.Abs(q.Value), q.Unit),
            _ => null,
        };
    }

    static object? Ceiling(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        return args[0] switch
        {
            int i => i,
            decimal d => (int)Math.Ceiling(d),
            _ => null,
        };
    }

    static object? Floor(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        return args[0] switch
        {
            int i => i,
            decimal d => (int)Math.Floor(d),
            _ => null,
        };
    }

    static object? Truncate(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        return args[0] switch
        {
            int i => i,
            decimal d => (int)Math.Truncate(d),
            _ => null,
        };
    }

    static object? Round(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        int precision = args.Length > 1 && args[1] is int p ? p : 0;
        return args[0] switch
        {
            decimal d => Math.Round(d, precision, MidpointRounding.AwayFromZero),
            int i => i,
            _ => null,
        };
    }

    static object? Ln(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        return args[0] switch
        {
            decimal d when d > 0 => (decimal)Math.Log((double)d),
            int i when i > 0 => (decimal)Math.Log(i),
            _ => null,
        };
    }

    static object? Log(object?[] args)
    {
        if (args.Length < 2 || args[0] is null || args[1] is null) return null;
        double val = args[0] switch { decimal d => (double)d, int i => i, _ => double.NaN };
        double @base = args[1] switch { decimal d => (double)d, int i => i, _ => double.NaN };
        if (double.IsNaN(val) || double.IsNaN(@base) || val <= 0 || @base <= 0) return null;
        return (decimal)Math.Log(val, @base);
    }

    static object? Exp(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        double val = args[0] switch { decimal d => (double)d, int i => i, _ => double.NaN };
        if (double.IsNaN(val)) return null;
        return (decimal)Math.Exp(val);
    }

    static object? Successor(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        return args[0] switch
        {
            int i => i + 1,
            long l => l + 1,
            decimal d => d + 0.00000001m,
            _ => null,
        };
    }

    static object? Predecessor(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        return args[0] switch
        {
            int i => i - 1,
            long l => l - 1,
            decimal d => d - 0.00000001m,
            _ => null,
        };
    }

    static object? MinValue(object?[] args)
    {
        if (args.Length < 1 || args[0] is not string typeName) return null;
        return typeName switch
        {
            "Integer" => int.MinValue,
            "Long" => long.MinValue,
            "Decimal" => -9999999999999999999999999999.99999999m,
            "DateTime" => new CqlDateTime(1, 1, 1, 0, 0, 0, 0),
            "Date" => new CqlDate(1, 1, 1),
            "Time" => new CqlTime(0, 0, 0, 0),
            _ => null,
        };
    }

    static object? MaxValue(object?[] args)
    {
        if (args.Length < 1 || args[0] is not string typeName) return null;
        return typeName switch
        {
            "Integer" => int.MaxValue,
            "Long" => long.MaxValue,
            "Decimal" => 9999999999999999999999999999.99999999m,
            "DateTime" => new CqlDateTime(9999, 12, 31, 23, 59, 59, 999),
            "Date" => new CqlDate(9999, 12, 31),
            "Time" => new CqlTime(23, 59, 59, 999),
            _ => null,
        };
    }

    static object? DateTimeFunc(object?[] args)
    {
        if (args.Length < 1) return null;
        if (args[0] is not int year) return null;
        int? month = args.Length > 1 ? args[1] as int? : null;
        int? day = args.Length > 2 ? args[2] as int? : null;
        int? hour = args.Length > 3 ? args[3] as int? : null;
        int? minute = args.Length > 4 ? args[4] as int? : null;
        int? second = args.Length > 5 ? args[5] as int? : null;
        int? ms = args.Length > 6 ? args[6] as int? : null;
        // args[7] would be timezone offset — ignored for now
        return new CqlDateTime(year, month, day, hour, minute, second, ms);
    }

    static object? DateFunc(object?[] args)
    {
        if (args.Length < 1 || args[0] is not int year) return null;
        int? month = args.Length > 1 ? args[1] as int? : null;
        int? day = args.Length > 2 ? args[2] as int? : null;
        return new CqlDate(year, month, day);
    }

    static object? TimeFunc(object?[] args)
    {
        if (args.Length < 1 || args[0] is not int hour) return null;
        int? minute = args.Length > 1 ? args[1] as int? : null;
        int? second = args.Length > 2 ? args[2] as int? : null;
        int? ms = args.Length > 3 ? args[3] as int? : null;
        return new CqlTime(hour, minute, second, ms);
    }

    static object? LengthFunc(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        return args[0] switch
        {
            string s => s.Length,
            List<object?> l => l.Count,
            _ => null,
        };
    }

    static object? CombineFunc(object?[] args)
    {
        if (args.Length < 1 || args[0] is not List<object?> list) return null;
        var sep = args.Length > 1 ? args[1] as string ?? "" : "";
        var strings = new List<string>();
        foreach (var item in list)
        {
            if (item is null) return null;
            strings.Add(item.ToString()!);
        }
        return string.Join(sep, strings);
    }

    static object? SplitFunc(object?[] args)
    {
        if (args.Length < 2 || args[0] is not string str || args[1] is not string sep) return null;
        return str.Split(sep).Select(s => (object?)s).ToList();
    }

    static object? SubstringFunc(object?[] args)
    {
        if (args.Length < 2) return null;
        if (args[0] is not string str) return null;
        if (args[1] is not int start) return null;
        if (start < 0 || start >= str.Length) return null;
        int length = args.Length > 2 && args[2] is int len ? len : str.Length - start;
        length = Math.Min(length, str.Length - start);
        return str.Substring(start, length);
    }

    static object? PositionOfFunc(object?[] args)
    {
        if (args.Length < 2 || args[0] is not string pattern || args[1] is not string str) return null;
        return str.IndexOf(pattern, StringComparison.Ordinal);
    }

    static object? LastPositionOfFunc(object?[] args)
    {
        if (args.Length < 2 || args[0] is not string pattern || args[1] is not string str) return null;
        return str.LastIndexOf(pattern, StringComparison.Ordinal);
    }

    static object? ToStringFunc(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        return args[0] switch
        {
            string s => s,
            int i => i.ToString(),
            long l => l.ToString(),
            decimal d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
            bool b => b ? "true" : "false",
            CqlDateTime dt => dt.ToString(),
            CqlDate d => d.ToString(),
            CqlTime t => t.ToString(),
            CqlQuantity q => q.ToString(),
            _ => args[0]!.ToString(),
        };
    }

    static object? ToBooleanFunc(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        return args[0] switch
        {
            bool b => b,
            string s => s.ToLowerInvariant() switch
            {
                "true" or "t" or "yes" or "y" or "1" => (object)true,
                "false" or "f" or "no" or "n" or "0" => false,
                _ => null,
            },
            int i => i switch { 1 => (object)true, 0 => false, _ => null },
            decimal d => d switch { 1.0m => (object)true, 0.0m => false, _ => null },
            _ => null,
        };
    }

    static object? ToIntegerFunc(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        return args[0] switch
        {
            int i => i,
            long l when l >= int.MinValue && l <= int.MaxValue => (int)l,
            string s when int.TryParse(s, out var i) => i,
            bool b => b ? 1 : 0,
            _ => null,
        };
    }

    static object? ToDecimalFunc(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        return args[0] switch
        {
            decimal d => d,
            int i => (decimal)i,
            long l => (decimal)l,
            string s when decimal.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, out var d) => d,
            _ => null,
        };
    }

    static object? ToQuantityFunc(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        return args[0] switch
        {
            CqlQuantity q => q,
            int i => new CqlQuantity(i, "1"),
            decimal d => new CqlQuantity(d, "1"),
            _ => null,
        };
    }

    static object? ToDateTimeFunc(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        return args[0] switch
        {
            CqlDateTime dt => dt,
            CqlDate d => new CqlDateTime(d.Year, d.Month, d.Day),
            string s when s.Length >= 4 => ParseDateTime(s.TrimStart('@')),
            _ => null,
        };
    }

    static object? ToDateFunc(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        return args[0] switch
        {
            CqlDate d => d,
            CqlDateTime dt => new CqlDate(dt.Year, dt.Month, dt.Day),
            string s when s.Length >= 4 => ParseDate(s.TrimStart('@')),
            _ => null,
        };
    }

    static object? ToTimeFunc(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        return args[0] switch
        {
            CqlTime t => t,
            string s when s.StartsWith("T") => ParseTime(s[1..]),
            string s when s.Contains(':') => ParseTime(s),
            _ => null,
        };
    }

    static object? SumFunc(object?[] args)
    {
        if (args.Length < 1 || args[0] is not List<object?> list) return null;
        object? result = null;
        foreach (var item in list)
        {
            if (item is null) continue;
            if (result is null) { result = item; continue; }
            result = Add(result, item);
        }
        return result;
    }

    static object? MinFunc(object?[] args)
    {
        if (args.Length < 1 || args[0] is not List<object?> list) return null;
        object? result = null;
        foreach (var item in list)
        {
            if (item is null) continue;
            if (result is null || CqlComparison.Compare(item, result) < 0) result = item;
        }
        return result;
    }

    static object? MaxFunc(object?[] args)
    {
        if (args.Length < 1 || args[0] is not List<object?> list) return null;
        object? result = null;
        foreach (var item in list)
        {
            if (item is null) continue;
            if (result is null || CqlComparison.Compare(item, result) > 0) result = item;
        }
        return result;
    }

    static object? AvgFunc(object?[] args)
    {
        if (args.Length < 1 || args[0] is not List<object?> list) return null;
        var nonNull = list.Where(x => x is not null).ToList();
        if (nonNull.Count == 0) return null;
        var sum = SumFunc([nonNull.Cast<object?>().ToList()]);
        if (sum is null) return null;
        return Divide(sum, nonNull.Count);
    }

    static object? IndexOfFunc(object?[] args)
    {
        if (args.Length < 2 || args[0] is not List<object?> list) return null;
        for (int i = 0; i < list.Count; i++)
            if (CqlComparison.Equal(list[i], args[1])) return i;
        return -1;
    }

    static object? DistinctFunc(object?[] args)
    {
        if (args.Length < 1 || args[0] is not List<object?> list) return args.Length > 0 ? args[0] : null;
        var result = new List<object?>();
        foreach (var item in list)
        {
            bool found = false;
            foreach (var existing in result)
                if (CqlComparison.Equal(item, existing)) { found = true; break; }
            if (!found) result.Add(item);
        }
        return result;
    }

    static object? FlattenFunc(object?[] args)
    {
        if (args.Length < 1 || args[0] is not List<object?> list) return args.Length > 0 ? args[0] : null;
        var result = new List<object?>();
        foreach (var item in list)
        {
            if (item is List<object?> inner) result.AddRange(inner);
            else result.Add(item);
        }
        return result;
    }

    // MinValue/MaxValue as expressionTerm: 'minimum'/'maximum' namedTypeSpecifier
    public override object? VisitTypeExtentExpressionTerm(cqlParser.TypeExtentExpressionTermContext context)
    {
        var typeName = context.namedTypeSpecifier().GetText();
        var isMin = context.GetChild(0).GetText() == "minimum";
        return isMin ? MinValue([typeName]) : MaxValue([typeName]);
    }

    // Successor/Predecessor of
    public override object? VisitSuccessorExpressionTerm(cqlParser.SuccessorExpressionTermContext context)
        => Successor([Visit(context.expressionTerm())]);

    public override object? VisitPredecessorExpressionTerm(cqlParser.PredecessorExpressionTermContext context)
        => Predecessor([Visit(context.expressionTerm())]);

    // Start/End of interval
    public override object? VisitTimeBoundaryExpressionTerm(cqlParser.TimeBoundaryExpressionTermContext context)
    {
        var value = Visit(context.expressionTerm());
        if (value is not CqlInterval interval) return null;
        return context.GetChild(0).GetText() == "start" ? interval.Low : interval.High;
    }

    // Width of interval
    public override object? VisitWidthExpressionTerm(cqlParser.WidthExpressionTermContext context)
    {
        var value = Visit(context.expressionTerm());
        if (value is not CqlInterval interval) return null;
        if (interval.Low is null || interval.High is null) return null;
        return Subtract(interval.High, interval.Low);
    }

    // Singleton from
    public override object? VisitElementExtractorExpressionTerm(cqlParser.ElementExtractorExpressionTermContext context)
    {
        var value = Visit(context.expressionTerm());
        if (value is List<object?> list)
            return list.Count == 1 ? list[0] : null;
        return value;
    }

    // Convert expression
    public override object? VisitConversionExpressionTerm(cqlParser.ConversionExpressionTermContext context)
    {
        var value = Visit(context.expression());
        var typeSpec = context.typeSpecifier();
        if (typeSpec is not null)
        {
            var typeName = typeSpec.GetText();
            return typeName switch
            {
                "Integer" => ToIntegerFunc([value]),
                "Long" => value is int i ? (long)i : value is string s && long.TryParse(s, out var l) ? l : null,
                "Decimal" => ToDecimalFunc([value]),
                "Boolean" => ToBooleanFunc([value]),
                "String" => ToStringFunc([value]),
                "DateTime" => ToDateTimeFunc([value]),
                "Date" => ToDateFunc([value]),
                "Time" => ToTimeFunc([value]),
                "Quantity" => ToQuantityFunc([value]),
                _ => null,
            };
        }
        return value;
    }

    static object? Negate(object? value) => value switch
    {
        null => null,
        int i => -i,
        long l => -l,
        decimal d => -d,
        CqlQuantity q => new CqlQuantity(-q.Value, q.Unit),
        _ => null,
    };

    static object? LowBoundary(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        int? precision = args.Length > 1 ? args[1] as int? : null;
        return args[0] switch
        {
            decimal d => d, // simplified — would need proper precision handling
            CqlDateTime dt => new CqlDateTime(dt.Year, dt.Month ?? 1, dt.Day ?? 1,
                dt.Hour ?? 0, dt.Minute ?? 0, dt.Second ?? 0, dt.Millisecond ?? 0),
            CqlDate d => new CqlDate(d.Year, d.Month ?? 1, d.Day ?? 1),
            CqlTime t => new CqlTime(t.Hour, t.Minute ?? 0, t.Second ?? 0, t.Millisecond ?? 0),
            _ => null,
        };
    }

    static object? HighBoundary(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        int? precision = args.Length > 1 ? args[1] as int? : null;
        return args[0] switch
        {
            decimal d => d, // simplified
            CqlDateTime dt => new CqlDateTime(dt.Year, dt.Month ?? 12,
                dt.Day ?? DaysInMonth(dt.Year, dt.Month ?? 12),
                dt.Hour ?? 23, dt.Minute ?? 59, dt.Second ?? 59, dt.Millisecond ?? 999),
            CqlDate d => new CqlDate(d.Year, d.Month ?? 12,
                d.Day ?? DaysInMonth(d.Year, d.Month ?? 12)),
            CqlTime t => new CqlTime(t.Hour, t.Minute ?? 59, t.Second ?? 59, t.Millisecond ?? 999),
            _ => null,
        };
    }

    static int DaysInMonth(int year, int month)
    {
        try { return System.DateTime.DaysInMonth(year, month); }
        catch { return 28; }
    }

    static object? PrecisionFunc(object?[] args)
    {
        if (args.Length < 1 || args[0] is null) return null;
        return args[0] switch
        {
            decimal d => CountDecimalPlaces(d),
            CqlDateTime dt => dt.Millisecond is not null ? 8
                : dt.Second is not null ? 5 : dt.Minute is not null ? 4
                : dt.Hour is not null ? 3 : dt.Day is not null ? 2
                : dt.Month is not null ? 1 : 0,
            CqlDate d => d.Day is not null ? 2 : d.Month is not null ? 1 : 0,
            CqlTime t => t.Millisecond is not null ? 4
                : t.Second is not null ? 3 : t.Minute is not null ? 2 : 1,
            _ => null,
        };
    }

    static int CountDecimalPlaces(decimal d)
    {
        var s = d.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var dot = s.IndexOf('.');
        return dot < 0 ? 0 : s.Length - dot - 1;
    }
}

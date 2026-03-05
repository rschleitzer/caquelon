using Antlr4.Runtime;
using Fire.Cql.Grammar;
using Elm = Fire.Cql.Elm;

namespace Fire.Cql;

public class CqlToElmVisitor : cqlBaseVisitor<Elm.Expression>
{
    static readonly System.Xml.XmlQualifiedName BoolType = new("Boolean", "urn:hl7-org:elm-types:r1");
    static readonly System.Xml.XmlQualifiedName IntType = new("Integer", "urn:hl7-org:elm-types:r1");
    static readonly System.Xml.XmlQualifiedName LongType = new("Long", "urn:hl7-org:elm-types:r1");
    static readonly System.Xml.XmlQualifiedName DecimalType = new("Decimal", "urn:hl7-org:elm-types:r1");
    static readonly System.Xml.XmlQualifiedName StringType = new("String", "urn:hl7-org:elm-types:r1");

    public static Elm.Expression Parse(string expression)
    {
        var input = new AntlrInputStream(expression);
        var lexer = new cqlLexer(input);
        var tokens = new CommonTokenStream(lexer);
        var parser = new cqlParser(tokens);
        parser.RemoveErrorListeners();
        var tree = parser.expression();
        return new CqlToElmVisitor().Visit(tree);
    }

    // Literals

    public override Elm.Expression VisitBooleanLiteral(cqlParser.BooleanLiteralContext context)
        => new Elm.Literal { ValueType = BoolType, Value = context.GetText() };

    public override Elm.Expression VisitNullLiteral(cqlParser.NullLiteralContext context)
        => new Elm.Null();

    public override Elm.Expression VisitStringLiteral(cqlParser.StringLiteralContext context)
    {
        var raw = context.STRING().GetText();
        return new Elm.Literal { ValueType = StringType, Value = UnescapeCqlString(raw[1..^1]) };
    }

    public override Elm.Expression VisitNumberLiteral(cqlParser.NumberLiteralContext context)
    {
        var text = context.NUMBER().GetText();
        if (text.Contains('.'))
            return new Elm.Literal { ValueType = DecimalType, Value = text };
        return new Elm.Literal { ValueType = IntType, Value = text };
    }

    public override Elm.Expression VisitLongNumberLiteral(cqlParser.LongNumberLiteralContext context)
        => new Elm.Literal { ValueType = LongType, Value = context.LONGNUMBER().GetText()[..^1] };

    public override Elm.Expression VisitQuantityLiteral(cqlParser.QuantityLiteralContext context)
    {
        var q = context.quantity();
        var value = new Elm.Literal { ValueType = DecimalType, Value = q.NUMBER().GetText() };
        var unit = q.unit();
        var unitStr = unit is null ? "1" : unit.STRING()?.GetText() is string s ? s[1..^1] : unit.GetText();
        return new Elm.Quantity { Value = decimal.Parse(q.NUMBER().GetText(), System.Globalization.CultureInfo.InvariantCulture), Unit = unitStr };
    }

    public override Elm.Expression VisitDateTimeLiteral(cqlParser.DateTimeLiteralContext context)
    {
        var text = context.DATETIME().GetText()[1..]; // skip @
        return ParseDateTimeToElm(text);
    }

    public override Elm.Expression VisitDateLiteral(cqlParser.DateLiteralContext context)
    {
        var text = context.DATE().GetText()[1..]; // skip @
        return ParseDateToElm(text);
    }

    public override Elm.Expression VisitTimeLiteral(cqlParser.TimeLiteralContext context)
    {
        var text = context.TIME().GetText()[2..]; // skip @T
        return ParseTimeToElm(text);
    }

    // Selectors

    public override Elm.Expression VisitIntervalSelectorTerm(cqlParser.IntervalSelectorTermContext context)
    {
        var sel = context.intervalSelector();
        return new Elm.Interval
        {
            Low = Visit(sel.expression(0)),
            High = Visit(sel.expression(1)),
            LowClosed = sel.GetChild(1).GetText() == "[",
            HighClosed = sel.GetChild(sel.ChildCount - 1).GetText() == "]",
        };
    }

    public override Elm.Expression VisitListSelectorTerm(cqlParser.ListSelectorTermContext context)
    {
        var sel = context.listSelector();
        var list = new Elm.List();
        foreach (var expr in sel.expression())
            list.Element.Add(Visit(expr));
        return list;
    }

    public override Elm.Expression VisitTupleSelectorTerm(cqlParser.TupleSelectorTermContext context)
    {
        var sel = context.tupleSelector();
        var tuple = new Elm.Tuple();
        foreach (var elem in sel.tupleElementSelector())
            tuple.Element.Add(new Elm.TupleElement
            {
                Name = elem.referentialIdentifier().GetText(),
                Value = Visit(elem.expression()),
            });
        return tuple;
    }

    public override Elm.Expression VisitInstanceSelectorTerm(cqlParser.InstanceSelectorTermContext context)
    {
        var sel = context.instanceSelector();
        var typeName = sel.namedTypeSpecifier().GetText();
        var inst = new Elm.Instance
        {
            ClassType = new System.Xml.XmlQualifiedName(typeName, "urn:hl7-org:elm-types:r1"),
        };
        foreach (var elem in sel.instanceElementSelector())
            inst.Element.Add(new Elm.InstanceElement
            {
                Name = elem.referentialIdentifier().GetText(),
                Value = Visit(elem.expression()),
            });
        return inst;
    }

    public override Elm.Expression VisitParenthesizedTerm(cqlParser.ParenthesizedTermContext context)
        => Visit(context.expression());

    // Arithmetic

    public override Elm.Expression VisitAdditionExpressionTerm(cqlParser.AdditionExpressionTermContext context)
    {
        var left = Visit(context.expressionTerm(0));
        var right = Visit(context.expressionTerm(1));
        var op = context.GetChild(1).GetText();
        return op switch
        {
            "+" => new Elm.Add { Operand = { left, right } },
            "-" => new Elm.Subtract { Operand = { left, right } },
            "&" => new Elm.Concatenate { Operand = { left, right } },
            _ => throw new NotSupportedException($"Unknown addition operator: {op}")
        };
    }

    public override Elm.Expression VisitMultiplicationExpressionTerm(cqlParser.MultiplicationExpressionTermContext context)
    {
        var left = Visit(context.expressionTerm(0));
        var right = Visit(context.expressionTerm(1));
        var op = context.GetChild(1).GetText();
        return op switch
        {
            "*" => new Elm.Multiply { Operand = { left, right } },
            "/" => new Elm.Divide { Operand = { left, right } },
            "div" => new Elm.TruncatedDivide { Operand = { left, right } },
            "mod" => new Elm.Modulo { Operand = { left, right } },
            _ => throw new NotSupportedException($"Unknown multiplication operator: {op}")
        };
    }

    public override Elm.Expression VisitPowerExpressionTerm(cqlParser.PowerExpressionTermContext context)
        => new Elm.Power { Operand = { Visit(context.expressionTerm(0)), Visit(context.expressionTerm(1)) } };

    public override Elm.Expression VisitPolarityExpressionTerm(cqlParser.PolarityExpressionTermContext context)
    {
        var operand = Visit(context.expressionTerm());
        return context.GetChild(0).GetText() == "+"
            ? operand
            : new Elm.Negate { Operand = operand };
    }

    // Comparison

    public override Elm.Expression VisitEqualityExpression(cqlParser.EqualityExpressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));
        var op = context.GetChild(1).GetText();
        return op switch
        {
            "=" => new Elm.Equal { Operand = { left, right } },
            "!=" => new Elm.NotEqual { Operand = { left, right } },
            "~" => new Elm.Equivalent { Operand = { left, right } },
            "!~" => new Elm.Not { Operand = new Elm.Equivalent { Operand = { left, right } } },
            _ => throw new NotSupportedException($"Unknown equality operator: {op}")
        };
    }

    public override Elm.Expression VisitInequalityExpression(cqlParser.InequalityExpressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));
        var op = context.GetChild(1).GetText();
        return op switch
        {
            "<" => new Elm.Less { Operand = { left, right } },
            ">" => new Elm.Greater { Operand = { left, right } },
            "<=" => new Elm.LessOrEqual { Operand = { left, right } },
            ">=" => new Elm.GreaterOrEqual { Operand = { left, right } },
            _ => throw new NotSupportedException($"Unknown inequality operator: {op}")
        };
    }

    // Boolean / Logical

    public override Elm.Expression VisitAndExpression(cqlParser.AndExpressionContext context)
        => new Elm.And { Operand = { Visit(context.expression(0)), Visit(context.expression(1)) } };

    public override Elm.Expression VisitOrExpression(cqlParser.OrExpressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));
        return context.GetChild(1).GetText() == "xor"
            ? new Elm.Xor { Operand = { left, right } }
            : new Elm.Or { Operand = { left, right } };
    }

    public override Elm.Expression VisitNotExpression(cqlParser.NotExpressionContext context)
        => new Elm.Not { Operand = Visit(context.expression()) };

    public override Elm.Expression VisitImpliesExpression(cqlParser.ImpliesExpressionContext context)
        => new Elm.Implies { Operand = { Visit(context.expression(0)), Visit(context.expression(1)) } };

    // Boolean expression: is null / is not null / is true / is false
    public override Elm.Expression VisitBooleanExpression(cqlParser.BooleanExpressionContext context)
    {
        var operand = Visit(context.expression());
        var children = Enumerable.Range(1, context.ChildCount - 1)
            .Select(i => context.GetChild(i).GetText()).ToList();
        bool negate = children.Contains("not");
        var keyword = children.Last();

        Elm.Expression result = keyword switch
        {
            "null" => new Elm.IsNull { Operand = operand },
            "true" => new Elm.IsTrue { Operand = operand },
            "false" => new Elm.IsFalse { Operand = operand },
            _ => new Elm.IsNull { Operand = operand },
        };
        return negate ? new Elm.Not { Operand = result } : result;
    }

    // Type expression: is / as
    public override Elm.Expression VisitCastExpression(cqlParser.CastExpressionContext context)
    {
        var operand = Visit(context.expression());
        var typeName = context.typeSpecifier().GetText();
        var typeQn = new System.Xml.XmlQualifiedName(typeName, "urn:hl7-org:elm-types:r1");
        return new Elm.As { Operand = operand, AsTypeSpecifier = new Elm.NamedTypeSpecifier { Name = typeQn }, Strict = true };
    }

    public override Elm.Expression VisitTypeExpression(cqlParser.TypeExpressionContext context)
    {
        var operand = Visit(context.expression());
        var typeName = context.typeSpecifier().GetText();
        var typeQn = new System.Xml.XmlQualifiedName(typeName, "urn:hl7-org:elm-types:r1");
        var isOp = context.GetChild(1).GetText() == "is";
        if (isOp)
            return new Elm.Is { Operand = operand, IsTypeSpecifier = new Elm.NamedTypeSpecifier { Name = typeQn } };
        return new Elm.As { Operand = operand, AsTypeSpecifier = new Elm.NamedTypeSpecifier { Name = typeQn } };
    }

    // If-then-else
    public override Elm.Expression VisitIfThenElseExpressionTerm(cqlParser.IfThenElseExpressionTermContext context)
        => new Elm.If
        {
            Condition = Visit(context.expression(0)),
            Then = Visit(context.expression(1)),
            Else = Visit(context.expression(2)),
        };

    // Case
    public override Elm.Expression VisitCaseExpressionTerm(cqlParser.CaseExpressionTermContext context)
    {
        var expressions = context.expression();
        var items = context.caseExpressionItem();
        bool hasComparand = expressions.Length > 1;

        var caseExpr = new Elm.Case();
        if (hasComparand)
            caseExpr.Comparand = Visit(expressions[0]);
        foreach (var item in items)
            caseExpr.CaseItem.Add(new Elm.CaseItem
            {
                When = Visit(item.expression(0)),
                Then = Visit(item.expression(1)),
            });
        caseExpr.Else = Visit(expressions[^1]);
        return caseExpr;
    }

    // Exists
    public override Elm.Expression VisitExistenceExpression(cqlParser.ExistenceExpressionContext context)
        => new Elm.Exists { Operand = Visit(context.expression()) };

    // Between
    public override Elm.Expression VisitBetweenExpression(cqlParser.BetweenExpressionContext context)
    {
        var operand = Visit(context.expression());
        var low = Visit(context.expressionTerm(0));
        var high = Visit(context.expressionTerm(1));
        // between is: operand >= low and operand <= high
        return new Elm.And
        {
            Operand =
            {
                new Elm.GreaterOrEqual { Operand = { operand, low } },
                new Elm.LessOrEqual { Operand = { operand, high } },
            }
        };
    }

    // Set operations: union | intersect | except
    public override Elm.Expression VisitInFixSetExpression(cqlParser.InFixSetExpressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));
        var op = context.GetChild(1).GetText();
        return op switch
        {
            "|" or "union" => new Elm.Union { Operand = { left, right } },
            "intersect" => new Elm.Intersect { Operand = { left, right } },
            "except" => new Elm.Except { Operand = { left, right } },
            _ => throw new NotSupportedException($"Unknown set operator: {op}")
        };
    }

    // Membership: in / contains
    public override Elm.Expression VisitMembershipExpression(cqlParser.MembershipExpressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));
        return context.GetChild(1).GetText() == "contains"
            ? new Elm.Contains { Operand = { left, right } }
            : new Elm.In { Operand = { left, right } };
    }

    // Duration between
    public override Elm.Expression VisitDurationExpressionTerm(cqlParser.DurationExpressionTermContext context)
    {
        // duration in days of X → DurationBetween(start of X, end of X)
        var precision = MapPluralPrecision(context.pluralDateTimePrecision().GetText());
        var operand = Visit(context.expressionTerm());
        return new Elm.DurationBetween
        {
            Operand = { new Elm.Start { Operand = operand }, new Elm.End { Operand = operand } },
            Precision = precision,
        };
    }

    public override Elm.Expression VisitDurationBetweenExpression(cqlParser.DurationBetweenExpressionContext context)
    {
        var precision = MapPluralPrecision(context.pluralDateTimePrecision().GetText());
        return new Elm.DurationBetween
        {
            Operand = { Visit(context.expressionTerm(0)), Visit(context.expressionTerm(1)) },
            Precision = precision,
        };
    }

    // Difference between
    public override Elm.Expression VisitDifferenceBetweenExpression(cqlParser.DifferenceBetweenExpressionContext context)
    {
        var precision = MapPluralPrecision(context.pluralDateTimePrecision().GetText());
        return new Elm.DifferenceBetween
        {
            Operand = { Visit(context.expressionTerm(0)), Visit(context.expressionTerm(1)) },
            Precision = precision,
        };
    }

    // Component extraction: year from, month from, etc.
    public override Elm.Expression VisitTimeUnitExpressionTerm(cqlParser.TimeUnitExpressionTermContext context)
    {
        var operand = Visit(context.expressionTerm());
        var component = context.dateTimeComponent().GetText();
        if (component == "date")
            return new Elm.DateFrom { Operand = operand };
        if (component == "time")
            return new Elm.TimeFrom { Operand = operand };
        if (component == "timezoneoffset")
            return new Elm.TimezoneOffsetFrom { Operand = operand };
        if (component == "timezone")
            return new Elm.Null(); // deprecated, always null
        return new Elm.DateTimeComponentFrom
        {
            Operand = operand,
            Precision = MapPrecision(component),
        };
    }

    // Functions
    public override Elm.Expression VisitFunctionInvocation(cqlParser.FunctionInvocationContext context)
    {
        var func = context.function();
        var name = func.referentialIdentifier().GetText();
        var args = func.paramList()?.expression().Select(Visit).ToArray() ?? [];
        return BuildFunctionElm(name, args);
    }

    public override Elm.Expression VisitQualifiedFunctionInvocation(cqlParser.QualifiedFunctionInvocationContext context)
    {
        var func = context.qualifiedFunction();
        var name = func.identifierOrFunctionIdentifier().GetText();
        var args = func.paramList()?.expression().Select(Visit).ToArray() ?? [];
        return BuildFunctionElm(name, args);
    }

    // Invocation on expression: expr.member or expr.function()
    public override Elm.Expression VisitMemberInvocation(cqlParser.MemberInvocationContext context)
        => new Elm.AliasRef { Name = context.referentialIdentifier().GetText() };

    public override Elm.Expression VisitInvocationExpressionTerm(cqlParser.InvocationExpressionTermContext context)
    {
        var target = Visit(context.expressionTerm());
        var invocation = context.qualifiedInvocation();

        if (invocation is cqlParser.QualifiedMemberInvocationContext member)
        {
            var name = member.referentialIdentifier().GetText();
            return new Elm.Property { Source = target, Path = name };
        }

        if (invocation is cqlParser.QualifiedFunctionInvocationContext funcCtx)
        {
            var func = funcCtx.qualifiedFunction();
            var name = func.identifierOrFunctionIdentifier().GetText();
            var args = func.paramList()?.expression().Select(Visit).ToArray() ?? [];
            // Fluent-style: target becomes first argument
            var allArgs = new[] { target }.Concat(args).ToArray();
            return BuildFunctionElm(name, allArgs);
        }

        return target;
    }

    // Aggregate expression: distinct, flatten
    public override Elm.Expression VisitSetAggregateExpressionTerm(cqlParser.SetAggregateExpressionTermContext context)
    {
        var operand = Visit(context.expression(0));
        Elm.Expression? per = context.expression().Length > 1 ? Visit(context.expression(1)) : null;
        if (per is null && context.dateTimePrecision() is { } dtp)
            per = Visit(dtp);
        return context.GetChild(0).GetText() == "collapse"
            ? new Elm.Collapse { Operand = { operand, per ?? new Elm.Null() } }
            : new Elm.Expand { Operand = { operand, per ?? new Elm.Null() } };
    }

    public override Elm.Expression VisitAggregateExpressionTerm(cqlParser.AggregateExpressionTermContext context)
    {
        var operand = Visit(context.expression());
        return context.GetChild(0).GetText() == "flatten"
            ? new Elm.Flatten { Operand = operand }
            : new Elm.Distinct { Operand = operand };
    }

    // Indexer: expr[index]
    public override Elm.Expression VisitIndexedExpressionTerm(cqlParser.IndexedExpressionTermContext context)
        => new Elm.Indexer { Operand = { Visit(context.expressionTerm()), Visit(context.expression()) } };

    // MinValue / MaxValue
    public override Elm.Expression VisitTypeExtentExpressionTerm(cqlParser.TypeExtentExpressionTermContext context)
    {
        var typeName = context.namedTypeSpecifier().GetText();
        var typeQn = new System.Xml.XmlQualifiedName(typeName, "urn:hl7-org:elm-types:r1");
        return context.GetChild(0).GetText() == "minimum"
            ? new Elm.MinValue { ValueType = typeQn }
            : new Elm.MaxValue { ValueType = typeQn };
    }

    // Successor / Predecessor
    public override Elm.Expression VisitSuccessorExpressionTerm(cqlParser.SuccessorExpressionTermContext context)
        => new Elm.Successor { Operand = Visit(context.expressionTerm()) };

    public override Elm.Expression VisitPredecessorExpressionTerm(cqlParser.PredecessorExpressionTermContext context)
        => new Elm.Predecessor { Operand = Visit(context.expressionTerm()) };

    // Start/End of interval
    public override Elm.Expression VisitTimeBoundaryExpressionTerm(cqlParser.TimeBoundaryExpressionTermContext context)
    {
        var operand = Visit(context.expressionTerm());
        return context.GetChild(0).GetText() == "start"
            ? new Elm.Start { Operand = operand }
            : new Elm.End { Operand = operand };
    }

    // Width of interval
    public override Elm.Expression VisitWidthExpressionTerm(cqlParser.WidthExpressionTermContext context)
        => new Elm.Width { Operand = Visit(context.expressionTerm()) };

    // Singleton from
    public override Elm.Expression VisitElementExtractorExpressionTerm(cqlParser.ElementExtractorExpressionTermContext context)
        => new Elm.SingletonFrom { Operand = Visit(context.expressionTerm()) };

    // Point from
    public override Elm.Expression VisitPointExtractorExpressionTerm(cqlParser.PointExtractorExpressionTermContext context)
        => new Elm.PointFrom { Operand = Visit(context.expressionTerm()) };

    // Convert
    public override Elm.Expression VisitConversionExpressionTerm(cqlParser.ConversionExpressionTermContext context)
    {
        var operand = Visit(context.expression());
        var typeSpec = context.typeSpecifier();
        if (typeSpec is not null)
        {
            var typeName = typeSpec.GetText();
            var typeQn = new System.Xml.XmlQualifiedName(typeName, "urn:hl7-org:elm-types:r1");
            return new Elm.Convert { Operand = operand, ToTypeSpecifier = new Elm.NamedTypeSpecifier { Name = typeQn } };
        }
        return operand;
    }

    // Timing expressions (same, before, after, etc.)
    public override Elm.Expression VisitTimingExpression(cqlParser.TimingExpressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));
        var phrase = context.intervalOperatorPhrase();

        return phrase switch
        {
            cqlParser.BeforeOrAfterIntervalOperatorPhraseContext ba =>
                BuildBeforeOrAfter(left, right, ba),
            cqlParser.ConcurrentWithIntervalOperatorPhraseContext cw =>
                BuildSameAs(left, right, cw),
            cqlParser.IncludesIntervalOperatorPhraseContext inc =>
                inc.GetText().StartsWith("properly")
                    ? new Elm.ProperIncludes { Operand = { left, right } }
                    : new Elm.Includes { Operand = { left, right } },
            cqlParser.IncludedInIntervalOperatorPhraseContext iinc =>
                BuildIncludedIn(left, right, iinc),
            cqlParser.MeetsIntervalOperatorPhraseContext m =>
                m.ChildCount > 1 && m.GetChild(1).GetText() == "before"
                    ? new Elm.MeetsBefore { Operand = { left, right } }
                    : m.ChildCount > 1 && m.GetChild(1).GetText() == "after"
                        ? new Elm.MeetsAfter { Operand = { left, right } }
                        : new Elm.Meets { Operand = { left, right } },
            cqlParser.OverlapsIntervalOperatorPhraseContext o =>
                o.ChildCount > 1 && o.GetChild(1).GetText() == "before"
                    ? new Elm.OverlapsBefore { Operand = { left, right } }
                    : o.ChildCount > 1 && o.GetChild(1).GetText() == "after"
                        ? new Elm.OverlapsAfter { Operand = { left, right } }
                        : new Elm.Overlaps { Operand = { left, right } },
            cqlParser.StartsIntervalOperatorPhraseContext => new Elm.Starts { Operand = { left, right } },
            cqlParser.EndsIntervalOperatorPhraseContext => new Elm.Ends { Operand = { left, right } },
            _ => new Elm.Equal { Operand = { left, right } },
        };
    }

    Elm.Expression BuildIncludedIn(Elm.Expression left, Elm.Expression right, cqlParser.IncludedInIntervalOperatorPhraseContext iinc)
    {
        var precSpec = iinc.dateTimePrecisionSpecifier();
        var hasPrecision = precSpec?.dateTimePrecision() is not null;
        var precision = hasPrecision ? MapPrecision(precSpec!.dateTimePrecision().GetText()) : default;
        var isProper = iinc.GetText().Contains("properly");
        if (isProper)
            return new Elm.ProperIncludedIn { Operand = { left, right }, Precision = precision, PrecisionSpecified = hasPrecision };
        return new Elm.IncludedIn { Operand = { left, right }, Precision = precision, PrecisionSpecified = hasPrecision };
    }

    Elm.Expression BuildBeforeOrAfter(Elm.Expression left, Elm.Expression right, cqlParser.BeforeOrAfterIntervalOperatorPhraseContext ba)
    {
        var precSpec = ba.dateTimePrecisionSpecifier();
        var hasPrecision = precSpec?.dateTimePrecision() is not null;
        var precision = hasPrecision ? MapPrecision(precSpec!.dateTimePrecision().GetText()) : default;
        var relText = ba.temporalRelationship().GetText();
        var isBefore = relText.Contains("before");
        var hasOn = relText.Contains("on");

        if (hasOn)
        {
            // "on or before" / "before or on" → SameOrBefore; "on or after" / "after or on" → SameOrAfter
            if (isBefore)
            {
                var node = new Elm.SameOrBefore { Operand = { left, right } };
                if (hasPrecision) { node.Precision = precision; node.PrecisionSpecified = true; }
                return node;
            }
            else
            {
                var node = new Elm.SameOrAfter { Operand = { left, right } };
                if (hasPrecision) { node.Precision = precision; node.PrecisionSpecified = true; }
                return node;
            }
        }

        if (isBefore)
        {
            var node = new Elm.Before { Operand = { left, right } };
            if (hasPrecision) { node.Precision = precision; node.PrecisionSpecified = true; }
            return node;
        }
        else
        {
            var node = new Elm.After { Operand = { left, right } };
            if (hasPrecision) { node.Precision = precision; node.PrecisionSpecified = true; }
            return node;
        }
    }

    Elm.Expression BuildSameAs(Elm.Expression left, Elm.Expression right, cqlParser.ConcurrentWithIntervalOperatorPhraseContext ctx)
    {
        var dtPrec = ctx.dateTimePrecision();
        var hasPrecision = dtPrec is not null;
        var precision = hasPrecision ? MapPrecision(dtPrec!.GetText()) : default;
        var relQual = ctx.relativeQualifier()?.GetText();

        if (relQual == "or before")
        {
            var node = new Elm.SameOrBefore { Operand = { left, right } };
            if (hasPrecision) { node.Precision = precision; node.PrecisionSpecified = true; }
            return node;
        }
        if (relQual == "or after")
        {
            var node = new Elm.SameOrAfter { Operand = { left, right } };
            if (hasPrecision) { node.Precision = precision; node.PrecisionSpecified = true; }
            return node;
        }
        var sameAs = new Elm.SameAs { Operand = { left, right } };
        if (hasPrecision) { sameAs.Precision = precision; sameAs.PrecisionSpecified = true; }
        return sameAs;
    }

    // Build ELM for named functions
    Elm.Expression BuildFunctionElm(string name, Elm.Expression[] args)
    {
        return name switch
        {
            "Abs" => Unary<Elm.Abs>(args),
            "Ceiling" => Unary<Elm.Ceiling>(args),
            "Floor" => Unary<Elm.Floor>(args),
            "Truncate" => Unary<Elm.Truncate>(args),
            "Round" => args.Length >= 1 ? new Elm.Round { Operand = args[0], Precision = args.Length > 1 ? args[1] : null } : new Elm.Null(),
            "Ln" => Unary<Elm.Ln>(args),
            "Exp" => Unary<Elm.Exp>(args),
            "Log" => Binary<Elm.Log>(args),
            "Power" => Binary<Elm.Power>(args),
            "Successor" => Unary<Elm.Successor>(args),
            "Predecessor" => Unary<Elm.Predecessor>(args),
            "MinValue" when args.Length == 1 && args[0] is Elm.Literal lit
                => new Elm.MinValue { ValueType = new System.Xml.XmlQualifiedName(lit.Value, "urn:hl7-org:elm-types:r1") },
            "MaxValue" when args.Length == 1 && args[0] is Elm.Literal lit
                => new Elm.MaxValue { ValueType = new System.Xml.XmlQualifiedName(lit.Value, "urn:hl7-org:elm-types:r1") },
            "DateTime" => BuildDateTime(args),
            "Date" => BuildDate(args),
            "Time" => BuildTime(args),
            "Now" => new Elm.Now(),
            "Today" => new Elm.Today(),
            "TimeOfDay" => new Elm.TimeOfDay(),
            "Length" => Unary<Elm.Length>(args),
            "Upper" => Unary<Elm.Upper>(args),
            "Lower" => Unary<Elm.Lower>(args),
            "Concatenate" => Binary<Elm.Add>(args),
            "Combine" => args.Length >= 1 ? new Elm.Combine { Source = args[0], Separator = args.Length > 1 ? args[1] : null } : new Elm.Null(),
            "Split" => args.Length >= 2 ? new Elm.Split { StringToSplit = args[0], Separator = args[1] } : new Elm.Null(),
            "Substring" => args.Length >= 2 ? new Elm.Substring { StringToSub = args[0], StartIndex = args[1], Length = args.Length > 2 ? args[2] : null } : new Elm.Null(),
            "PositionOf" => args.Length >= 2 ? new Elm.PositionOf { Pattern = args[0], String = args[1] } : new Elm.Null(),
            "LastPositionOf" => args.Length >= 2 ? new Elm.LastPositionOf { Pattern = args[0], String = args[1] } : new Elm.Null(),
            "StartsWith" => Binary<Elm.StartsWith>(args),
            "EndsWith" => Binary<Elm.EndsWith>(args),
            "Matches" => Binary<Elm.Matches>(args),
            "ReplaceMatches" => Ternary<Elm.ReplaceMatches>(args),
            "Indexer" => Binary<Elm.Indexer>(args),
            "ToString" => Unary<Elm.ToString>(args),
            "ToBoolean" => Unary<Elm.ToBoolean>(args),
            "ToInteger" => Unary<Elm.ToInteger>(args),
            "ToDecimal" => Unary<Elm.ToDecimal>(args),
            "ToLong" => Unary<Elm.ToLong>(args),
            "ToQuantity" => Unary<Elm.ToQuantity>(args),
            "ToDateTime" => Unary<Elm.ToDateTime>(args),
            "ToDate" => Unary<Elm.ToDate>(args),
            "ToTime" => Unary<Elm.ToTime>(args),
            "ToConcept" => Unary<Elm.ToConcept>(args),
            "Coalesce" => Nary<Elm.Coalesce>(args),
            "IsNull" => Unary<Elm.IsNull>(args),
            "IsTrue" => Unary<Elm.IsTrue>(args),
            "IsFalse" => Unary<Elm.IsFalse>(args),
            "Not" => Unary<Elm.Not>(args),
            "Exists" => Unary<Elm.Exists>(args),
            "Distinct" => Unary<Elm.Distinct>(args),
            "Flatten" => Unary<Elm.Flatten>(args),
            "SingletonFrom" => Unary<Elm.SingletonFrom>(args),
            "First" => args.Length >= 1 ? new Elm.First { Source = args[0] } : new Elm.Null(),
            "Last" => args.Length >= 1 ? new Elm.Last { Source = args[0] } : new Elm.Null(),
            "IndexOf" => args.Length >= 2 ? new Elm.IndexOf { Source = args[0], Element = args[1] } : new Elm.Null(),
            "Count" => Aggregate<Elm.Count>(args),
            "Sum" => Aggregate<Elm.Sum>(args),
            "Min" => Aggregate<Elm.Min>(args),
            "Max" => Aggregate<Elm.Max>(args),
            "Avg" => Aggregate<Elm.Avg>(args),
            "AllTrue" => Aggregate<Elm.AllTrue>(args),
            "AnyTrue" => Aggregate<Elm.AnyTrue>(args),
            "Product" => Aggregate<Elm.Product>(args),
            "Median" => Aggregate<Elm.Median>(args),
            "Mode" => Aggregate<Elm.Mode>(args),
            "StdDev" => Aggregate<Elm.StdDev>(args),
            "Variance" => Aggregate<Elm.Variance>(args),
            "PopulationStdDev" => Aggregate<Elm.PopulationStdDev>(args),
            "PopulationVariance" => Aggregate<Elm.PopulationVariance>(args),
            "Negate" => Unary<Elm.Negate>(args),
            "LowBoundary" => Binary<Elm.LowBoundary>(args),
            "HighBoundary" => Binary<Elm.HighBoundary>(args),
            "Precision" => Unary<Elm.Precision>(args),
            "Width" => Unary<Elm.Width>(args),
            "Size" => Unary<Elm.Size>(args),
            "Start" => Unary<Elm.Start>(args),
            "End" => Unary<Elm.End>(args),
            "PointFrom" => Unary<Elm.PointFrom>(args),
            "Contains" => Binary<Elm.Contains>(args),
            "In" => Binary<Elm.In>(args),
            "Includes" => Binary<Elm.Includes>(args),
            "IncludedIn" => Binary<Elm.IncludedIn>(args),
            "ProperContains" => Binary<Elm.ProperContains>(args),
            "ProperIn" => Binary<Elm.ProperIn>(args),
            "ProperIncludes" => Binary<Elm.ProperIncludes>(args),
            "ProperIncludedIn" => Binary<Elm.ProperIncludedIn>(args),
            "Before" => Binary<Elm.Before>(args),
            "After" => Binary<Elm.After>(args),
            "Meets" => Binary<Elm.Meets>(args),
            "MeetsBefore" => Binary<Elm.MeetsBefore>(args),
            "MeetsAfter" => Binary<Elm.MeetsAfter>(args),
            "Overlaps" => Binary<Elm.Overlaps>(args),
            "OverlapsBefore" => Binary<Elm.OverlapsBefore>(args),
            "OverlapsAfter" => Binary<Elm.OverlapsAfter>(args),
            "Starts" => Binary<Elm.Starts>(args),
            "Ends" => Binary<Elm.Ends>(args),
            "Collapse" => Binary<Elm.Collapse>(args),
            "Expand" => Binary<Elm.Expand>(args),
            "Union" => Nary<Elm.Union>(args),
            "Intersect" => Nary<Elm.Intersect>(args),
            "Except" => Nary<Elm.Except>(args),
            "Message" => args.Length >= 1 ? new Elm.Message { Source = args[0], Condition = args.Length > 1 ? args[1] : null, Code = args.Length > 3 ? args[3] : null, Severity = args.Length > 2 ? args[2] : null } : new Elm.Null(),
            // Fluent-style methods (target is first arg)
            "toString" => Unary<Elm.ToString>(args),
            "toBoolean" => Unary<Elm.ToBoolean>(args),
            "toInteger" => Unary<Elm.ToInteger>(args),
            "toDecimal" => Unary<Elm.ToDecimal>(args),
            "toDateTime" => Unary<Elm.ToDateTime>(args),
            "toDate" => Unary<Elm.ToDate>(args),
            "toTime" => Unary<Elm.ToTime>(args),
            "toQuantity" => Unary<Elm.ToQuantity>(args),
            "length" => Unary<Elm.Length>(args),
            "upper" => Unary<Elm.Upper>(args),
            "lower" => Unary<Elm.Lower>(args),
            "abs" => Unary<Elm.Abs>(args),
            "ceiling" => Unary<Elm.Ceiling>(args),
            "floor" => Unary<Elm.Floor>(args),
            "truncate" => Unary<Elm.Truncate>(args),
            "round" => args.Length >= 1 ? new Elm.Round { Operand = args[0], Precision = args.Length > 1 ? args[1] : null } : new Elm.Null(),
            "ln" => Unary<Elm.Ln>(args),
            "exp" => Unary<Elm.Exp>(args),
            "successor" => Unary<Elm.Successor>(args),
            "predecessor" => Unary<Elm.Predecessor>(args),
            "distinct" => Unary<Elm.Distinct>(args),
            "flatten" => Unary<Elm.Flatten>(args),
            "count" => Aggregate<Elm.Count>(args),
            "exists" => Unary<Elm.Exists>(args),
            "first" => args.Length >= 1 ? new Elm.First { Source = args[0] } : new Elm.Null(),
            "last" => args.Length >= 1 ? new Elm.Last { Source = args[0] } : new Elm.Null(),
            "startsWith" => Binary<Elm.StartsWith>(args),
            "endsWith" => Binary<Elm.EndsWith>(args),
            "matches" => Binary<Elm.Matches>(args),
            "replaceMatches" => Ternary<Elm.ReplaceMatches>(args),
            "contains" when args.Length >= 2 && IsStringContext(args[0]) => Binary<Elm.Contains>(args),
            "contains" => Binary<Elm.Contains>(args),
            "indexOf" when args.Length >= 2 => new Elm.IndexOf { Source = args[0], Element = args[1] },
            "lastIndexOf" when args.Length >= 2 => new Elm.LastPositionOf { Pattern = args[1], String = args[0] },
            "substring" => args.Length >= 2 ? new Elm.Substring { StringToSub = args[0], StartIndex = args[1], Length = args.Length > 2 ? args[2] : null } : new Elm.Null(),
            "combine" => args.Length >= 1 ? new Elm.Combine { Source = args[0], Separator = args.Length > 1 ? args[1] : null } : new Elm.Null(),
            "split" => args.Length >= 2 ? new Elm.Split { StringToSplit = args[0], Separator = args[1] } : new Elm.Null(),
            _ => BuildFunctionRef(name, args),
        };
    }

    static bool IsStringContext(Elm.Expression expr) => expr is Elm.Literal { ValueType.Name: "String" };

    static Elm.Expression Unary<T>(Elm.Expression[] args) where T : Elm.UnaryExpression, new()
        => args.Length >= 1 ? new T { Operand = args[0] } : new Elm.Null();

    static Elm.Expression Binary<T>(Elm.Expression[] args) where T : Elm.BinaryExpression, new()
    {
        if (args.Length < 2) return new Elm.Null();
        var result = new T();
        result.Operand.Add(args[0]);
        result.Operand.Add(args[1]);
        return result;
    }

    static Elm.Expression Ternary<T>(Elm.Expression[] args) where T : Elm.TernaryExpression, new()
    {
        if (args.Length < 3) return new Elm.Null();
        var result = new T();
        result.Operand.Add(args[0]);
        result.Operand.Add(args[1]);
        result.Operand.Add(args[2]);
        return result;
    }

    static Elm.Expression Nary<T>(Elm.Expression[] args) where T : Elm.NaryExpression, new()
    {
        var result = new T();
        foreach (var arg in args) result.Operand.Add(arg);
        return result;
    }

    static Elm.Expression Aggregate<T>(Elm.Expression[] args) where T : Elm.AggregateExpression, new()
        => args.Length >= 1 ? new T { Source = args[0] } : new Elm.Null();

    static Elm.Expression BuildFunctionRef(string name, Elm.Expression[] args)
    {
        var result = new Elm.FunctionRef { Name = name };
        foreach (var arg in args) result.Operand.Add(arg);
        return result;
    }

    // DateTime/Date/Time construction helpers

    static Elm.Expression BuildDateTime(Elm.Expression[] args)
    {
        var dt = new Elm.DateTime();
        if (args.Length > 0) dt.Year = args[0];
        if (args.Length > 1) dt.Month = args[1];
        if (args.Length > 2) dt.Day = args[2];
        if (args.Length > 3) dt.Hour = args[3];
        if (args.Length > 4) dt.Minute = args[4];
        if (args.Length > 5) dt.Second = args[5];
        if (args.Length > 6) dt.Millisecond = args[6];
        if (args.Length > 7) dt.TimezoneOffset = args[7];
        return dt;
    }

    static Elm.Expression BuildDate(Elm.Expression[] args)
    {
        var d = new Elm.Date();
        if (args.Length > 0) d.Year = args[0];
        if (args.Length > 1) d.Month = args[1];
        if (args.Length > 2) d.Day = args[2];
        return d;
    }

    static Elm.Expression BuildTime(Elm.Expression[] args)
    {
        var t = new Elm.Time();
        if (args.Length > 0) t.Hour = args[0];
        if (args.Length > 1) t.Minute = args[1];
        if (args.Length > 2) t.Second = args[2];
        if (args.Length > 3) t.Millisecond = args[3];
        return t;
    }

    // Parse datetime literal to ELM DateTime constructor
    static Elm.Expression ParseDateTimeToElm(string text)
    {
        var parts = text.Split('T');
        var datePart = parts[0];
        var timePart = parts.Length > 1 ? parts[1] : null;

        var datePieces = datePart.Split('-');
        var dt = new Elm.DateTime { Year = IntLiteral(datePieces[0]) };
        if (datePieces.Length > 1) dt.Month = IntLiteral(datePieces[1]);
        if (datePieces.Length > 2) dt.Day = IntLiteral(datePieces[2]);

        if (!string.IsNullOrEmpty(timePart))
        {
            // Extract timezone offset if present
            string? tzPart = null;
            if (timePart.EndsWith("Z"))
            {
                tzPart = "Z";
                timePart = timePart[..^1];
            }
            else
            {
                var plusIdx = timePart.IndexOf('+');
                var minusIdx = timePart.LastIndexOf('-');
                var tzIdx = plusIdx >= 0 ? plusIdx : (minusIdx > 0 ? minusIdx : -1);
                if (tzIdx > 0)
                {
                    tzPart = timePart[tzIdx..];
                    timePart = timePart[..tzIdx];
                }
            }

            var timePieces = timePart.Split(':');
            if (timePieces.Length > 0 && timePieces[0].Length > 0) dt.Hour = IntLiteral(timePieces[0]);
            if (timePieces.Length > 1) dt.Minute = IntLiteral(timePieces[1]);
            if (timePieces.Length > 2)
            {
                var secParts = timePieces[2].Split('.');
                dt.Second = IntLiteral(secParts[0]);
                if (secParts.Length > 1) dt.Millisecond = IntLiteral(secParts[1].PadRight(3, '0')[..3]);
            }

            if (tzPart is not null)
            {
                if (tzPart == "Z")
                    dt.TimezoneOffset = DecimalLiteral("0.0");
                else
                {
                    var sign = tzPart[0] == '-' ? -1 : 1;
                    var tzPieces = tzPart[1..].Split(':');
                    var hours = int.Parse(tzPieces[0]);
                    var minutes = tzPieces.Length > 1 ? int.Parse(tzPieces[1]) : 0;
                    var offset = sign * (hours + minutes / 60.0m);
                    dt.TimezoneOffset = DecimalLiteral(offset.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
            }
        }
        return dt;
    }

    static Elm.Expression ParseDateToElm(string text)
    {
        var pieces = text.Split('-');
        var d = new Elm.Date { Year = IntLiteral(pieces[0]) };
        if (pieces.Length > 1) d.Month = IntLiteral(pieces[1]);
        if (pieces.Length > 2) d.Day = IntLiteral(pieces[2]);
        return d;
    }

    static Elm.Expression ParseTimeToElm(string text)
    {
        var pieces = text.Split(':');
        var t = new Elm.Time { Hour = IntLiteral(pieces[0]) };
        if (pieces.Length > 1) t.Minute = IntLiteral(pieces[1]);
        if (pieces.Length > 2)
        {
            var secParts = pieces[2].Split('.');
            t.Second = IntLiteral(secParts[0]);
            if (secParts.Length > 1)
            {
                if (secParts[1].Length > 3) return new Elm.Null();
                t.Millisecond = IntLiteral(secParts[1].PadRight(3, '0')[..3]);
            }
        }
        return t;
    }

    static Elm.Literal IntLiteral(string value)
        => new() { ValueType = IntType, Value = int.Parse(value).ToString() };

    static Elm.Literal DecimalLiteral(string value)
        => new() { ValueType = DecimalType, Value = value };

    // Precision mapping

    static Elm.DateTimePrecision MapPluralPrecision(string plural) => plural switch
    {
        "years" => Elm.DateTimePrecision.Year,
        "months" => Elm.DateTimePrecision.Month,
        "weeks" => Elm.DateTimePrecision.Week,
        "days" => Elm.DateTimePrecision.Day,
        "hours" => Elm.DateTimePrecision.Hour,
        "minutes" => Elm.DateTimePrecision.Minute,
        "seconds" => Elm.DateTimePrecision.Second,
        "milliseconds" => Elm.DateTimePrecision.Millisecond,
        _ => Elm.DateTimePrecision.Day,
    };

    public override Elm.Expression VisitDateTimePrecision(cqlParser.DateTimePrecisionContext context)
    {
        var unit = context.GetText();
        return new Elm.Quantity { Value = 1m, Unit = unit };
    }

    // Query support

    public override Elm.Expression VisitQueryExpression(cqlParser.QueryExpressionContext context)
        => Visit(context.query());

    public override Elm.Expression VisitQuery(cqlParser.QueryContext context)
    {
        var query = new Elm.Query();
        var sc = context.sourceClause();
        foreach (var aqs in sc.aliasedQuerySource())
        {
            var qs = aqs.querySource();
            var expr = qs.expression() is { } e ? Visit(e)
                : qs.qualifiedIdentifierExpression() is { } qie ? new Elm.AliasRef { Name = qie.referentialIdentifier().GetText() }
                : Visit(qs);
            var alias = aqs.alias().identifier().GetText();
            query.Source.Add(new Elm.AliasedQuerySource { Expression = expr, Alias = alias });
        }

        if (context.letClause() is { } lc)
        {
            foreach (var item in lc.letClauseItem())
            {
                query.Let.Add(new Elm.LetClause
                {
                    Identifier = item.identifier().GetText(),
                    Expression = Visit(item.expression()),
                });
            }
        }

        if (context.whereClause() is { } wc)
            query.Where = Visit(wc.expression());

        if (context.returnClause() is { } rc)
        {
            var distinct = rc.ChildCount > 2 && rc.GetChild(1).GetText() == "distinct";
            var all = rc.ChildCount > 2 && rc.GetChild(1).GetText() == "all";
            query.Return = new Elm.ReturnClause
            {
                Expression = Visit(rc.expression()),
                Distinct = distinct && !all,
            };
        }

        if (context.aggregateClause() is { } ac)
        {
            var distinct = false;
            var all = false;
            for (int i = 1; i < ac.ChildCount; i++)
            {
                var t = ac.GetChild(i).GetText();
                if (t == "distinct") distinct = true;
                if (t == "all") all = true;
            }
            query.Aggregate = new Elm.AggregateClause
            {
                Identifier = ac.identifier().GetText(),
                Expression = Visit(ac.expression()),
                Starting = ac.startingClause() is { } stc ? VisitStartingClauseValue(stc) : null,
                Distinct = distinct && !all,
            };
        }

        if (context.sortClause() is { } sortC)
        {
            query.Sort = new Elm.SortClause();
            var sd = sortC.sortDirection();
            if (sd is not null)
            {
                var dir = sd.GetText() switch
                {
                    "asc" or "ascending" => Elm.SortDirection.Asc,
                    _ => Elm.SortDirection.Desc,
                };
                query.Sort.By.Add(new Elm.ByDirection { Direction = dir });
            }
        }

        return query;
    }

    Elm.Expression VisitStartingClauseValue(cqlParser.StartingClauseContext stc)
    {
        if (stc.expression() is { } expr)
            return Visit(expr);
        if (stc.quantity() is { } qty)
            return Visit(qty);
        var sl = stc.simpleLiteral();
        if (sl is cqlParser.SimpleNumberLiteralContext sn)
        {
            var text = sn.NUMBER().GetText();
            return text.Contains('.')
                ? new Elm.Literal { ValueType = DecimalType, Value = text }
                : new Elm.Literal { ValueType = IntType, Value = text };
        }
        if (sl is cqlParser.SimpleStringLiteralContext ss)
            return new Elm.Literal { ValueType = StringType, Value = ss.STRING().GetText()[1..^1] };
        return Visit(sl);
    }

    static Elm.DateTimePrecision MapPrecision(string singular) => singular switch
    {
        "year" => Elm.DateTimePrecision.Year,
        "month" => Elm.DateTimePrecision.Month,
        "week" => Elm.DateTimePrecision.Week,
        "day" => Elm.DateTimePrecision.Day,
        "hour" => Elm.DateTimePrecision.Hour,
        "minute" => Elm.DateTimePrecision.Minute,
        "second" => Elm.DateTimePrecision.Second,
        "millisecond" => Elm.DateTimePrecision.Millisecond,
        _ => Elm.DateTimePrecision.Day,
    };

    // String unescaping

    static string UnescapeCqlString(string s)
    {
        var sb = new System.Text.StringBuilder(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '\\' && i + 1 < s.Length)
            {
                i++;
                if (s[i] == 'u' && i + 4 < s.Length)
                {
                    sb.Append((char)int.Parse(s.AsSpan(i + 1, 4), System.Globalization.NumberStyles.HexNumber));
                    i += 4;
                }
                else
                {
                    sb.Append(s[i] switch
                    {
                        'n' => '\n', 'r' => '\r', 't' => '\t', 'f' => '\f',
                        '\\' => '\\', '\'' => '\'', '"' => '"', '/' => '/', '`' => '`',
                        _ => s[i],
                    });
                }
            }
            else
            {
                sb.Append(s[i]);
            }
        }
        return sb.ToString();
    }
}

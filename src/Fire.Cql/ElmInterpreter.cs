using Elm = Fire.Cql.Elm;

namespace Fire.Cql;

public class ElmInterpreter
{
    readonly Dictionary<string, object?> _queryScope = new();

    public static object? Evaluate(string expression)
    {
        var elm = CqlToElmVisitor.Parse(expression);
        return new ElmInterpreter().Eval(elm);
    }

    object? Eval(Elm.Expression? expr)
    {
        if (expr is null) return null;
        return expr switch
        {
            // Literals
            Elm.Literal lit => EvalLiteral(lit),
            Elm.Null => null,
            Elm.Quantity q => new CqlQuantity(q.Value, q.Unit),

            // DateTime constructors
            Elm.DateTime dt => EvalDateTime(dt),
            Elm.Date d => EvalDate(d),
            Elm.Time t => EvalTime(t),
            Elm.Now => new CqlDateTime(2025, 1, 1, 0, 0, 0, 0),
            Elm.Today => new CqlDate(2025, 1, 1),
            Elm.TimeOfDay => new CqlTime(0, 0, 0, 0),

            // Selectors
            Elm.Interval iv => EvalInterval(iv),
            Elm.List list => EvalList(list),
            Elm.Tuple tuple => EvalTuple(tuple),

            // Arithmetic
            Elm.Add op => BinaryArith(op, Add),
            Elm.Subtract op => BinaryArith(op, Subtract),
            Elm.Multiply op => BinaryArith(op, Multiply),
            Elm.Divide op => BinaryArith(op, Divide),
            Elm.TruncatedDivide op => BinaryArith(op, TruncatedDivide),
            Elm.Modulo op => BinaryArith(op, Modulo),
            Elm.Power op => BinaryArith(op, Power),
            Elm.Negate neg => EvalNegate(neg),
            Elm.Abs abs => EvalAbs(Eval(abs.Operand)),
            Elm.Ceiling ceil => EvalCeiling(Eval(ceil.Operand)),
            Elm.Floor floor => EvalFloor(Eval(floor.Operand)),
            Elm.Truncate trunc => EvalTruncate(Eval(trunc.Operand)),
            Elm.Round rnd => EvalRound(Eval(rnd.Operand), Eval(rnd.Precision)),
            Elm.Ln ln => EvalLn(Eval(ln.Operand)),
            Elm.Exp exp => EvalExp(Eval(exp.Operand)),
            Elm.Log log => EvalLog(log),
            Elm.Successor succ => EvalSuccessor(Eval(succ.Operand)),
            Elm.Predecessor pred => EvalPredecessor(Eval(pred.Operand)),
            Elm.MinValue mv => EvalMinValue(mv.ValueType),
            Elm.MaxValue mv => EvalMaxValue(mv.ValueType),
            Elm.Concatenate cat => EvalConcatenate(cat),

            // Comparison
            Elm.Equal eq => EqualWithNull(EvalBinaryOperands(eq)),
            Elm.NotEqual ne => NegateNullable(EqualWithNull(EvalBinaryOperands(ne))),
            Elm.Equivalent eq => EvalEquivalent(EvalBinaryOperands(eq)),
            Elm.Less lt => CompareOp(lt, c => c < 0),
            Elm.Greater gt => CompareOp(gt, c => c > 0),
            Elm.LessOrEqual le => CompareOp(le, c => c <= 0),
            Elm.GreaterOrEqual ge => CompareOp(ge, c => c >= 0),

            // Boolean
            Elm.And and => EvalAnd(and),
            Elm.Or or => EvalOr(or),
            Elm.Xor xor => EvalXor(xor),
            Elm.Not not => EvalNot(not),
            Elm.Implies imp => EvalImplies(imp),
            Elm.IsNull isn => EvalIsNull(isn),
            Elm.IsTrue ist => Eval(ist.Operand) is true,
            Elm.IsFalse isf => Eval(isf.Operand) is false,
            Elm.If iff => EvalIf(iff),
            Elm.Case cs => EvalCase(cs),
            Elm.Coalesce coal => EvalCoalesce(coal),

            // Type operations
            Elm.Is isOp => EvalIs(isOp),
            Elm.As asOp => EvalAs(asOp),
            Elm.Convert conv => EvalConvert(conv),

            // String
            Elm.Length len => EvalLength(len),
            Elm.Upper up => Eval(up.Operand) is string s ? s.ToUpperInvariant() : null,
            Elm.Lower lo => Eval(lo.Operand) is string s ? s.ToLowerInvariant() : null,
            Elm.Combine comb => EvalCombine(comb),
            Elm.Split sp => EvalSplit(sp),
            Elm.Substring sub => EvalSubstring(sub),
            Elm.PositionOf po => EvalPositionOf(po),
            Elm.LastPositionOf lpo => EvalLastPositionOf(lpo),
            Elm.StartsWith sw => EvalStartsWith(sw),
            Elm.EndsWith ew => EvalEndsWith(ew),
            Elm.Matches m => EvalMatches(m),
            Elm.ReplaceMatches rm => EvalReplaceMatches(rm),
            Elm.Indexer idx => EvalIndexer(idx),
            Elm.IndexOf iof => EvalIndexOf(iof),

            // Conversion
            Elm.ToString ts => ToStringFunc(Eval(ts.Operand)),
            Elm.ToBoolean tb => ToBooleanFunc(Eval(tb.Operand)),
            Elm.ToInteger ti => ToIntegerFunc(Eval(ti.Operand)),
            Elm.ToDecimal td => ToDecimalFunc(Eval(td.Operand)),
            Elm.ToLong tl => ToLongFunc(Eval(tl.Operand)),
            Elm.ToQuantity tq => ToQuantityFunc(Eval(tq.Operand)),
            Elm.ToDateTime tdt => ToDateTimeFunc(Eval(tdt.Operand)),
            Elm.ToDate tda => ToDateFunc(Eval(tda.Operand)),
            Elm.ToTime tt => ToTimeFunc(Eval(tt.Operand)),
            Elm.ToConcept tc => Eval(tc.Operand) is CqlCode code ? new CqlConcept([code]) : null,

            // Interval
            Elm.Start start => Eval(start.Operand) is CqlInterval iv ? iv.Low : null,
            Elm.End end => Eval(end.Operand) is CqlInterval iv ? iv.High : null,
            Elm.Width w => EvalWidth(w),
            Elm.Size sz => EvalWidth(sz),
            Elm.PointFrom pf => Eval(pf.Operand) is CqlInterval iv && CqlComparison.Equal(iv.Low, iv.High) ? iv.Low : null,
            Elm.Contains cont => EvalContains(cont),
            Elm.In inOp => EvalIn(inOp),
            Elm.Includes inc => EvalIncludes(inc),
            Elm.IncludedIn ii => EvalIncludedIn(ii),
            Elm.ProperContains pc => EvalProperContains(pc),
            Elm.ProperIn pi => EvalProperIn(pi),
            Elm.ProperIncludes pic => EvalProperIncludes(pic),
            Elm.ProperIncludedIn pii => EvalProperIncludedIn(pii),
            Elm.Before bef => EvalBefore(bef),
            Elm.After aft => EvalAfter(aft),
            Elm.Meets meets => EvalMeets(meets),
            Elm.MeetsBefore mb => EvalMeetsBefore(mb),
            Elm.MeetsAfter ma => EvalMeetsAfter(ma),
            Elm.Overlaps ol => EvalOverlaps(ol),
            Elm.OverlapsBefore ob => EvalOverlapsBefore(ob),
            Elm.OverlapsAfter oa => EvalOverlapsAfter(oa),
            Elm.Starts st => EvalStarts(st),
            Elm.Ends en => EvalEnds(en),
            Elm.SameAs sa => EvalSameAs(sa),
            Elm.SameOrBefore sob => EvalSameOrBefore(sob),
            Elm.SameOrAfter soa => EvalSameOrAfter(soa),
            Elm.Collapse col => EvalCollapse(col),
            Elm.Expand exp => EvalExpand(exp),

            // List
            Elm.Exists ex => EvalExists(ex),
            Elm.Distinct dist => DistinctFunc(Eval(dist.Operand)),
            Elm.Flatten flat => FlattenFunc(Eval(flat.Operand)),
            Elm.SingletonFrom sf => EvalSingletonFrom(sf),
            Elm.First first => Eval(first.Source) is List<object?> { Count: > 0 } l ? l[0] : null,
            Elm.Last last => Eval(last.Source) is List<object?> { Count: > 0 } l ? l[^1] : null,
            Elm.Count cnt => Eval(cnt.Source) is List<object?> l ? l.Count(x => x is not null) : 0,
            Elm.Sum sum => SumFunc(Eval(sum.Source)),
            Elm.Min min => MinFunc(Eval(min.Source)),
            Elm.Max max => MaxFunc(Eval(max.Source)),
            Elm.Avg avg => AvgFunc(Eval(avg.Source)),
            Elm.AllTrue at => EvalAllTrue(at),
            Elm.AnyTrue an => Eval(an.Source) is List<object?> l ? l.Any(x => x is true) : null,
            Elm.Product prod => ProductFunc(Eval(prod.Source)),
            Elm.Median med => MedianFunc(Eval(med.Source)),
            Elm.Mode mode => ModeFunc(Eval(mode.Source)),
            Elm.StdDev sd => StdDevFunc(Eval(sd.Source), false),
            Elm.Variance v => VarianceFunc(Eval(v.Source), false),
            Elm.PopulationStdDev psd => StdDevFunc(Eval(psd.Source), true),
            Elm.PopulationVariance pv => VarianceFunc(Eval(pv.Source), true),
            Elm.Union u => EvalUnion(u),
            Elm.Intersect i => EvalIntersect(i),
            Elm.Except e => EvalExcept(e),

            // DateTime components
            Elm.DateTimeComponentFrom dtc => EvalComponentFrom(dtc),
            Elm.DateFrom df => EvalDateFrom(df),
            Elm.TimeFrom tf => EvalTimeFrom(tf),
            Elm.TimezoneOffsetFrom tzo => Eval(tzo.Operand) is CqlDateTime dt ? dt.TimezoneOffset : null,
            Elm.DurationBetween db => EvalDurationBetween(db),
            Elm.DifferenceBetween db => EvalDifferenceBetween(db),

            // Boundary
            Elm.LowBoundary lb => EvalLowBoundary(lb),
            Elm.HighBoundary hb => EvalHighBoundary(hb),
            Elm.Precision prec => EvalPrecision(prec),

            // Property access
            Elm.Property prop => EvalProperty(prop),

            // Message
            Elm.Message msg => EvalMessage(msg),

            // FunctionRef (runtime-evaluated functions)
            Elm.FunctionRef fr => EvalFunctionRef(fr),

            // Query
            Elm.Query q => EvalQuery(q),
            Elm.AliasRef ar => _queryScope.TryGetValue(ar.Name, out var arv) ? arv : null,
            Elm.Instance inst => EvalInstance(inst),

            _ => throw new NotSupportedException($"Unsupported ELM expression: {expr.GetType().Name}")
        };
    }

    object? EvalMessage(Elm.Message msg)
    {
        var source = Eval(msg.Source);
        var severity = Eval(msg.Severity) as string;
        if (severity is "400" or "Error") return null;
        return source;
    }

    object? EvalInstance(Elm.Instance inst)
    {
        var rawName = inst.ClassType.Name;
        var typeName = rawName.Contains('.') ? rawName[(rawName.LastIndexOf('.') + 1)..] : rawName;
        var elements = new Dictionary<string, object?>();
        foreach (var elem in inst.Element)
            elements[elem.Name] = Eval(elem.Value);
        return typeName switch
        {
            "Quantity" => new CqlQuantity(
                elements.TryGetValue("value", out var v) && v is decimal dv ? dv
                    : elements.TryGetValue("value", out var v2) && v2 is int iv ? (decimal)iv : 0m,
                elements.TryGetValue("unit", out var u) && u is string su ? su : "1"),
            "Code" => new CqlCode(
                elements.TryGetValue("code", out var c) && c is string cs ? cs : "",
                elements.TryGetValue("system", out var s) && s is string ss ? ss : null,
                elements.TryGetValue("display", out var d) && d is string ds ? ds : null),
            "Concept" => new CqlConcept(
                elements.TryGetValue("codes", out var codes) ? codes switch
                {
                    CqlCode cc => [cc],
                    List<object?> list => list.OfType<CqlCode>().ToList(),
                    _ => [],
                } : [],
                elements.TryGetValue("display", out var disp) && disp is string dispStr ? dispStr : null),
            _ => MakeTuple(elements, typeName),
        };
    }

    static CqlTuple MakeTuple(Dictionary<string, object?> elements, string? typeName = null)
    {
        var t = new CqlTuple { TypeName = typeName };
        foreach (var (k, v) in elements)
            t.Elements[k] = v;
        return t;
    }

    // Query evaluation

    object? EvalQuery(Elm.Query query)
    {
        // Evaluate source(s)
        var sources = new List<(string alias, List<object?> items)>();
        foreach (var src in query.Source)
        {
            var val = Eval(src.Expression);
            var items = val is List<object?> list ? list : new List<object?> { val };
            sources.Add((src.Alias, items));
        }

        // Compute cross product of all sources
        var rows = CrossProduct(sources, 0);

        // Apply where clause
        if (query.Where is not null)
        {
            rows = rows.Where(row =>
            {
                SetScope(row, query);
                var result = Eval(query.Where);
                return result is true;
            }).ToList();
        }

        // Apply aggregate clause
        if (query.Aggregate is not null)
        {
            var agg = query.Aggregate;
            object? accumulator = agg.Starting is not null ? Eval(agg.Starting) : null;
            var processRows = agg.Distinct ? DistinctRows(rows, sources) : rows;
            foreach (var row in processRows)
            {
                SetScope(row, query);
                _queryScope[agg.Identifier] = accumulator;
                accumulator = Eval(agg.Expression);
            }
            ClearScope(sources, query);
            return accumulator;
        }

        // Apply return clause or default projection
        List<object?> result;
        if (query.Return is not null)
        {
            result = rows.Select(row =>
            {
                SetScope(row, query);
                var retVal = Eval(query.Return.Expression);
                return retVal;
            }).ToList();
        }
        else if (sources.Count == 1)
        {
            result = rows.Select(row => row[0].value).ToList();
        }
        else
        {
            // Multi-source: return tuples
            result = rows.Select(row =>
            {
                var tuple = new CqlTuple();
                foreach (var (alias, value) in row)
                    tuple.Elements[alias] = value;
                return (object?)tuple;
            }).ToList();
        }

        ClearScope(sources, query);

        // Apply sort
        if (query.Sort is not null && query.Sort.By.Count > 0)
        {
            var sortBy = query.Sort.By[0];
            var dir = sortBy.Direction;
            result.Sort((a, b) =>
            {
                var cmp = CqlComparison.Compare(a, b) ?? ComparePrecision(a, b);
                return dir is Elm.SortDirection.Desc or Elm.SortDirection.Descending ? -cmp : cmp;
            });
        }

        // If source was a single non-list value, unwrap to single value
        if (sources.Count == 1 && query.Sort is null)
        {
            var originalVal = Eval(query.Source[0].Expression);
            if (originalVal is not List<object?>)
                return result.Count > 0 ? result[0] : null;
        }

        return result;
    }

    static int ComparePrecision(object? a, object? b) => (a, b) switch
    {
        (CqlDateTime da, CqlDateTime db) => PrecisionLevel(da).CompareTo(PrecisionLevel(db)),
        (CqlDate da, CqlDate db) => PrecisionLevel(da).CompareTo(PrecisionLevel(db)),
        (CqlTime ta, CqlTime tb) => PrecisionLevel(ta).CompareTo(PrecisionLevel(tb)),
        _ => 0,
    };

    static int PrecisionLevel(CqlDateTime dt) =>
        dt.Millisecond is not null ? 7 : dt.Second is not null ? 6 : dt.Minute is not null ? 5 :
        dt.Hour is not null ? 4 : dt.Day is not null ? 3 : dt.Month is not null ? 2 : 1;

    static int PrecisionLevel(CqlDate d) =>
        d.Day is not null ? 3 : d.Month is not null ? 2 : 1;

    static int PrecisionLevel(CqlTime t) =>
        t.Millisecond is not null ? 4 : t.Second is not null ? 3 : t.Minute is not null ? 2 : 1;

    List<List<(string alias, object? value)>> CrossProduct(
        List<(string alias, List<object?> items)> sources, int idx)
    {
        if (idx >= sources.Count)
            return new List<List<(string, object?)>> { new() };

        var (alias, items) = sources[idx];
        var rest = CrossProduct(sources, idx + 1);
        var result = new List<List<(string, object?)>>();
        foreach (var item in items)
            foreach (var row in rest)
            {
                var newRow = new List<(string, object?)> { (alias, item) };
                newRow.AddRange(row);
                result.Add(newRow);
            }
        return result;
    }

    void SetScope(List<(string alias, object? value)> row, Elm.Query? query = null)
    {
        foreach (var (alias, value) in row)
            _queryScope[alias] = value;
        if (query is not null)
            foreach (var let in query.Let)
                _queryScope[let.Identifier] = Eval(let.Expression);
    }

    void ClearScope(List<(string alias, List<object?> items)> sources, Elm.Query? query = null)
    {
        foreach (var (alias, _) in sources)
            _queryScope.Remove(alias);
        if (query is not null)
            foreach (var let in query.Let)
                _queryScope.Remove(let.Identifier);
    }

    List<List<(string alias, object? value)>> DistinctRows(
        List<List<(string alias, object? value)>> rows,
        List<(string alias, List<object?> items)> sources)
    {
        var seen = new HashSet<string>();
        var result = new List<List<(string alias, object? value)>>();
        foreach (var row in rows)
        {
            var key = string.Join("|", row.Select(r => r.value?.ToString() ?? "null"));
            if (seen.Add(key))
                result.Add(row);
        }
        return result;
    }

    // Literal evaluation

    static object? EvalLiteral(Elm.Literal lit)
    {
        var typeName = lit.ValueType?.Name;
        return typeName switch
        {
            "Boolean" => lit.Value == "true",
            "Integer" => int.TryParse(lit.Value, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var i) ? i : null,
            "Long" => long.TryParse(lit.Value, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var l) ? l : null,
            "Decimal" => ParseCqlDecimal(lit.Value),
            "String" => lit.Value,
            _ => lit.Value,
        };
    }

    static object? ParseCqlDecimal(string value)
    {
        if (!decimal.TryParse(value, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out var d))
            return null;
        var dotIdx = value.IndexOf('.');
        if (dotIdx >= 0 && value.Length - dotIdx - 1 > 8)
            return null;
        // CQL max: 28 digits before decimal point
        var intPart = dotIdx >= 0 ? value[..dotIdx] : value;
        intPart = intPart.TrimStart('-').TrimStart('+').TrimStart('0');
        if (intPart.Length > 28)
            return null;
        return d;
    }

    // DateTime construction

    object? EvalDateTime(Elm.DateTime dt)
    {
        if (Eval(dt.Year) is not int year) return null;
        int? month = Eval(dt.Month) as int?;
        int? day = Eval(dt.Day) as int?;
        int? hour = Eval(dt.Hour) as int?;
        int? minute = Eval(dt.Minute) as int?;
        int? second = Eval(dt.Second) as int?;
        int? ms = Eval(dt.Millisecond) as int?;
        var tzOffset = Eval(dt.TimezoneOffset);
        decimal? tz = tzOffset switch
        {
            decimal d => d,
            int i => i,
            long l => (decimal)l,
            _ => null,
        };
        if (year < 1 || year > 9999) return null;
        if (month is < 1 or > 12) return null;
        if (day is < 1 or > 31) return null;
        if (hour is < 0 or > 23) return null;
        if (minute is < 0 or > 59) return null;
        if (second is < 0 or > 59) return null;
        if (ms is < 0 or > 999) return null;
        return new CqlDateTime(year, month, day, hour, minute, second, ms, tz);
    }

    object? EvalDate(Elm.Date d)
    {
        if (Eval(d.Year) is not int year) return null;
        int? month = Eval(d.Month) as int?;
        int? day = Eval(d.Day) as int?;
        if (year < 1 || year > 9999) return null;
        if (month is < 1 or > 12) return null;
        if (day is < 1 or > 31) return null;
        return new CqlDate(year, month, day);
    }

    object? EvalTime(Elm.Time t)
    {
        if (Eval(t.Hour) is not int hour) return null;
        int? minute = Eval(t.Minute) as int?;
        int? second = Eval(t.Second) as int?;
        int? ms = Eval(t.Millisecond) as int?;
        if (hour < 0 || hour > 23) return null;
        if (minute is < 0 or > 59) return null;
        if (second is < 0 or > 59) return null;
        if (ms is < 0 or > 999) return null;
        return new CqlTime(hour, minute, second, ms);
    }

    // Selectors

    object? EvalInterval(Elm.Interval iv)
    {
        var low = Eval(iv.Low);
        var high = Eval(iv.High);
        // Check for invalid interval (low > high with both closed)
        if (low is not null && high is not null)
        {
            var cmp = CqlComparison.Compare(low, high);
            if (cmp > 0) return null;
            if (cmp == 0 && (!iv.LowClosed || !iv.HighClosed)) return null;
        }
        return new CqlInterval(low, high, iv.LowClosed, iv.HighClosed);
    }

    List<object?> EvalList(Elm.List list)
    {
        var result = new List<object?>();
        foreach (var elem in list.Element)
            result.Add(Eval(elem));
        return result;
    }

    CqlTuple EvalTuple(Elm.Tuple tuple)
    {
        var result = new CqlTuple();
        foreach (var elem in tuple.Element)
            result.Elements[elem.Name] = Eval(elem.Value);
        return result;
    }

    // Arithmetic helpers

    (object? left, object? right) EvalBinaryOperands(Elm.BinaryExpression op)
        => (Eval(op.Operand[0]), Eval(op.Operand[1]));

    object? BinaryArith(Elm.BinaryExpression op, Func<object, object, object?> fn)
    {
        var (left, right) = EvalBinaryOperands(op);
        if (left is null || right is null) return null;
        return fn(left, right);
    }

    static object? Add(object left, object right) => (left, right) switch
    {
        (int a, int b) => a + b,
        (long a, long b) => a + b,
        (int a, long b) => a + b,
        (long a, int b) => a + b,
        (decimal a, decimal b) => a + b,
        (decimal a, int b) => a + b,
        (int a, decimal b) => a + b,
        (decimal a, long b) => a + (decimal)b,
        (long a, decimal b) => (decimal)a + b,
        (string a, string b) => a + b,
        (CqlQuantity a, CqlQuantity b) when a.Unit == b.Unit => new CqlQuantity(a.Value + b.Value, a.Unit),
        (CqlDateTime dt, CqlQuantity q) => AddToDateTime(dt, q),
        (CqlDate d, CqlQuantity q) => AddToDate(d, q),
        (CqlTime t, CqlQuantity q) => AddToTime(t, q),
        (CqlInterval a, CqlInterval b) => IntervalArith(a, b, Add),
        _ => null,
    };

    static object? Subtract(object left, object right) => (left, right) switch
    {
        (int a, int b) => a - b,
        (long a, long b) => a - b,
        (int a, long b) => a - b,
        (long a, int b) => a - b,
        (decimal a, decimal b) => a - b,
        (decimal a, int b) => a - b,
        (int a, decimal b) => a - b,
        (decimal a, long b) => a - (decimal)b,
        (long a, decimal b) => (decimal)a - b,
        (CqlQuantity a, CqlQuantity b) when a.Unit == b.Unit => new CqlQuantity(a.Value - b.Value, a.Unit),
        (CqlDateTime dt, CqlQuantity q) => AddToDateTime(dt, new CqlQuantity(-q.Value, q.Unit)),
        (CqlDate d, CqlQuantity q) => AddToDate(d, new CqlQuantity(-q.Value, q.Unit)),
        (CqlTime t, CqlQuantity q) => AddToTime(t, new CqlQuantity(-q.Value, q.Unit)),
        (CqlInterval a, CqlInterval b) => IntervalArith(a, b, Subtract),
        _ => null,
    };

    static object? Multiply(object left, object right) => (left, right) switch
    {
        (int a, int b) => a * b,
        (long a, long b) => a * b,
        (int a, long b) => a * (long)b,
        (long a, int b) => a * (long)b,
        (decimal a, decimal b) => a * b,
        (decimal a, int b) => a * b,
        (int a, decimal b) => a * b,
        (decimal a, long b) => a * (decimal)b,
        (long a, decimal b) => (decimal)a * b,
        (decimal a, CqlQuantity b) => new CqlQuantity(a * b.Value, b.Unit),
        (CqlQuantity a, decimal b) => new CqlQuantity(a.Value * b, a.Unit),
        (CqlQuantity a, CqlQuantity b) => new CqlQuantity(a.Value * b.Value, MultiplyUnits(a.Unit, b.Unit)),
        (CqlInterval a, CqlInterval b) => IntervalArithMul(a, b),
        _ => null,
    };

    static string MultiplyUnits(string a, string b) => a == b ? $"{a}2" : $"{a}.{b}";

    static object? IntervalArith(CqlInterval a, CqlInterval b, Func<object, object, object?> op)
    {
        if (a.Low is null || a.High is null || b.Low is null || b.High is null) return null;
        // For add: [a,b]+[c,d] = [a+c, b+d]; for subtract: [a,b]-[c,d] = [a-d, b-c]
        var results = new[] {
            op(a.Low, b.Low), op(a.Low, b.High),
            op(a.High, b.Low), op(a.High, b.High)
        };
        var valid = results.Where(r => r is not null).ToArray();
        if (valid.Length == 0) return null;
        var sorted = valid.OrderBy(v => v, Comparer<object?>.Create((x, y) => CqlComparison.Compare(x, y) ?? 0)).ToArray();
        return new CqlInterval(sorted.First(), sorted.Last(), true, true);
    }

    static object? IntervalArithMul(CqlInterval a, CqlInterval b)
    {
        if (a.Low is null || a.High is null || b.Low is null || b.High is null) return null;
        var products = new[] {
            Multiply(a.Low, b.Low), Multiply(a.Low, b.High),
            Multiply(a.High, b.Low), Multiply(a.High, b.High)
        };
        var valid = products.Where(p => p is not null).ToArray();
        if (valid.Length == 0) return null;
        var sorted = valid.OrderBy(v => v, Comparer<object?>.Create((x, y) => CqlComparison.Compare(x, y) ?? 0)).ToArray();
        return new CqlInterval(sorted.First(), sorted.Last(), true, true);
    }

    static object? Divide(object left, object right) => (left, right) switch
    {
        (int a, int b) when b != 0 => (decimal)a / b,
        (long a, long b) when b != 0 => (decimal)a / b,
        (decimal a, decimal b) when b != 0m => a / b,
        (decimal a, int b) when b != 0 => a / b,
        (int a, decimal b) when b != 0m => a / b,
        (CqlQuantity a, int b) when b != 0 => new CqlQuantity(a.Value / b, a.Unit),
        (CqlQuantity a, decimal b) when b != 0m => new CqlQuantity(a.Value / b, a.Unit),
        (CqlQuantity a, CqlQuantity b) when a.Unit == b.Unit && b.Value != 0m
            => new CqlQuantity(a.Value / b.Value, "1"),
        _ => null,
    };

    static object? TruncatedDivide(object left, object right) => (left, right) switch
    {
        (int a, int b) when b != 0 => a / b,
        (long a, long b) when b != 0 => a / b,
        (decimal a, decimal b) when b != 0m => Math.Truncate(a / b),
        (decimal a, int b) when b != 0 => Math.Truncate(a / b),
        (int a, decimal b) when b != 0m => Math.Truncate(a / b),
        (CqlQuantity a, CqlQuantity b) when a.Unit == b.Unit && b.Value != 0m
            => new CqlQuantity(Math.Truncate(a.Value / b.Value), a.Unit),
        _ => null,
    };

    static object? Modulo(object left, object right) => (left, right) switch
    {
        (int a, int b) when b != 0 => a % b,
        (long a, long b) when b != 0 => a % b,
        (decimal a, decimal b) when b != 0m => a % b,
        (decimal a, int b) when b != 0 => a % b,
        (int a, decimal b) when b != 0m => a % b,
        (CqlQuantity a, CqlQuantity b) when a.Unit == b.Unit && b.Value != 0m
            => new CqlQuantity(a.Value % b.Value, a.Unit),
        _ => null,
    };

    static object? Power(object left, object right)
    {
        double Pow(double b, double e) => Math.Pow(b, e);
        return (left, right) switch
        {
            (int a, int b) when b >= 0 => (int)Pow(a, b),
            (int a, int b) => (decimal)Pow(a, b),
            (long a, long b) when b >= 0 => (long)Pow(a, b),
            (long a, long b) => (decimal)Pow(a, b),
            (decimal a, decimal b) => (decimal)Pow((double)a, (double)b),
            (int a, decimal b) => (decimal)Pow(a, (double)b),
            (decimal a, int b) => (decimal)Pow((double)a, b),
            _ => null,
        };
    }

    object? EvalNegate(Elm.Negate neg)
    {
        if (neg.Operand is Elm.Literal lit)
        {
            // Special case: -2147483648 (int.MinValue can't be expressed as positive int)
            if (lit.ValueType?.Name == "Integer" && lit.Value == "2147483648")
                return int.MinValue;
            // Special case: -9223372036854775808L (long.MinValue can't be expressed as positive long)
            if (lit.ValueType?.Name == "Long" && lit.Value == "9223372036854775808")
                return long.MinValue;
        }
        return Negate(Eval(neg.Operand));
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

    static object? EvalAbs(object? value) => value switch
    {
        null => null,
        int i => Math.Abs(i),
        long l => Math.Abs(l),
        decimal d => Math.Abs(d),
        CqlQuantity q => new CqlQuantity(Math.Abs(q.Value), q.Unit),
        _ => null,
    };

    static object? EvalCeiling(object? value) => value switch
    {
        null => null,
        int i => i,
        decimal d => (int)Math.Ceiling(d),
        _ => null,
    };

    static object? EvalFloor(object? value) => value switch
    {
        null => null,
        int i => i,
        decimal d => (int)Math.Floor(d),
        _ => null,
    };

    static object? EvalTruncate(object? value) => value switch
    {
        null => null,
        int i => i,
        decimal d => (int)Math.Truncate(d),
        _ => null,
    };

    static object? EvalRound(object? value, object? precision)
    {
        if (value is null) return null;
        int prec = precision is int p ? p : 0;
        return value switch
        {
            decimal d => RoundHalfUp(d, prec),
            int i => i,
            _ => null,
        };
    }

    static object? EvalLn(object? value) => value switch
    {
        null => null,
        decimal d when d > 0 => (decimal)Math.Log((double)d),
        int i when i > 0 => (decimal)Math.Log(i),
        long l when l > 0 => (decimal)Math.Log(l),
        _ => null,
    };

    static object? EvalExp(object? value)
    {
        if (value is null) return null;
        double val = value switch { decimal d => (double)d, int i => i, long l => l, _ => double.NaN };
        if (double.IsNaN(val)) return null;
        var result = Math.Exp(val);
        if (double.IsInfinity(result)) return null;
        return (decimal)result;
    }

    object? EvalLog(Elm.Log log)
    {
        var (left, right) = EvalBinaryOperands(log);
        if (left is null || right is null) return null;
        double val = left switch { decimal d => (double)d, int i => i, long l => l, _ => double.NaN };
        double @base = right switch { decimal d => (double)d, int i => i, long l => l, _ => double.NaN };
        if (double.IsNaN(val) || double.IsNaN(@base) || val <= 0 || @base <= 0 || @base == 1) return null;
        return (decimal)Math.Log(val, @base);
    }

    static object? EvalSuccessor(object? value) => value switch
    {
        null => null,
        int i => i + 1,
        long l => l + 1,
        decimal d => d + 0.00000001m,
        CqlQuantity q => new CqlQuantity(q.Value + 0.00000001m, q.Unit),
        CqlDateTime dt => AddToDateTime(dt, new CqlQuantity(1, dt.Millisecond.HasValue ? "millisecond" : dt.Second.HasValue ? "second" : dt.Minute.HasValue ? "minute" : dt.Hour.HasValue ? "hour" : dt.Day.HasValue ? "day" : dt.Month.HasValue ? "month" : "year")),
        CqlDate d => AddToDate(d, new CqlQuantity(1, d.Day.HasValue ? "day" : d.Month.HasValue ? "month" : "year")),
        CqlTime t => AddToTime(t, new CqlQuantity(1, t.Millisecond.HasValue ? "millisecond" : t.Second.HasValue ? "second" : t.Minute.HasValue ? "minute" : "hour")),
        _ => null,
    };

    static object? EvalPredecessor(object? value) => value switch
    {
        null => null,
        int i => i - 1,
        long l => l - 1,
        decimal d => d - 0.00000001m,
        CqlQuantity q => new CqlQuantity(q.Value - 0.00000001m, q.Unit),
        CqlDateTime dt => AddToDateTime(dt, new CqlQuantity(-1, dt.Millisecond.HasValue ? "millisecond" : dt.Second.HasValue ? "second" : dt.Minute.HasValue ? "minute" : dt.Hour.HasValue ? "hour" : dt.Day.HasValue ? "day" : dt.Month.HasValue ? "month" : "year")),
        CqlDate d => AddToDate(d, new CqlQuantity(-1, d.Day.HasValue ? "day" : d.Month.HasValue ? "month" : "year")),
        CqlTime t => AddToTime(t, new CqlQuantity(-1, t.Millisecond.HasValue ? "millisecond" : t.Second.HasValue ? "second" : t.Minute.HasValue ? "minute" : "hour")),
        _ => null,
    };

    static object? EvalMinValue(System.Xml.XmlQualifiedName? type) => type?.Name switch
    {
        "Integer" => int.MinValue,
        "Long" => long.MinValue,
        "Decimal" => -99999999999999999999.99999999m,
        "DateTime" => new CqlDateTime(1, 1, 1, 0, 0, 0, 0),
        "Date" => new CqlDate(1, 1, 1),
        "Time" => new CqlTime(0, 0, 0, 0),
        _ => null,
    };

    static object? EvalMaxValue(System.Xml.XmlQualifiedName? type) => type?.Name switch
    {
        "Integer" => int.MaxValue,
        "Long" => long.MaxValue,
        "Decimal" => 99999999999999999999.99999999m,
        "DateTime" => new CqlDateTime(9999, 12, 31, 23, 59, 59, 999),
        "Date" => new CqlDate(9999, 12, 31),
        "Time" => new CqlTime(23, 59, 59, 999),
        _ => null,
    };

    object? EvalConcatenate(Elm.Concatenate cat)
    {
        var left = Eval(cat.Operand[0]) as string ?? "";
        var right = Eval(cat.Operand[1]) as string ?? "";
        return left + right;
    }

    // Comparison

    static object? EqualWithNull((object? left, object? right) pair)
    {
        if (pair.left is null || pair.right is null) return null;
        if (IsNullInterval(pair.left) || IsNullInterval(pair.right)) return null;
        if (pair.left is CqlTuple lt && pair.right is CqlTuple rt)
            return CqlTuple.TupleEqual(lt, rt);
        if (pair.left is List<object?> ll && pair.right is List<object?> rl)
            return ListEqual(ll, rl);
        if (pair.left is CqlInterval li && pair.right is CqlInterval ri)
            return IntervalEqual(li, ri);
        // Use Compare for types that support three-valued equality (temporal, quantity)
        if (pair.left is CqlDateTime or CqlDate or CqlTime or CqlQuantity)
        {
            var cmp = CqlComparison.Compare(pair.left, pair.right);
            if (cmp is null) return null;
            return cmp == 0;
        }
        return CqlComparison.Equal(pair.left, pair.right);
    }

    static bool IsNullInterval(object? value)
        => value is CqlInterval { Low: null, High: null };

    static object? IntervalEqual(CqlInterval a, CqlInterval b)
    {
        if (a.LowClosed != b.LowClosed || a.HighClosed != b.HighClosed) return false;
        var lowEq = EndpointEqual(a.Low, b.Low);
        if (lowEq is false) return false;
        var highEq = EndpointEqual(a.High, b.High);
        if (highEq is false) return false;
        if (lowEq is null || highEq is null) return null;
        return true;
    }

    static object? EndpointEqual(object? a, object? b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return null;
        return CqlComparison.Equal(a, b);
    }

    static object? ListEqual(List<object?> a, List<object?> b)
    {
        if (a.Count != b.Count) return false;
        bool hasNull = false;
        for (int i = 0; i < a.Count; i++)
        {
            // CQL: null elements are considered equal for list equality
            if (a[i] is null && b[i] is null) continue;
            if (a[i] is null || b[i] is null) { hasNull = true; continue; }
            if (!CqlComparison.Equal(a[i], b[i])) return false;
        }
        return hasNull ? null : true;
    }

    static object? NegateNullable(object? value) => value switch
    {
        null => null,
        bool b => !b,
        _ => null,
    };

    static object? EvalEquivalent((object? left, object? right) pair)
    {
        if (pair.left is null && pair.right is null) return true;
        if (pair.left is null || pair.right is null) return false;
        if (pair.left is string ls && pair.right is string rs)
            return string.Equals(ls, rs, StringComparison.OrdinalIgnoreCase);
        if (pair.left is List<object?> ll && pair.right is List<object?> rl)
            return ListEquivalent(ll, rl);
        return CqlComparison.Equal(pair.left, pair.right);
    }

    static bool ListEquivalent(List<object?> a, List<object?> b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
        {
            var eq = EvalEquivalent((a[i], b[i]));
            if (eq is not true) return false;
        }
        return true;
    }

    object? CompareOp(Elm.BinaryExpression op, Func<int, bool> predicate)
    {
        var (left, right) = EvalBinaryOperands(op);
        // Handle uncertain values (intervals used as scalars)
        if (left is CqlInterval li && right is not CqlInterval)
        {
            var cmpLow = CqlComparison.Compare(li.Low, right);
            var cmpHigh = CqlComparison.Compare(li.High, right);
            if (cmpLow is null || cmpHigh is null) return null;
            var lowResult = predicate(cmpLow.Value);
            var highResult = predicate(cmpHigh.Value);
            if (lowResult == highResult) return lowResult;
            return null;
        }
        if (right is CqlInterval ri && left is not CqlInterval)
        {
            var cmpLow = CqlComparison.Compare(left, ri.Low);
            var cmpHigh = CqlComparison.Compare(left, ri.High);
            if (cmpLow is null || cmpHigh is null) return null;
            var lowResult = predicate(cmpLow.Value);
            var highResult = predicate(cmpHigh.Value);
            if (lowResult == highResult) return lowResult;
            return null;
        }
        var cmp = CqlComparison.Compare(left, right);
        if (cmp is null) return null;
        return predicate(cmp.Value);
    }

    // Boolean

    object? EvalAnd(Elm.And and)
    {
        var left = Eval(and.Operand[0]) as bool?;
        var right = Eval(and.Operand[1]) as bool?;
        if (left == false || right == false) return false;
        if (left == true && right == true) return true;
        return null;
    }

    object? EvalOr(Elm.Or or)
    {
        var left = Eval(or.Operand[0]) as bool?;
        var right = Eval(or.Operand[1]) as bool?;
        if (left == true || right == true) return true;
        if (left == false && right == false) return false;
        return null;
    }

    object? EvalXor(Elm.Xor xor)
    {
        var left = Eval(xor.Operand[0]) as bool?;
        var right = Eval(xor.Operand[1]) as bool?;
        if (left is null || right is null) return null;
        return left.Value ^ right.Value;
    }

    object? EvalNot(Elm.Not not)
    {
        var value = Eval(not.Operand) as bool?;
        if (value is null) return null;
        return !value.Value;
    }

    bool EvalIsNull(Elm.IsNull isn)
    {
        var value = Eval(isn.Operand);
        if (value is null) return true;
        if (value is CqlInterval iv && iv.Low is null && iv.High is null) return true;
        return false;
    }

    object? EvalImplies(Elm.Implies imp)
    {
        var left = Eval(imp.Operand[0]) as bool?;
        var right = Eval(imp.Operand[1]) as bool?;
        if (left == false) return true;
        if (right == true) return true;
        if (left == true && right == false) return false;
        return null;
    }

    object? EvalIf(Elm.If iff)
    {
        var condition = Eval(iff.Condition) as bool?;
        return condition == true ? Eval(iff.Then) : Eval(iff.Else);
    }

    object? EvalCase(Elm.Case cs)
    {
        var comparand = cs.Comparand is not null ? Eval(cs.Comparand) : null;
        foreach (var item in cs.CaseItem)
        {
            if (cs.Comparand is not null)
            {
                var when = Eval(item.When);
                if (CqlComparison.Equal(comparand, when))
                    return Eval(item.Then);
            }
            else
            {
                if (Eval(item.When) is true)
                    return Eval(item.Then);
            }
        }
        return Eval(cs.Else);
    }

    object? EvalCoalesce(Elm.Coalesce coal)
    {
        // Single list argument: return first non-null element from the list
        if (coal.Operand.Count == 1)
        {
            var val = Eval(coal.Operand[0]);
            if (val is List<object?> list)
                return list.FirstOrDefault(x => x is not null);
            return val;
        }
        // Multiple arguments: return first non-null argument
        foreach (var op in coal.Operand)
        {
            var val = Eval(op);
            if (val is not null) return val;
        }
        return null;
    }

    // Type

    object? EvalIs(Elm.Is isOp)
    {
        var value = Eval(isOp.Operand);
        var typeName = (isOp.IsTypeSpecifier as Elm.NamedTypeSpecifier)?.Name?.Name;
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
            "Code" => value is CqlCode,
            "Concept" => value is CqlConcept,
            "Vocabulary" => value is CqlTuple t && t.TypeName is "ValueSet" or "CodeSystem" or "ConceptSet",
            _ => false,
        };
    }

    object? EvalAs(Elm.As asOp)
    {
        var value = Eval(asOp.Operand);
        var typeName = (asOp.AsTypeSpecifier as Elm.NamedTypeSpecifier)?.Name?.Name;
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

    object? EvalConvert(Elm.Convert conv)
    {
        var value = Eval(conv.Operand);
        var typeName = (conv.ToTypeSpecifier as Elm.NamedTypeSpecifier)?.Name?.Name;
        return typeName switch
        {
            "Integer" => ToIntegerFunc(value),
            "Long" => ToLongFunc(value),
            "Decimal" => ToDecimalFunc(value),
            "Boolean" => ToBooleanFunc(value),
            "String" => ToStringFunc(value),
            "DateTime" => ToDateTimeFunc(value),
            "Date" => ToDateFunc(value),
            "Time" => ToTimeFunc(value),
            "Quantity" => ToQuantityFunc(value),
            _ => null,
        };
    }

    // String

    object? EvalLength(Elm.Length len)
    {
        var value = Eval(len.Operand);
        return value switch
        {
            null => IsListType(len.Operand) ? 0 : null,
            string s => s.Length,
            List<object?> l => l.Count,
            _ => null,
        };
    }

    static bool IsListType(Elm.Expression? expr) => expr switch
    {
        Elm.As a => a.AsTypeSpecifier is Elm.ListTypeSpecifier
            || (a.AsTypeSpecifier is Elm.NamedTypeSpecifier ns && ns.Name.Name.StartsWith("List<")),
        Elm.List => true,
        _ => false,
    };

    object? EvalCombine(Elm.Combine comb)
    {
        if (Eval(comb.Source) is not List<object?> list) return null;
        if (list.Count == 0) return null;
        var sep = Eval(comb.Separator) as string ?? "";
        var strings = new List<string>();
        foreach (var item in list)
        {
            if (item is null) return null;
            strings.Add(item.ToString()!);
        }
        return string.Join(sep, strings);
    }

    object? EvalSplit(Elm.Split sp)
    {
        if (Eval(sp.StringToSplit) is not string str) return null;
        var sep = Eval(sp.Separator) as string;
        if (sep is null) return new List<object?> { str };
        return str.Split(sep).Select(s => (object?)s).ToList();
    }

    object? EvalSubstring(Elm.Substring sub)
    {
        if (Eval(sub.StringToSub) is not string str) return null;
        if (Eval(sub.StartIndex) is not int start) return null;
        if (start < 0 || start >= str.Length) return null;
        int length = sub.Length is not null && Eval(sub.Length) is int len ? len : str.Length - start;
        length = Math.Min(length, str.Length - start);
        return str.Substring(start, length);
    }

    object? EvalPositionOf(Elm.PositionOf po)
    {
        if (Eval(po.Pattern) is not string pattern || Eval(po.String) is not string str) return null;
        return str.IndexOf(pattern, StringComparison.Ordinal);
    }

    object? EvalLastPositionOf(Elm.LastPositionOf lpo)
    {
        if (Eval(lpo.Pattern) is not string pattern || Eval(lpo.String) is not string str) return null;
        return str.LastIndexOf(pattern, StringComparison.Ordinal);
    }

    object? EvalStartsWith(Elm.StartsWith sw)
    {
        var (left, right) = EvalBinaryOperands(sw);
        if (left is string a && right is string b) return a.StartsWith(b, StringComparison.Ordinal);
        return null;
    }

    object? EvalEndsWith(Elm.EndsWith ew)
    {
        var (left, right) = EvalBinaryOperands(ew);
        if (left is string a && right is string b) return a.EndsWith(b, StringComparison.Ordinal);
        return null;
    }

    object? EvalMatches(Elm.Matches m)
    {
        var (left, right) = EvalBinaryOperands(m);
        if (left is string s && right is string pattern)
            return System.Text.RegularExpressions.Regex.IsMatch(s, $"^{pattern}$");
        return null;
    }

    object? EvalReplaceMatches(Elm.ReplaceMatches rm)
    {
        var s = Eval(rm.Operand[0]) as string;
        var pattern = Eval(rm.Operand[1]) as string;
        var replacement = Eval(rm.Operand[2]) as string;
        if (s is null || pattern is null || replacement is null) return null;
        // CQL uses Java regex semantics where \$ = literal $ and \\ = literal \
        // .NET uses $$ for literal $ and \ has no special meaning in replacements
        var netReplacement = JavaReplacementToNet(replacement);
        return System.Text.RegularExpressions.Regex.Replace(s, pattern, netReplacement);
    }

    static string JavaReplacementToNet(string rep)
    {
        var sb = new System.Text.StringBuilder(rep.Length);
        for (int i = 0; i < rep.Length; i++)
        {
            if (rep[i] == '\\' && i + 1 < rep.Length)
            {
                i++;
                if (rep[i] == '$') sb.Append("$$");
                else if (rep[i] == '\\') sb.Append('\\');
                else { sb.Append('\\'); sb.Append(rep[i]); }
            }
            else
            {
                sb.Append(rep[i]);
            }
        }
        return sb.ToString();
    }

    object? EvalIndexer(Elm.Indexer idx)
    {
        var target = Eval(idx.Operand[0]);
        var index = Eval(idx.Operand[1]);
        if (target is List<object?> list && index is int i)
            return i >= 0 && i < list.Count ? list[i] : null;
        if (target is string s && index is int si)
            return si >= 0 && si < s.Length ? s[si].ToString() : null;
        return null;
    }

    object? EvalIndexOf(Elm.IndexOf iof)
    {
        var source = Eval(iof.Source);
        var element = Eval(iof.Element);
        if (source is null || element is null) return null;
        if (source is List<object?> list)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i] is not null && CqlComparison.Equal(list[i], element)) return i;
            return -1;
        }
        return null;
    }

    // Conversion functions

    static object? ToStringFunc(object? value) => value switch
    {
        null => null,
        string s => s,
        int i => i.ToString(),
        long l => l.ToString(),
        decimal d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
        bool b => b ? "true" : "false",
        CqlDateTime dt => DateTimeToDisplayString(dt),
        CqlDate d => DateToDisplayString(d),
        CqlTime t => TimeToDisplayString(t),
        CqlQuantity q => $"{q.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)} '{q.Unit}'",
        _ => value.ToString(),
    };

    static string DateTimeToDisplayString(CqlDateTime dt)
    {
        var s = $"{dt.Year:D4}";
        if (dt.Month is null) return AppendTimezone(s, dt.TimezoneOffset);
        s += $"-{dt.Month:D2}";
        if (dt.Day is null) return AppendTimezone(s, dt.TimezoneOffset);
        s += $"-{dt.Day:D2}";
        if (dt.Hour is null) return AppendTimezone(s, dt.TimezoneOffset);
        s += $"T{dt.Hour:D2}";
        if (dt.Minute is null) return AppendTimezone(s, dt.TimezoneOffset);
        s += $":{dt.Minute:D2}";
        if (dt.Second is null) return AppendTimezone(s, dt.TimezoneOffset);
        s += $":{dt.Second:D2}";
        if (dt.Millisecond is null) return AppendTimezone(s, dt.TimezoneOffset);
        s += $".{dt.Millisecond:D3}";
        return AppendTimezone(s, dt.TimezoneOffset);
    }

    static string AppendTimezone(string s, decimal? tz)
    {
        if (tz is null) return s;
        if (tz == 0m) return s + "Z";
        var sign = tz > 0 ? "+" : "-";
        var abs = Math.Abs(tz.Value);
        var hours = (int)abs;
        var minutes = (int)((abs - hours) * 60);
        return s + $"{sign}{hours:D2}:{minutes:D2}";
    }

    static string DateToDisplayString(CqlDate d)
    {
        var s = $"{d.Year:D4}";
        if (d.Month is null) return s;
        s += $"-{d.Month:D2}";
        if (d.Day is null) return s;
        s += $"-{d.Day:D2}";
        return s;
    }

    static string TimeToDisplayString(CqlTime t)
    {
        var s = $"{t.Hour:D2}";
        if (t.Minute is null) return s;
        s += $":{t.Minute:D2}";
        if (t.Second is null) return s;
        s += $":{t.Second:D2}";
        if (t.Millisecond is null) return s;
        s += $".{t.Millisecond:D3}";
        return s;
    }

    static object? ToBooleanFunc(object? value) => value switch
    {
        null => null,
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

    static object? ToIntegerFunc(object? value) => value switch
    {
        null => null,
        int i => i,
        long l when l >= int.MinValue && l <= int.MaxValue => (int)l,
        string s when int.TryParse(s, out var i) => i,
        bool b => b ? 1 : 0,
        _ => null,
    };

    static object? ToLongFunc(object? value) => value switch
    {
        null => null,
        long l => l,
        int i => (long)i,
        string s when long.TryParse(s, out var l) => l,
        _ => null,
    };

    static object? ToDecimalFunc(object? value) => value switch
    {
        null => null,
        decimal d => d,
        int i => (decimal)i,
        long l => (decimal)l,
        string s when decimal.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, out var d) => d,
        _ => null,
    };

    static object? ToQuantityFunc(object? value) => value switch
    {
        null => null,
        CqlQuantity q => q,
        int i => new CqlQuantity(i, "1"),
        decimal d => new CqlQuantity(d, "1"),
        string s => ParseQuantityString(s),
        _ => null,
    };

    static object? ParseQuantityString(string s)
    {
        s = s.Trim();
        // Format: "value 'unit'" or just "value"
        var spaceIdx = s.IndexOf(' ');
        if (spaceIdx < 0)
        {
            if (decimal.TryParse(s, System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out var d))
                return new CqlQuantity(d, "1");
            return null;
        }
        var numPart = s[..spaceIdx];
        var unitPart = s[(spaceIdx + 1)..].Trim();
        if (unitPart.StartsWith("'") && unitPart.EndsWith("'"))
            unitPart = unitPart[1..^1];
        if (!decimal.TryParse(numPart, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out var val))
            return null;
        return new CqlQuantity(val, unitPart);
    }

    static object? ToDateTimeFunc(object? value)
    {
        try { return value switch
        {
            null => null,
            CqlDateTime dt => dt,
            CqlDate d => new CqlDateTime(d.Year, d.Month, d.Day),
            string s when s.Length >= 4 => ParseDateTime(s.TrimStart('@')),
            _ => null,
        }; } catch { return null; }
    }

    static object? ToDateFunc(object? value)
    {
        try { return value switch
        {
            null => null,
            CqlDate d => d,
            CqlDateTime dt => new CqlDate(dt.Year, dt.Month, dt.Day),
            string s when s.Length >= 4 => ParseDate(s.TrimStart('@')),
            _ => null,
        }; } catch { return null; }
    }

    static object? ToTimeFunc(object? value)
    {
        try { return value switch
        {
            null => null,
            CqlTime t => t,
            string s when s.StartsWith('T') => ParseTime(s[1..]),
            string s when s.Contains(':') => ParseTime(s),
            _ => null,
        }; } catch { return null; }
    }

    // Date arithmetic

    static object? AddToDateTime(CqlDateTime dt, CqlQuantity q)
    {
        var amount = (int)q.Value;
        try
        {
            // CQL: truncate duration to the precision of the operand
            var unit = q.Unit;
            if (dt.Month is null && unit is "months" or "month") { amount /= 12; unit = "year"; }
            if (dt.Month is null && unit is "weeks" or "week") { amount = amount * 7 / 365; unit = "year"; }
            if (dt.Month is null && unit is "days" or "day") { amount /= 365; unit = "year"; }
            if (dt.Day is null && unit is "weeks" or "week") { amount = amount * 7 / 30; unit = "month"; }
            if (dt.Day is null && unit is "days" or "day") { amount /= 30; unit = "month"; }

            var sdt = new DateTime(dt.Year, dt.Month ?? 1, dt.Day ?? 1, dt.Hour ?? 0, dt.Minute ?? 0, dt.Second ?? 0, dt.Millisecond ?? 0);
            sdt = unit switch
            {
                "years" or "year" => sdt.AddYears(amount),
                "months" or "month" => sdt.AddMonths(amount),
                "weeks" or "week" => sdt.AddDays(amount * 7),
                "days" or "day" => sdt.AddDays(amount),
                "hours" or "hour" => sdt.AddHours(amount),
                "minutes" or "minute" => sdt.AddMinutes(amount),
                "seconds" or "second" => sdt.AddSeconds(amount),
                "milliseconds" or "millisecond" => sdt.AddMilliseconds(amount),
                _ => sdt,
            };
            return new CqlDateTime(sdt.Year, dt.Month is not null ? sdt.Month : null,
                dt.Day is not null ? sdt.Day : null, dt.Hour is not null ? sdt.Hour : null,
                dt.Minute is not null ? sdt.Minute : null, dt.Second is not null ? sdt.Second : null,
                dt.Millisecond is not null ? sdt.Millisecond : null, dt.TimezoneOffset);
        }
        catch { return null; }
    }

    static object? AddToDate(CqlDate d, CqlQuantity q)
    {
        var amount = (int)q.Value;
        try
        {
            var unit = q.Unit;
            if (d.Month is null && unit is "months" or "month") { amount /= 12; unit = "year"; }
            if (d.Month is null && unit is "weeks" or "week") { amount = amount * 7 / 365; unit = "year"; }
            if (d.Month is null && unit is "days" or "day") { amount /= 365; unit = "year"; }
            if (d.Day is null && unit is "weeks" or "week") { amount = amount * 7 / 30; unit = "month"; }
            if (d.Day is null && unit is "days" or "day") { amount /= 30; unit = "month"; }

            var sd = new DateTime(d.Year, d.Month ?? 1, d.Day ?? 1);
            sd = unit switch
            {
                "years" or "year" => sd.AddYears(amount),
                "months" or "month" => sd.AddMonths(amount),
                "weeks" or "week" => sd.AddDays(amount * 7),
                "days" or "day" => sd.AddDays(amount),
                _ => sd,
            };
            return new CqlDate(sd.Year, d.Month is not null ? sd.Month : null,
                d.Day is not null ? sd.Day : null);
        }
        catch { return null; }
    }

    static object? AddToTime(CqlTime t, CqlQuantity q)
    {
        var amount = (int)q.Value;
        try
        {
            var ts = new TimeSpan(0, t.Hour, t.Minute ?? 0, t.Second ?? 0, t.Millisecond ?? 0);
            ts = q.Unit switch
            {
                "hours" or "hour" => ts.Add(TimeSpan.FromHours(amount)),
                "minutes" or "minute" => ts.Add(TimeSpan.FromMinutes(amount)),
                "seconds" or "second" => ts.Add(TimeSpan.FromSeconds(amount)),
                "milliseconds" or "millisecond" => ts.Add(TimeSpan.FromMilliseconds(amount)),
                _ => ts,
            };
            if (ts < TimeSpan.Zero || ts >= TimeSpan.FromDays(1)) return null;
            return new CqlTime((int)ts.TotalHours == 0 && ts.Hours == 0 ? 0 : ts.Hours,
                t.Minute is not null ? ts.Minutes : null,
                t.Second is not null ? ts.Seconds : null,
                t.Millisecond is not null ? ts.Milliseconds : null);
        }
        catch { return null; }
    }

    // List operations

    object? EvalExists(Elm.Exists ex)
    {
        var value = Eval(ex.Operand);
        if (value is null) return false;
        if (value is List<object?> list) return list.Any(x => x is not null);
        return true;
    }

    static object? DistinctFunc(object? value)
    {
        if (value is not List<object?> list) return value;
        var result = new List<object?>();
        bool hasNull = false;
        foreach (var item in list)
        {
            if (item is null) { if (!hasNull) { hasNull = true; result.Add(null); } continue; }
            bool found = false;
            foreach (var existing in result)
                if (existing is not null && CqlComparison.Equal(item, existing)) { found = true; break; }
            if (!found) result.Add(item);
        }
        return result;
    }

    static object? FlattenFunc(object? value)
    {
        if (value is not List<object?> list) return value;
        var result = new List<object?>();
        foreach (var item in list)
        {
            if (item is List<object?> inner) result.AddRange(inner);
            else result.Add(item);
        }
        return result;
    }

    object? EvalSingletonFrom(Elm.SingletonFrom sf)
    {
        var value = Eval(sf.Operand);
        if (value is List<object?> list) return list.Count == 1 ? list[0] : null;
        return value;
    }

    static object? SumFunc(object? source)
    {
        if (source is not List<object?> list) return null;
        object? result = null;
        foreach (var item in list)
        {
            if (item is null) continue;
            if (result is null) { result = item; continue; }
            result = Add(result, item);
        }
        return result;
    }

    static object? MinFunc(object? source)
    {
        if (source is not List<object?> list) return null;
        object? result = null;
        foreach (var item in list)
        {
            if (item is null) continue;
            if (result is null || CqlComparison.Compare(item, result) < 0) result = item;
        }
        return result;
    }

    static object? MaxFunc(object? source)
    {
        if (source is not List<object?> list) return null;
        object? result = null;
        foreach (var item in list)
        {
            if (item is null) continue;
            if (result is null || CqlComparison.Compare(item, result) > 0) result = item;
        }
        return result;
    }

    static object? AvgFunc(object? source)
    {
        if (source is not List<object?> list) return null;
        var nonNull = list.Where(x => x is not null).ToList();
        if (nonNull.Count == 0) return null;
        var sum = SumFunc(nonNull.Cast<object?>().ToList());
        if (sum is null) return null;
        return Divide(sum, nonNull.Count);
    }

    object? EvalAllTrue(Elm.AllTrue at)
    {
        var source = Eval(at.Source);
        if (source is null) return true;
        if (source is not List<object?> list) return null;
        // AllTrue: null elements are ignored, returns true if all non-null values are true
        return list.Where(x => x is not null).All(x => x is true);
    }

    static object? ProductFunc(object? source)
    {
        if (source is not List<object?> list) return null;
        object? result = null;
        foreach (var item in list)
        {
            if (item is null) continue;
            if (result is null) { result = item; continue; }
            result = Multiply(result, item);
        }
        return result;
    }

    static object? MedianFunc(object? source)
    {
        if (source is not List<object?> list) return null;
        var sorted = list.Where(x => x is not null).OrderBy(x => x, Comparer<object?>.Create((a, b) => CqlComparison.Compare(a, b) ?? 0)).ToList();
        if (sorted.Count == 0) return null;
        int mid = sorted.Count / 2;
        if (sorted.Count % 2 == 1) return ToDecimalValue(sorted[mid]);
        var a = ToDecimalValue(sorted[mid - 1]);
        var b = ToDecimalValue(sorted[mid]);
        if (a is null || b is null) return null;
        return ((decimal)a + (decimal)b) / 2m;
    }

    static object? ModeFunc(object? source)
    {
        if (source is not List<object?> list) return null;
        var nonNull = list.Where(x => x is not null).ToList();
        if (nonNull.Count == 0) return null;
        var groups = new List<(object? value, int count)>();
        foreach (var item in nonNull)
        {
            bool found = false;
            for (int i = 0; i < groups.Count; i++)
            {
                if (CqlComparison.Equal(groups[i].value, item))
                {
                    groups[i] = (groups[i].value, groups[i].count + 1);
                    found = true;
                    break;
                }
            }
            if (!found) groups.Add((item, 1));
        }
        return groups.OrderByDescending(g => g.count).First().value;
    }

    static object? VarianceFunc(object? source, bool population)
    {
        if (source is not List<object?> list) return null;
        var nonNull = list.Where(x => x is not null).Select(ToDecimalValue).Where(x => x is not null).Cast<decimal>().ToList();
        if (nonNull.Count == 0) return null;
        var mean = nonNull.Average();
        var sumSq = nonNull.Sum(x => (x - mean) * (x - mean));
        int divisor = population ? nonNull.Count : nonNull.Count - 1;
        if (divisor == 0) return null;
        return sumSq / divisor;
    }

    static object? StdDevFunc(object? source, bool population)
    {
        var variance = VarianceFunc(source, population);
        if (variance is not decimal v) return null;
        return Math.Round((decimal)Math.Sqrt((double)v), 8);
    }

    static object? ToDecimalValue(object? value) => value switch
    {
        decimal d => d,
        int i => (decimal)i,
        long l => (decimal)l,
        _ => null,
    };

    object? EvalUnion(Elm.Union u)
    {
        var rawLeft = Eval(u.Operand[0]);
        var rawRight = Eval(u.Operand[1]);
        if (rawLeft is CqlInterval li && rawRight is CqlInterval ri)
            return IntervalUnion(li, ri);
        var left = ToList(rawLeft);
        var right = ToList(rawRight);
        var combined = new List<object?>(left);
        combined.AddRange(right);
        return DistinctFunc(combined) as List<object?> ?? combined;
    }

    object? IntervalUnion(CqlInterval a, CqlInterval b)
    {
        if (IsNullInterval(a) || IsNullInterval(b)) return null;
        if (a.Low is null || a.High is null || b.Low is null || b.High is null) return null;
        // Intervals must overlap or meet for union to be a single interval
        var ix = IntervalIntersect(a, b);
        bool overlapsOrMeets = ix is CqlInterval
            || IntervalsMeet(a.High, b.Low) || IntervalsMeet(b.High, a.Low);
        if (!overlapsOrMeets) return null;
        var cmpLow = CqlComparison.Compare(a.Low, b.Low);
        var cmpHigh = CqlComparison.Compare(a.High, b.High);
        if (cmpLow is null || cmpHigh is null) return null;
        var newLow = cmpLow <= 0 ? a.Low : b.Low;
        var newLowClosed = cmpLow < 0 ? a.LowClosed : cmpLow > 0 ? b.LowClosed : a.LowClosed || b.LowClosed;
        var newHigh = cmpHigh >= 0 ? a.High : b.High;
        var newHighClosed = cmpHigh > 0 ? a.HighClosed : cmpHigh < 0 ? b.HighClosed : a.HighClosed || b.HighClosed;
        return new CqlInterval(newLow, newHigh, newLowClosed, newHighClosed);
    }

    object? EvalIntersect(Elm.Intersect i)
    {
        var rawLeft = Eval(i.Operand[0]);
        var rawRight = Eval(i.Operand[1]);
        if (rawLeft is CqlInterval li && rawRight is CqlInterval ri)
            return IntervalIntersect(li, ri);
        var left = ToList(rawLeft);
        var right = ToList(rawRight);
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

    static object? IntervalIntersect(CqlInterval a, CqlInterval b)
    {
        // Determine new low: max of the two lows (null = unknown → max is unknown too)
        object? newLow; bool newLowClosed;
        if (a.Low is null && b.Low is null) { newLow = null; newLowClosed = a.LowClosed && b.LowClosed; }
        else if (a.Low is null) { newLow = null; newLowClosed = a.LowClosed; }
        else if (b.Low is null) { newLow = null; newLowClosed = b.LowClosed; }
        else
        {
            var cmp = CqlComparison.Compare(a.Low, b.Low);
            if (cmp is null) return null; // indeterminate
            if (cmp > 0) { newLow = a.Low; newLowClosed = a.LowClosed; }
            else if (cmp < 0) { newLow = b.Low; newLowClosed = b.LowClosed; }
            else { newLow = a.Low; newLowClosed = a.LowClosed && b.LowClosed; }
        }
        // Determine new high: min of the two highs (null = unknown → min is unknown too)
        object? newHigh; bool newHighClosed;
        if (a.High is null && b.High is null) { newHigh = null; newHighClosed = a.HighClosed && b.HighClosed; }
        else if (a.High is null) { newHigh = null; newHighClosed = a.HighClosed; }
        else if (b.High is null) { newHigh = null; newHighClosed = b.HighClosed; }
        else
        {
            var cmp = CqlComparison.Compare(a.High, b.High);
            if (cmp is null) return null; // indeterminate
            if (cmp < 0) { newHigh = a.High; newHighClosed = a.HighClosed; }
            else if (cmp > 0) { newHigh = b.High; newHighClosed = b.HighClosed; }
            else { newHigh = a.High; newHighClosed = a.HighClosed && b.HighClosed; }
        }
        // Check if the result is empty using effective bounds (handles discrete types)
        if (newLow is not null && newHigh is not null)
        {
            var effLow = newLowClosed ? newLow : EvalSuccessor(newLow);
            var effHigh = newHighClosed ? newHigh : EvalPredecessor(newHigh);
            if (effLow is not null && effHigh is not null)
            {
                var cmp = CqlComparison.Compare(effLow, effHigh);
                if (cmp is null) return null; // indeterminate
                if (cmp > 0) return null;
            }
        }
        return new CqlInterval(newLow, newHigh, newLowClosed, newHighClosed);
    }

    object? EvalExcept(Elm.Except e)
    {
        var rawLeft = Eval(e.Operand[0]);
        var rawRight = Eval(e.Operand[1]);
        if (rawLeft is null && rawRight is null) return null;
        if (rawLeft is CqlInterval li && rawRight is CqlInterval ri)
            return IntervalExcept(li, ri);
        if (rawLeft is null) return null;
        var left = ToList(rawLeft);
        var right = ToList(rawRight);
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

    static object? IntervalExcept(CqlInterval a, CqlInterval b)
    {
        if (a.Low is null && a.High is null) return null;
        if (b.Low is null && b.High is null) return null;
        if (a.Low is null || a.High is null || b.Low is null || b.High is null) return null;
        // If no overlap, return a unchanged
        if (IntervalIntersect(a, b) is not CqlInterval) return a;
        var (startA, endA) = EffectiveBounds(a);
        var (startB, endB) = EffectiveBounds(b);
        if (startA is null || endA is null || startB is null || endB is null) return null;
        var cmpStart = CqlComparison.Compare(startA, startB);
        var cmpEnd = CqlComparison.Compare(endA, endB);
        if (cmpStart is null || cmpEnd is null) return null;
        // b splits a in the middle → two disjoint intervals → null
        if (cmpStart < 0 && cmpEnd > 0) return null;
        // b covers a entirely → empty
        if (cmpStart >= 0 && cmpEnd <= 0) return null;
        if (cmpStart < 0)
        {
            // b removes from the high end
            var newHigh = b.LowClosed ? EvalPredecessor(b.Low) : b.Low;
            return new CqlInterval(a.Low, newHigh, a.LowClosed, true);
        }
        // b removes from the low end
        var newLow = b.HighClosed ? EvalSuccessor(b.High) : b.High;
        return new CqlInterval(newLow, a.High, true, a.HighClosed);
    }

    static List<object?> ToList(object? value) => value switch
    {
        List<object?> list => list,
        null => [],
        _ => [value],
    };

    // Interval operations

    object? EvalWidth(Elm.UnaryExpression w)
    {
        if (Eval(w.Operand) is not CqlInterval interval) return null;
        if (interval.Low is null || interval.High is null) return null;
        return Subtract(interval.High, interval.Low);
    }

    object? EvalContains(Elm.Contains cont)
    {
        var (left, right) = EvalBinaryOperands(cont);
        // contains: left is collection/interval, right is element
        if (left is List<object?> list)
            return InList(right, list);
        if (left is CqlInterval interval)
        {
            if (right is null) return null;
            if (IsNullInterval(interval)) return null;
            return InInterval(right, interval);
        }
        // string contains
        if (left is string s && right is string sub)
            return s.Contains(sub, StringComparison.Ordinal);
        return null;
    }

    object? EvalIn(Elm.In inOp)
    {
        var (left, right) = EvalBinaryOperands(inOp);
        if (right is List<object?> list)
            return InList(left, list);
        if (right is CqlInterval interval)
        {
            if (left is null) return null;
            if (IsNullInterval(interval)) return null;
            return InInterval(left, interval);
        }
        return null;
    }

    static object? InList(object? element, List<object?> list)
    {
        if (element is null)
        {
            // CQL: null elements are considered equal for membership
            return list.Any(x => x is null);
        }
        bool hasNull = false;
        foreach (var item in list)
        {
            if (item is null) { hasNull = true; continue; }
            var eq = EqualWithNull((element, item));
            if (eq is true) return true;
            if (eq is null) hasNull = true;
        }
        return hasNull ? null : false;
    }

    static object? InInterval(object? element, CqlInterval interval)
    {
        if (element is null) return null;
        var cmpLow = interval.Low is null ? null : CqlComparison.Compare(element, interval.Low);
        var cmpHigh = interval.High is null ? null : CqlComparison.Compare(element, interval.High);
        bool? lowOk = interval.Low is null ? true : cmpLow is null ? null : interval.LowClosed ? cmpLow >= 0 : cmpLow > 0;
        bool? highOk = interval.High is null ? true : cmpHigh is null ? null : interval.HighClosed ? cmpHigh <= 0 : cmpHigh < 0;
        if (lowOk == false || highOk == false) return false;
        if (lowOk == true && highOk == true) return true;
        return null;
    }

    object? EvalIncludes(Elm.Includes inc)
    {
        var (left, right) = EvalBinaryOperands(inc);
        if (left is null || right is null) return null;
        if (left is CqlInterval li && right is CqlInterval ri)
        {
            var lowIn = InInterval(ri.Low, li);
            var highIn = InInterval(ri.High, li);
            if (lowIn is true && highIn is true) return true;
            if (lowIn is false || highIn is false) return false;
            return null;
        }
        if (left is List<object?> ll && right is List<object?> rl)
        {
            foreach (var item in rl)
                if (!ListContainsEquivalent(ll, item)) return false;
            return true;
        }
        // includes with non-list right (element contains)
        if (left is List<object?> list)
            return ListContainsEquivalent(list, right);
        return null;
    }

    static bool ListContainsEquivalent(List<object?> list, object? element)
    {
        foreach (var item in list)
            if (EvalEquivalent((item, element)) is true) return true;
        return false;
    }

    object? EvalIncludedIn(Elm.IncludedIn ii)
    {
        var (left, right) = EvalBinaryOperands(ii);
        if (left is null || right is null) return null;
        if (right is CqlInterval ri && left is CqlInterval li)
        {
            if (ii.PrecisionSpecified)
            {
                var liLow = CqlComparison.TruncateToPrecision(li.Low, ii.Precision);
                var liHigh = CqlComparison.TruncateToPrecision(li.High, ii.Precision);
                var riLow = CqlComparison.TruncateToPrecision(ri.Low, ii.Precision);
                var riHigh = CqlComparison.TruncateToPrecision(ri.High, ii.Precision);
                var truncRi = new CqlInterval(riLow, riHigh, ri.LowClosed, ri.HighClosed);
                var lowIn = InInterval(liLow, truncRi);
                var highIn = InInterval(liHigh, truncRi);
                if (lowIn is true && highIn is true) return true;
                if (lowIn is false || highIn is false) return false;
                return null;
            }
            var lowInR = InInterval(li.Low, ri);
            var highInR = InInterval(li.High, ri);
            if (lowInR is true && highInR is true) return true;
            if (lowInR is false || highInR is false) return false;
            return null;
        }
        if (right is List<object?> rl && left is List<object?> ll)
        {
            foreach (var item in ll)
                if (!ListContainsEquivalent(rl, item)) return false;
            return true;
        }
        // element included in list
        if (right is List<object?> rlist)
            return InList(left, rlist);
        // element included in interval
        if (right is CqlInterval interval)
            return InInterval(left, interval);
        return null;
    }

    object? EvalProperContains(Elm.ProperContains pc)
    {
        var left = Eval(pc.Operand[0]);
        var right = Eval(pc.Operand[1]);
        if (left is List<object?> list)
        {
            var contains = InList(right, list);
            if (contains is not true) return contains;
            return list.Count > 1;
        }
        if (left is CqlInterval interval)
            return StrictlyInInterval(right, interval, pc is Elm.ProperContains { PrecisionSpecified: true } p ? p.Precision : null);
        return null;
    }

    object? EvalProperIn(Elm.ProperIn pi)
    {
        var left = Eval(pi.Operand[0]);
        var right = Eval(pi.Operand[1]);
        if (right is List<object?> list)
        {
            var contains = InList(left, list);
            if (contains is not true) return contains;
            return list.Count > 1;
        }
        if (right is CqlInterval interval)
            return StrictlyInInterval(left, interval, pi is Elm.ProperIn { PrecisionSpecified: true } p ? p.Precision : null);
        return null;
    }

    static object? StrictlyInInterval(object? element, CqlInterval interval, Elm.DateTimePrecision? precision)
    {
        if (element is null) return null;
        var (start, end) = EffectiveBounds(interval);
        if (start is null || end is null) return null;
        var el = precision is not null ? CqlComparison.TruncateToPrecision(element, precision.Value) : element;
        var s = precision is not null ? CqlComparison.TruncateToPrecision(start, precision.Value) : start;
        var e = precision is not null ? CqlComparison.TruncateToPrecision(end, precision.Value) : end;
        var cmpLow = CqlComparison.Compare(el, s);
        var cmpHigh = CqlComparison.Compare(el, e);
        if (cmpLow is null || cmpHigh is null) return null;
        return cmpLow > 0 && cmpHigh < 0;
    }

    object? EvalProperIncludes(Elm.ProperIncludes pic)
    {
        var (left, right) = EvalBinaryOperands(pic);
        if (left is null) return null;
        if (left is List<object?> ll && right is List<object?> rl)
        {
            foreach (var item in rl)
                if (!ListContainsEquivalent(ll, item)) return false;
            return ll.Count > rl.Count;
        }
        // list properly includes element
        if (left is List<object?> list)
        {
            var contains = InList(right, list);
            if (contains is not true) return contains;
            return list.Count > 1;
        }
        if (right is null) return null;
        if (left is CqlInterval li && right is CqlInterval ri)
        {
            var includes = EvalIncludes(new Elm.Includes { Operand = { pic.Operand[0], pic.Operand[1] } });
            if (includes is not true) return includes;
            return !CqlComparison.Equal(li.Low, ri.Low) || !CqlComparison.Equal(li.High, ri.High);
        }
        // interval properly includes element
        if (left is CqlInterval interval)
            return StrictlyInInterval(right, interval, null);
        return null;
    }

    object? EvalProperIncludedIn(Elm.ProperIncludedIn pii)
    {
        var (left, right) = EvalBinaryOperands(pii);
        if (right is null) return null;
        if (left is List<object?> ll && right is List<object?> rl)
        {
            foreach (var item in ll)
                if (!ListContainsEquivalent(rl, item)) return false;
            return rl.Count > ll.Count;
        }
        // element properly included in list
        if (right is List<object?> rlist)
        {
            var contains = InList(left, rlist);
            if (contains is not true) return contains;
            return rlist.Count > 1;
        }
        if (left is null) return null;
        if (left is CqlInterval li && right is CqlInterval ri)
        {
            var includes = EvalIncludedIn(new Elm.IncludedIn { Operand = { pii.Operand[0], pii.Operand[1] } });
            if (includes is not true) return includes;
            return !CqlComparison.Equal(ri.Low, li.Low) || !CqlComparison.Equal(ri.High, li.High);
        }
        // element properly included in interval
        if (right is CqlInterval interval)
            return StrictlyInInterval(left, interval, null);
        return null;
    }

    object? EvalBefore(Elm.Before bef)
    {
        var (left, right) = EvalBinaryOperands(bef);
        if (left is null || right is null) return null;
        var l = left is CqlInterval li ? li.High : left;
        var r = right is CqlInterval ri ? ri.Low : right;
        if (bef.PrecisionSpecified)
        {
            l = CqlComparison.TruncateToPrecision(l, bef.Precision);
            r = CqlComparison.TruncateToPrecision(r, bef.Precision);
        }
        var cmp = CqlComparison.Compare(l, r);
        return cmp is null ? null : cmp < 0;
    }

    object? EvalAfter(Elm.After aft)
    {
        var (left, right) = EvalBinaryOperands(aft);
        if (left is null || right is null) return null;
        var l = left is CqlInterval li ? li.Low : left;
        var r = right is CqlInterval ri ? ri.High : right;
        if (aft.PrecisionSpecified)
        {
            l = CqlComparison.TruncateToPrecision(l, aft.Precision);
            r = CqlComparison.TruncateToPrecision(r, aft.Precision);
        }
        var cmp = CqlComparison.Compare(l, r);
        return cmp is null ? null : cmp > 0;
    }

    object? EvalMeets(Elm.Meets meets)
    {
        var (left, right) = EvalBinaryOperands(meets);
        if (left is CqlInterval li && right is CqlInterval ri)
        {
            if (li.High is null || ri.Low is null || li.Low is null || ri.High is null) return null;
            var meetsBefore = IntervalsMeet(li.High, ri.Low);
            var meetsAfter = IntervalsMeet(ri.High, li.Low);
            return meetsBefore || meetsAfter;
        }
        return null;
    }

    object? EvalMeetsBefore(Elm.MeetsBefore mb)
    {
        var (left, right) = EvalBinaryOperands(mb);
        if (left is CqlInterval li && right is CqlInterval ri)
        {
            if (li.High is null || ri.Low is null) return null;
            return IntervalsMeet(li.High, ri.Low);
        }
        return null;
    }

    object? EvalMeetsAfter(Elm.MeetsAfter ma)
    {
        var (left, right) = EvalBinaryOperands(ma);
        if (left is CqlInterval li && right is CqlInterval ri)
        {
            if (li.Low is null || ri.High is null) return null;
            return IntervalsMeet(ri.High, li.Low);
        }
        return null;
    }

    static bool IntervalsMeet(object? end, object? start)
    {
        var succ = EvalSuccessor(end);
        if (succ is null) return false;
        return CqlComparison.Equal(succ, start);
    }

    object? EvalOverlaps(Elm.Overlaps ol)
    {
        var (left, right) = EvalBinaryOperands(ol);
        if (left is CqlInterval li && right is CqlInterval ri)
        {
            if (li.Low is null || li.High is null || ri.Low is null || ri.High is null) return null;
            // Check definite non-overlap using effective bounds
            var (startL, endL) = EffectiveBounds(li);
            var (startR, endR) = EffectiveBounds(ri);
            if (endL is not null && startR is not null)
            {
                var c = CqlComparison.Compare(endL, startR);
                if (c is not null && c < 0) return false;
            }
            if (endR is not null && startL is not null)
            {
                var c = CqlComparison.Compare(endR, startL);
                if (c is not null && c < 0) return false;
            }
            // Check definite overlap: startL < endR AND startR < endL (accounting for boundaries)
            var c1 = CqlComparison.Compare(li.Low, ri.High);
            var c2 = CqlComparison.Compare(ri.Low, li.High);
            if (c1 is not null && c2 is not null)
            {
                bool o1 = c1 < 0 || (c1 == 0 && li.LowClosed && ri.HighClosed);
                bool o2 = c2 < 0 || (c2 == 0 && ri.LowClosed && li.HighClosed);
                if (o1 && o2) return true;
                if (!o1 || !o2) return false;
            }
            var ix = IntervalIntersect(li, ri);
            if (ix is CqlInterval) return true;
            return null;
        }
        return null;
    }

    object? EvalOverlapsBefore(Elm.OverlapsBefore ob)
    {
        var (left, right) = EvalBinaryOperands(ob);
        if (left is CqlInterval li && right is CqlInterval ri)
        {
            if (li.Low is null || li.High is null || ri.Low is null || ri.High is null) return null;
            var (startX, endX) = EffectiveBounds(li);
            var (startY, endY) = EffectiveBounds(ri);
            if (startX is null || endX is null || startY is null || endY is null) return null;
            var startCmp = CqlComparison.Compare(startX, startY);
            if (startCmp is null || startCmp >= 0) return false;
            var overlapCmp = CqlComparison.Compare(endX, startY);
            if (overlapCmp is null || overlapCmp < 0) return false;
            var endCmp = CqlComparison.Compare(endX, endY);
            return endCmp is not null && endCmp <= 0;
        }
        return null;
    }

    object? EvalOverlapsAfter(Elm.OverlapsAfter oa)
    {
        var (left, right) = EvalBinaryOperands(oa);
        if (left is CqlInterval li && right is CqlInterval ri)
        {
            if (li.Low is null || li.High is null || ri.Low is null || ri.High is null) return null;
            var (startX, endX) = EffectiveBounds(li);
            var (startY, endY) = EffectiveBounds(ri);
            if (startX is null || endX is null || startY is null || endY is null) return null;
            var endCmp = CqlComparison.Compare(endX, endY);
            if (endCmp is null || endCmp <= 0) return false;
            var startMinCmp = CqlComparison.Compare(startX, startY);
            if (startMinCmp is null || startMinCmp < 0) return false;
            var startMaxCmp = CqlComparison.Compare(startX, endY);
            return startMaxCmp is not null && startMaxCmp <= 0;
        }
        return null;
    }

    static (object? start, object? end) EffectiveBounds(CqlInterval iv)
    {
        var start = iv.LowClosed ? iv.Low : EvalSuccessor(iv.Low);
        var end = iv.HighClosed ? iv.High : EvalPredecessor(iv.High);
        return (start, end);
    }

    object? EvalStarts(Elm.Starts st)
    {
        var (left, right) = EvalBinaryOperands(st);
        if (left is CqlInterval li && right is CqlInterval ri)
        {
            if (li.Low is null || ri.Low is null) return null;
            if (!CqlComparison.Equal(li.Low, ri.Low)) return false;
            return InInterval(li.High, ri) is true;
        }
        return null;
    }

    object? EvalEnds(Elm.Ends en)
    {
        var (left, right) = EvalBinaryOperands(en);
        if (left is CqlInterval li && right is CqlInterval ri)
        {
            if (li.High is null || ri.High is null) return null;
            if (!CqlComparison.Equal(li.High, ri.High)) return false;
            return InInterval(li.Low, ri) is true;
        }
        return null;
    }

    object? EvalSameAs(Elm.SameAs sa)
    {
        var (left, right) = EvalBinaryOperands(sa);
        if (sa.PrecisionSpecified)
        {
            var l = CqlComparison.TruncateToPrecision(left, sa.Precision);
            var r = CqlComparison.TruncateToPrecision(right, sa.Precision);
            var cmp = CqlComparison.Compare(l, r);
            return cmp is null ? null : cmp == 0;
        }
        else
        {
            var cmp = CqlComparison.Compare(left, right);
            return cmp is null ? null : cmp == 0;
        }
    }

    object? EvalSameOrBefore(Elm.SameOrBefore sob)
    {
        var (left, right) = EvalBinaryOperands(sob);
        if (left is null || right is null) return null;
        var l = left is CqlInterval li ? li.High : left;
        var r = right is CqlInterval ri ? ri.Low : right;
        if (sob.PrecisionSpecified)
        {
            l = CqlComparison.TruncateToPrecision(l, sob.Precision);
            r = CqlComparison.TruncateToPrecision(r, sob.Precision);
        }
        var cmp = CqlComparison.Compare(l, r);
        return cmp is null ? null : cmp <= 0;
    }

    object? EvalSameOrAfter(Elm.SameOrAfter soa)
    {
        var (left, right) = EvalBinaryOperands(soa);
        if (left is null || right is null) return null;
        var l = left is CqlInterval li ? li.Low : left;
        var r = right is CqlInterval ri ? ri.High : right;
        if (soa.PrecisionSpecified)
        {
            l = CqlComparison.TruncateToPrecision(l, soa.Precision);
            r = CqlComparison.TruncateToPrecision(r, soa.Precision);
        }
        var cmp = CqlComparison.Compare(l, r);
        return cmp is null ? null : cmp >= 0;
    }

    object? EvalCollapse(Elm.Collapse col)
    {
        var (left, _) = EvalBinaryOperands(col);
        if (left is not List<object?> list) return null;
        var intervals = new List<CqlInterval>();
        foreach (var item in list)
        {
            if (item is CqlInterval iv && !IsNullInterval(iv))
                intervals.Add(iv);
        }
        if (intervals.Count == 0) return new List<object?>();
        intervals.Sort((a, b) =>
        {
            var cmp = CqlComparison.Compare(a.Low, b.Low);
            if (cmp is not null) return cmp.Value;
            return 0;
        });
        var result = new List<object?>();
        var current = intervals[0];
        for (int i = 1; i < intervals.Count; i++)
        {
            var next = intervals[i];
            var merged = IntervalUnion(current, next);
            if (merged is CqlInterval m)
                current = m;
            else
            {
                result.Add(current);
                current = next;
            }
        }
        result.Add(current);
        return result;
    }

    object? EvalExpand(Elm.Expand exp)
    {
        var (left, right) = EvalBinaryOperands(exp);
        if (left is null) return null;

        // Determine step quantity
        CqlQuantity step;
        if (right is CqlQuantity q)
            step = q;
        else if (right is int si)
            step = new CqlQuantity(si, "1");
        else if (right is decimal sd)
            step = new CqlQuantity(sd, "1");
        else
            step = new CqlQuantity(1m, "1");

        // Single interval overload: expand Interval[...] per X → list of points
        if (left is CqlInterval singleInterval)
            return ExpandSingleInterval(singleInterval, step);

        // List overload: expand { intervals } per X → list of sub-intervals
        if (left is List<object?> list)
        {
            var result = new List<object?>();
            foreach (var item in list)
            {
                if (item is CqlInterval interval)
                {
                    var expanded = ExpandInterval(interval, step);
                    if (expanded is not null)
                        result.AddRange(expanded);
                }
            }
            return result;
        }

        return left;
    }

    static object? ExpandAddStep(object current, CqlQuantity step)
    {
        if (step.Unit == "1") return Add(current, (object)step.Value);
        return Add(current, step);
    }

    static object? ExpandSubStep(object current, CqlQuantity step)
    {
        if (step.Unit == "1") return Subtract(current, (object)step.Value);
        return Subtract(current, step);
    }

    static List<object?>? ExpandSingleInterval(CqlInterval interval, CqlQuantity step)
    {
        if (interval.Low is null || interval.High is null) return null;
        if (StepFinerThanValue(interval.Low, step)) return new List<object?>();
        var low = ConvertForExpand(interval.Low, step);
        var high = ConvertForExpand(interval.High, step, isHigh: true);
        var result = new List<object?>();
        var current = low;

        for (int safety = 0; safety < 100000; safety++)
        {
            // Check if the full step from current fits within the interval
            var nextStart = ExpandAddStep(current, step);
            if (nextStart is null) break;
            var subEnd = ComputeSubIntervalEnd(nextStart, current, step);
            var cmpEnd = CqlComparison.Compare(subEnd, high);
            if (cmpEnd is null) break;
            if (interval.HighClosed ? cmpEnd > 0 : cmpEnd >= 0) break;

            var point = TruncateToStepPrecision(current, step);
            result.Add(point);
            current = nextStart;
        }
        return result;
    }

    static List<object?>? ExpandInterval(CqlInterval interval, CqlQuantity step)
    {
        if (interval.Low is null || interval.High is null) return null;
        if (StepFinerThanValue(interval.Low, step)) return new List<object?>();
        var low = ConvertForExpand(interval.Low, step);
        var high = ConvertForExpand(interval.High, step, isHigh: true);
        var result = new List<object?>();
        var current = low;

        for (int safety = 0; safety < 100000; safety++)
        {
            var nextStart = ExpandAddStep(current, step);
            if (nextStart is null) break;
            var subEnd = ComputeSubIntervalEnd(nextStart, current, step);
            // Check if sub-interval fits within the original interval
            var cmpEnd = CqlComparison.Compare(subEnd, high);
            if (cmpEnd is null) break;
            if (interval.HighClosed ? cmpEnd > 0 : cmpEnd >= 0) break;

            var truncCurrent = TruncateToStepPrecision(current, step);
            var truncEnd = TruncateToStepPrecision(subEnd, step);
            result.Add(new CqlInterval(truncCurrent, truncEnd, true, true));
            current = nextStart;
        }
        return result;
    }

    static bool StepFinerThanValue(object value, CqlQuantity step)
    {
        var stepPrec = StepToPrecision(step);
        if (stepPrec is null) return false;
        return value switch
        {
            CqlTime t => stepPrec > (t.Millisecond is not null ? Elm.DateTimePrecision.Millisecond
                : t.Second is not null ? Elm.DateTimePrecision.Second
                : t.Minute is not null ? Elm.DateTimePrecision.Minute
                : Elm.DateTimePrecision.Hour),
            CqlDate d => stepPrec > (d.Day is not null ? Elm.DateTimePrecision.Day
                : d.Month is not null ? Elm.DateTimePrecision.Month
                : Elm.DateTimePrecision.Year),
            CqlDateTime dt => stepPrec > (dt.Millisecond is not null ? Elm.DateTimePrecision.Millisecond
                : dt.Second is not null ? Elm.DateTimePrecision.Second
                : dt.Minute is not null ? Elm.DateTimePrecision.Minute
                : dt.Hour is not null ? Elm.DateTimePrecision.Hour
                : dt.Day is not null ? Elm.DateTimePrecision.Day
                : dt.Month is not null ? Elm.DateTimePrecision.Month
                : Elm.DateTimePrecision.Year),
            _ => false,
        };
    }

    static object ConvertForExpand(object value, CqlQuantity step, bool isHigh = false)
    {
        if (value is int i && step.Unit == "1")
        {
            if (isHigh && step.Value != Math.Floor(step.Value))
            {
                // Integer high with decimal step: integer N covers up to N + 1 - minUnit
                var minUnit = GetNumericMinUnit(step);
                return (decimal)i + 1m - minUnit;
            }
            return (decimal)i;
        }
        return value;
    }

    static object? ComputeSubIntervalEnd(object nextStart, object current, CqlQuantity step)
    {
        if (current is int or decimal)
        {
            // Minimum unit based on step precision
            var minUnit = GetNumericMinUnit(step);
            return Subtract(nextStart, (object)minUnit);
        }
        var minStep = new CqlQuantity(1m, step.Unit);
        return Subtract(nextStart, minStep);
    }

    static decimal GetNumericMinUnit(CqlQuantity step)
    {
        var s = step.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var dotIdx = s.IndexOf('.');
        if (dotIdx < 0) return 1m;
        var decimals = s.Length - dotIdx - 1;
        return (decimal)Math.Pow(10, -decimals);
    }

    static object? TruncateToStepPrecision(object? value, CqlQuantity step)
    {
        if (value is null) return null;
        var prec = StepToPrecision(step);
        if (prec is not null)
            return CqlComparison.TruncateToPrecision(value, prec.Value);
        return value;
    }

    static Elm.DateTimePrecision? StepToPrecision(CqlQuantity step) => step.Unit switch
    {
        "year" or "years" => Elm.DateTimePrecision.Year,
        "month" or "months" => Elm.DateTimePrecision.Month,
        "week" or "weeks" => Elm.DateTimePrecision.Week,
        "day" or "days" => Elm.DateTimePrecision.Day,
        "hour" or "hours" => Elm.DateTimePrecision.Hour,
        "minute" or "minutes" => Elm.DateTimePrecision.Minute,
        "second" or "seconds" => Elm.DateTimePrecision.Second,
        "millisecond" or "milliseconds" => Elm.DateTimePrecision.Millisecond,
        _ => null,
    };

    // DateTime components

    object? EvalComponentFrom(Elm.DateTimeComponentFrom dtc)
    {
        var value = Eval(dtc.Operand);
        if (value is null) return null;
        return (value, dtc.Precision) switch
        {
            (CqlDateTime dt, Elm.DateTimePrecision.Year) => dt.Year,
            (CqlDateTime dt, Elm.DateTimePrecision.Month) => dt.Month,
            (CqlDateTime dt, Elm.DateTimePrecision.Day) => dt.Day,
            (CqlDateTime dt, Elm.DateTimePrecision.Hour) => dt.Hour,
            (CqlDateTime dt, Elm.DateTimePrecision.Minute) => dt.Minute,
            (CqlDateTime dt, Elm.DateTimePrecision.Second) => dt.Second,
            (CqlDateTime dt, Elm.DateTimePrecision.Millisecond) => dt.Millisecond,
            (CqlDate d, Elm.DateTimePrecision.Year) => d.Year,
            (CqlDate d, Elm.DateTimePrecision.Month) => d.Month,
            (CqlDate d, Elm.DateTimePrecision.Day) => d.Day,
            (CqlTime t, Elm.DateTimePrecision.Hour) => t.Hour,
            (CqlTime t, Elm.DateTimePrecision.Minute) => t.Minute,
            (CqlTime t, Elm.DateTimePrecision.Second) => t.Second,
            (CqlTime t, Elm.DateTimePrecision.Millisecond) => t.Millisecond,
            _ => null,
        };
    }

    object? EvalDateFrom(Elm.DateFrom df)
    {
        var value = Eval(df.Operand);
        if (value is CqlDateTime dt) return new CqlDate(dt.Year, dt.Month, dt.Day);
        return null;
    }

    object? EvalTimeFrom(Elm.TimeFrom tf)
    {
        var value = Eval(tf.Operand);
        if (value is CqlDateTime dt && dt.Hour is not null)
            return new CqlTime(dt.Hour.Value, dt.Minute, dt.Second, dt.Millisecond);
        return null;
    }

    object? EvalDurationBetween(Elm.BinaryExpression db)
    {
        var (left, right) = EvalBinaryOperands(db);
        if (left is null || right is null) return null;

        var precision = db switch
        {
            Elm.DurationBetween d => d.Precision,
            _ => Elm.DateTimePrecision.Day,
        };

        if (left is CqlDateTime ldt && right is CqlDateTime rdt)
            return DurationBetweenDateTimes(ldt, rdt, precision);

        if (left is CqlDate ld2 && right is CqlDate rd2)
            return DurationBetweenDates(ld2, rd2, precision);

        if (left is CqlTime lt && right is CqlTime rt)
            return DurationBetweenTimes(lt, rt, precision);

        return null;
    }

    static object? DurationBetweenTimes(CqlTime lt, CqlTime rt, Elm.DateTimePrecision precision)
    {
        try
        {
            // Determine the precision of each operand
            var lPrec = lt.Millisecond is not null ? Elm.DateTimePrecision.Millisecond
                : lt.Second is not null ? Elm.DateTimePrecision.Second
                : lt.Minute is not null ? Elm.DateTimePrecision.Minute
                : Elm.DateTimePrecision.Hour;
            var rPrec = rt.Millisecond is not null ? Elm.DateTimePrecision.Millisecond
                : rt.Second is not null ? Elm.DateTimePrecision.Second
                : rt.Minute is not null ? Elm.DateTimePrecision.Minute
                : Elm.DateTimePrecision.Hour;
            // CQL spec: compute at the less precise operand's level
            var effectivePrec = lPrec < rPrec ? lPrec : rPrec;
            // If requested precision is finer than effective precision, result is uncertain
            if (precision > effectivePrec)
                return null;

            var lts = new TimeSpan(0, lt.Hour, lt.Minute ?? 0, lt.Second ?? 0, lt.Millisecond ?? 0);
            var rts = new TimeSpan(0, rt.Hour, rt.Minute ?? 0, rt.Second ?? 0, rt.Millisecond ?? 0);
            var diff = rts - lts;
            return precision switch
            {
                Elm.DateTimePrecision.Hour => (int)diff.TotalHours,
                Elm.DateTimePrecision.Minute => (int)diff.TotalMinutes,
                Elm.DateTimePrecision.Second => (int)diff.TotalSeconds,
                Elm.DateTimePrecision.Millisecond => (int)diff.TotalMilliseconds,
                _ => null,
            };
        }
        catch { return null; }
    }

    object? EvalDifferenceBetween(Elm.DifferenceBetween db)
    {
        var (left, right) = EvalBinaryOperands(db);
        if (left is null || right is null) return null;
        var precision = db.Precision;

        if (left is CqlDateTime ldt && right is CqlDateTime rdt)
        {
            // Convert to UTC when both have timezone offsets
            if (ldt.TimezoneOffset is not null && rdt.TimezoneOffset is not null)
            {
                ldt = ldt.ToUtc();
                rdt = rdt.ToUtc();
            }
            bool uncertain = HasDifferenceUncertainty(ldt, rdt, precision);
            if (uncertain)
            {
                var ldMin = new DateTime(ldt.Year, ldt.Month ?? 1, ldt.Day ?? 1, ldt.Hour ?? 0, ldt.Minute ?? 0, ldt.Second ?? 0, ldt.Millisecond ?? 0);
                var ldMax = new DateTime(ldt.Year, ldt.Month ?? 12, ldt.Day ?? DateTime.DaysInMonth(ldt.Year, ldt.Month ?? 12),
                    ldt.Hour ?? 23, ldt.Minute ?? 59, ldt.Second ?? 59, ldt.Millisecond ?? 999);
                var rdMin = new DateTime(rdt.Year, rdt.Month ?? 1, rdt.Day ?? 1, rdt.Hour ?? 0, rdt.Minute ?? 0, rdt.Second ?? 0, rdt.Millisecond ?? 0);
                var rdMax = new DateTime(rdt.Year, rdt.Month ?? 12, rdt.Day ?? DateTime.DaysInMonth(rdt.Year, rdt.Month ?? 12),
                    rdt.Hour ?? 23, rdt.Minute ?? 59, rdt.Second ?? 59, rdt.Millisecond ?? 999);
                var min = DifferenceInPrecision(ldMax, rdMin, precision);
                var max = DifferenceInPrecision(ldMin, rdMax, precision);
                if (min == max) return min;
                return new CqlInterval(min, max, true, true);
            }
            var ld = new DateTime(ldt.Year, ldt.Month ?? 1, ldt.Day ?? 1, ldt.Hour ?? 0, ldt.Minute ?? 0, ldt.Second ?? 0, ldt.Millisecond ?? 0);
            var rd = new DateTime(rdt.Year, rdt.Month ?? 1, rdt.Day ?? 1, rdt.Hour ?? 0, rdt.Minute ?? 0, rdt.Second ?? 0, rdt.Millisecond ?? 0);
            return DifferenceInPrecision(ld, rd, precision);
        }

        if (left is CqlDate ld2 && right is CqlDate rd2)
        {
            bool uncertain = HasDateDifferenceUncertainty(ld2, rd2, precision);
            if (uncertain)
            {
                var ldMin = new DateTime(ld2.Year, ld2.Month ?? 1, ld2.Day ?? 1);
                var ldMax = new DateTime(ld2.Year, ld2.Month ?? 12, ld2.Day ?? DateTime.DaysInMonth(ld2.Year, ld2.Month ?? 12));
                var rdMin = new DateTime(rd2.Year, rd2.Month ?? 1, rd2.Day ?? 1);
                var rdMax = new DateTime(rd2.Year, rd2.Month ?? 12, rd2.Day ?? DateTime.DaysInMonth(rd2.Year, rd2.Month ?? 12));
                var min = DifferenceInPrecision(ldMax, rdMin, precision);
                var max = DifferenceInPrecision(ldMin, rdMax, precision);
                if (min == max) return min;
                return new CqlInterval(min, max, true, true);
            }
            var ldd = new DateTime(ld2.Year, ld2.Month ?? 1, ld2.Day ?? 1);
            var rdd = new DateTime(rd2.Year, rd2.Month ?? 1, rd2.Day ?? 1);
            return DifferenceInPrecision(ldd, rdd, precision);
        }

        if (left is CqlTime lt && right is CqlTime rt)
            return DifferenceBetweenTimes(lt, rt, precision);

        return null;
    }

    static object? DurationBetweenDateTimes(CqlDateTime ldt, CqlDateTime rdt, Elm.DateTimePrecision precision)
    {
        try
        {
            // Convert to UTC when both have timezone offsets
            if (ldt.TimezoneOffset is not null && rdt.TimezoneOffset is not null)
            {
                ldt = ldt.ToUtc();
                rdt = rdt.ToUtc();
            }

            bool uncertain = HasUncertainty(ldt, rdt, precision);
            if (uncertain)
            {
                var ldMin = new DateTime(ldt.Year, ldt.Month ?? 1, ldt.Day ?? 1, ldt.Hour ?? 0, ldt.Minute ?? 0, ldt.Second ?? 0, ldt.Millisecond ?? 0);
                var ldMax = new DateTime(ldt.Year, ldt.Month ?? 12, ldt.Day ?? DateTime.DaysInMonth(ldt.Year, ldt.Month ?? 12),
                    ldt.Hour ?? 23, ldt.Minute ?? 59, ldt.Second ?? 59, ldt.Millisecond ?? 999);
                var rdMin = new DateTime(rdt.Year, rdt.Month ?? 1, rdt.Day ?? 1, rdt.Hour ?? 0, rdt.Minute ?? 0, rdt.Second ?? 0, rdt.Millisecond ?? 0);
                var rdMax = new DateTime(rdt.Year, rdt.Month ?? 12, rdt.Day ?? DateTime.DaysInMonth(rdt.Year, rdt.Month ?? 12),
                    rdt.Hour ?? 23, rdt.Minute ?? 59, rdt.Second ?? 59, rdt.Millisecond ?? 999);
                var min = DurationInPrecision(ldMax, rdMin, precision);
                var max = DurationInPrecision(ldMin, rdMax, precision);
                if (min == max) return min;
                return new CqlInterval(min, max, true, true);
            }

            var ld = new DateTime(ldt.Year, ldt.Month ?? 1, ldt.Day ?? 1, ldt.Hour ?? 0, ldt.Minute ?? 0, ldt.Second ?? 0, ldt.Millisecond ?? 0);
            var rd = new DateTime(rdt.Year, rdt.Month ?? 1, rdt.Day ?? 1, rdt.Hour ?? 0, rdt.Minute ?? 0, rdt.Second ?? 0, rdt.Millisecond ?? 0);
            return DurationInPrecision(ld, rd, precision);
        }
        catch { return null; }
    }

    static object? DurationBetweenDates(CqlDate ld2, CqlDate rd2, Elm.DateTimePrecision precision)
    {
        try
        {
            bool uncertain = HasDateUncertainty(ld2, rd2, precision);
            if (uncertain)
            {
                var ldMin = new DateTime(ld2.Year, ld2.Month ?? 1, ld2.Day ?? 1);
                var ldMax = new DateTime(ld2.Year, ld2.Month ?? 12, ld2.Day ?? DateTime.DaysInMonth(ld2.Year, ld2.Month ?? 12));
                var rdMin = new DateTime(rd2.Year, rd2.Month ?? 1, rd2.Day ?? 1);
                var rdMax = new DateTime(rd2.Year, rd2.Month ?? 12, rd2.Day ?? DateTime.DaysInMonth(rd2.Year, rd2.Month ?? 12));
                var min = DurationInPrecision(ldMax, rdMin, precision);
                var max = DurationInPrecision(ldMin, rdMax, precision);
                if (min == max) return min;
                return new CqlInterval(min, max, true, true);
            }

            var ldd = new DateTime(ld2.Year, ld2.Month ?? 1, ld2.Day ?? 1);
            var rdd = new DateTime(rd2.Year, rd2.Month ?? 1, rd2.Day ?? 1);
            return DurationInPrecision(ldd, rdd, precision);
        }
        catch { return null; }
    }

    static int DurationInPrecision(DateTime ld, DateTime rd, Elm.DateTimePrecision precision) => precision switch
    {
        Elm.DateTimePrecision.Year => YearsBetween(ld, rd),
        Elm.DateTimePrecision.Month => (rd.Year - ld.Year) * 12 + rd.Month - ld.Month - (rd.Day < ld.Day ? 1 : 0),
        Elm.DateTimePrecision.Week => (int)(rd - ld).TotalDays / 7,
        Elm.DateTimePrecision.Day => (int)(rd - ld).TotalDays,
        Elm.DateTimePrecision.Hour => (int)(rd - ld).TotalHours,
        Elm.DateTimePrecision.Minute => (int)(rd - ld).TotalMinutes,
        Elm.DateTimePrecision.Second => (int)(rd - ld).TotalSeconds,
        Elm.DateTimePrecision.Millisecond => (int)(rd - ld).TotalMilliseconds,
        _ => 0,
    };

    static int YearsBetween(DateTime ld, DateTime rd)
    {
        var years = rd.Year - ld.Year;
        // Check if we haven't reached the anniversary yet
        if (years > 0 && (rd.Month < ld.Month || (rd.Month == ld.Month && rd.Day < ld.Day)))
            years--;
        if (years < 0 && (rd.Month > ld.Month || (rd.Month == ld.Month && rd.Day > ld.Day)))
            years++;
        return years;
    }

    // Difference counts boundary crossings (no sub-precision adjustment)
    static int DifferenceInPrecision(DateTime ld, DateTime rd, Elm.DateTimePrecision precision) => precision switch
    {
        Elm.DateTimePrecision.Year => rd.Year - ld.Year,
        Elm.DateTimePrecision.Month => (rd.Year - ld.Year) * 12 + rd.Month - ld.Month,
        Elm.DateTimePrecision.Week => (int)(rd.Date - ld.Date).TotalDays / 7,
        Elm.DateTimePrecision.Day => (int)(rd.Date - ld.Date).TotalDays,
        Elm.DateTimePrecision.Hour => (int)Math.Truncate((rd - ld).TotalHours),
        Elm.DateTimePrecision.Minute => (int)Math.Truncate((rd - ld).TotalMinutes),
        Elm.DateTimePrecision.Second => (int)Math.Truncate((rd - ld).TotalSeconds),
        Elm.DateTimePrecision.Millisecond => (int)(rd - ld).TotalMilliseconds,
        _ => 0,
    };

    static bool HasUncertainty(CqlDateTime ldt, CqlDateTime rdt, Elm.DateTimePrecision prec)
    {
        // Uncertainty exists if any component BELOW the requested precision is unknown
        if (prec <= Elm.DateTimePrecision.Year && (ldt.Month is null || rdt.Month is null)) return true;
        if (prec <= Elm.DateTimePrecision.Month && (ldt.Day is null || rdt.Day is null)) return true;
        if (prec <= Elm.DateTimePrecision.Day && (ldt.Hour is null || rdt.Hour is null)) return true;
        if (prec <= Elm.DateTimePrecision.Hour && (ldt.Minute is null || rdt.Minute is null)) return true;
        if (prec <= Elm.DateTimePrecision.Minute && (ldt.Second is null || rdt.Second is null)) return true;
        return false;
    }

    static bool HasDateUncertainty(CqlDate ld, CqlDate rd, Elm.DateTimePrecision prec)
    {
        if (prec <= Elm.DateTimePrecision.Year && (ld.Month is null || rd.Month is null)) return true;
        if (prec <= Elm.DateTimePrecision.Month && (ld.Day is null || rd.Day is null)) return true;
        return false;
    }

    // For difference (boundary crossings): only components AT or ABOVE the precision matter
    static bool HasDifferenceUncertainty(CqlDateTime ldt, CqlDateTime rdt, Elm.DateTimePrecision prec)
    {
        if (prec >= Elm.DateTimePrecision.Month && (ldt.Month is null || rdt.Month is null)) return true;
        if (prec >= Elm.DateTimePrecision.Week && (ldt.Day is null || rdt.Day is null)) return true;
        if (prec >= Elm.DateTimePrecision.Hour && (ldt.Hour is null || rdt.Hour is null)) return true;
        if (prec >= Elm.DateTimePrecision.Minute && (ldt.Minute is null || rdt.Minute is null)) return true;
        if (prec >= Elm.DateTimePrecision.Second && (ldt.Second is null || rdt.Second is null)) return true;
        if (prec >= Elm.DateTimePrecision.Millisecond && (ldt.Millisecond is null || rdt.Millisecond is null)) return true;
        return false;
    }

    static bool HasDateDifferenceUncertainty(CqlDate ld, CqlDate rd, Elm.DateTimePrecision prec)
    {
        if (prec >= Elm.DateTimePrecision.Month && (ld.Month is null || rd.Month is null)) return true;
        if (prec >= Elm.DateTimePrecision.Week && (ld.Day is null || rd.Day is null)) return true;
        return false;
    }

    static object? DifferenceBetweenTimes(CqlTime lt, CqlTime rt, Elm.DateTimePrecision precision)
    {
        // For difference, only check components at or above precision
        if (precision >= Elm.DateTimePrecision.Minute && (lt.Minute is null || rt.Minute is null)) return null;
        if (precision >= Elm.DateTimePrecision.Second && (lt.Second is null || rt.Second is null)) return null;
        if (precision >= Elm.DateTimePrecision.Millisecond && (lt.Millisecond is null || rt.Millisecond is null)) return null;
        var lts = new TimeSpan(0, lt.Hour, lt.Minute ?? 0, lt.Second ?? 0, lt.Millisecond ?? 0);
        var rts = new TimeSpan(0, rt.Hour, rt.Minute ?? 0, rt.Second ?? 0, rt.Millisecond ?? 0);
        return precision switch
        {
            Elm.DateTimePrecision.Hour => rt.Hour - lt.Hour,
            Elm.DateTimePrecision.Minute => (int)(rts - lts).TotalMinutes,
            Elm.DateTimePrecision.Second => (int)(rts - lts).TotalSeconds,
            Elm.DateTimePrecision.Millisecond => (int)(rts - lts).TotalMilliseconds,
            _ => null,
        };
    }

    // Boundary/Precision

    object? EvalLowBoundary(Elm.LowBoundary lb)
    {
        var (value, precision) = EvalBinaryOperands(lb);
        if (value is null) return null;
        int prec = precision as int? ?? int.MaxValue;
        return value switch
        {
            decimal d when precision is int p => DecimalLowBoundary(d, p),
            decimal d => d,
            CqlDateTime dt => new CqlDateTime(dt.Year,
                prec >= 4 ? dt.Month ?? 1 : dt.Month,
                prec >= 6 ? dt.Day ?? 1 : dt.Day,
                prec >= 9 ? dt.Hour ?? 0 : dt.Hour,
                prec >= 12 ? dt.Minute ?? 0 : dt.Minute,
                prec >= 14 ? dt.Second ?? 0 : dt.Second,
                prec >= 17 ? dt.Millisecond ?? 0 : dt.Millisecond),
            CqlDate d => new CqlDate(d.Year,
                prec >= 6 ? d.Month ?? 1 : d.Month,
                prec >= 8 ? d.Day ?? 1 : d.Day),
            CqlTime t => new CqlTime(t.Hour,
                prec >= 4 ? t.Minute ?? 0 : t.Minute,
                prec >= 6 ? t.Second ?? 0 : t.Second,
                prec >= 9 ? t.Millisecond ?? 0 : t.Millisecond),
            _ => null,
        };
    }

    object? EvalHighBoundary(Elm.HighBoundary hb)
    {
        var (value, precision) = EvalBinaryOperands(hb);
        if (value is null) return null;
        int prec = precision as int? ?? int.MaxValue;
        return value switch
        {
            decimal d when precision is int p => DecimalHighBoundary(d, p),
            decimal d => d,
            CqlDateTime dt => new CqlDateTime(dt.Year,
                prec >= 4 ? dt.Month ?? 12 : dt.Month,
                prec >= 6 ? dt.Day ?? DaysInMonth(dt.Year, dt.Month ?? 12) : dt.Day,
                prec >= 9 ? dt.Hour ?? 23 : dt.Hour,
                prec >= 12 ? dt.Minute ?? 59 : dt.Minute,
                prec >= 14 ? dt.Second ?? 59 : dt.Second,
                prec >= 17 ? dt.Millisecond ?? 999 : dt.Millisecond),
            CqlDate d => new CqlDate(d.Year,
                prec >= 6 ? d.Month ?? 12 : d.Month,
                prec >= 8 ? d.Day ?? DaysInMonth(d.Year, d.Month ?? 12) : d.Day),
            CqlTime t => new CqlTime(t.Hour,
                prec >= 4 ? t.Minute ?? 59 : t.Minute,
                prec >= 6 ? t.Second ?? 59 : t.Second,
                prec >= 9 ? t.Millisecond ?? 999 : t.Millisecond),
            _ => null,
        };
    }

    static decimal DecimalLowBoundary(decimal d, int precision)
    {
        int currentPlaces = CountDecimalPlaces(d);
        if (precision <= currentPlaces) return d;
        // Pad with zeros to the target precision
        var s = d.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (!s.Contains('.')) s += ".";
        s = s.PadRight(s.IndexOf('.') + 1 + precision, '0');
        return decimal.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
    }

    static decimal DecimalHighBoundary(decimal d, int precision)
    {
        int currentPlaces = CountDecimalPlaces(d);
        if (precision <= currentPlaces) return d;
        // Pad with 9s to the target precision
        var s = d.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (!s.Contains('.')) s += ".";
        int targetLen = s.IndexOf('.') + 1 + precision;
        // Pad existing places with 0, then fill remaining with 9
        s = s.PadRight(s.IndexOf('.') + 1 + currentPlaces, '0');
        s = s.PadRight(targetLen, '9');
        return decimal.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
    }

    static int DaysInMonth(int year, int month)
    {
        try { return DateTime.DaysInMonth(year, month); }
        catch { return 28; }
    }

    object? EvalPrecision(Elm.Precision prec)
    {
        var value = Eval(prec.Operand);
        if (value is null) return null;
        return value switch
        {
            decimal d => CountDecimalPlaces(d),
            CqlDateTime dt => 4 + (dt.Month is not null ? 2 : 0) + (dt.Day is not null ? 2 : 0)
                + (dt.Hour is not null ? 2 : 0) + (dt.Minute is not null ? 2 : 0)
                + (dt.Second is not null ? 2 : 0) + (dt.Millisecond is not null ? 3 : 0),
            CqlDate d => 4 + (d.Month is not null ? 2 : 0) + (d.Day is not null ? 2 : 0),
            CqlTime t => 2 + (t.Minute is not null ? 2 : 0) + (t.Second is not null ? 2 : 0)
                + (t.Millisecond is not null ? 3 : 0),
            _ => null,
        };
    }

    // CQL "round half up": midpoints round towards positive infinity
    static decimal RoundHalfUp(decimal d, int prec)
    {
        decimal factor = 1m;
        for (int i = 0; i < prec; i++) factor *= 10m;
        decimal shifted = d * factor;
        decimal floor = Math.Floor(shifted);
        decimal frac = shifted - floor;
        return frac >= 0.5m ? (floor + 1) / factor : floor / factor;
    }

    static int CountDecimalPlaces(decimal d)
    {
        var s = d.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var dot = s.IndexOf('.');
        return dot < 0 ? 0 : s.Length - dot - 1;
    }

    // Property access

    object? EvalProperty(Elm.Property prop)
    {
        var source = Eval(prop.Source);
        if (source is CqlTuple tuple && tuple.Elements.TryGetValue(prop.Path, out var val))
            return val;
        return null;
    }

    // FunctionRef dispatch

    object? EvalFunctionRef(Elm.FunctionRef fr)
    {
        var args = fr.Operand.Select(Eval).ToArray();
        return fr.Name switch
        {
            "Skip" => EvalSkip(args),
            "Tail" => EvalTail(args),
            "Take" => EvalTake(args),
            "skip" => EvalSkip(args),
            "tail" => EvalTail(args),
            "take" => EvalTake(args),
            "descendents" or "Descendents" => args.Length > 0 && args[0] is null ? null : new List<object?>(),
            _ => throw new NotSupportedException($"Unknown function: {fr.Name}"),
        };
    }

    static object? EvalSkip(object?[] args)
    {
        if (args.Length < 2 || args[0] is not List<object?> list) return null;
        if (args[1] is not int count) return null;
        if (count < 0) count = 0;
        return count >= list.Count ? new List<object?>() : list.Skip(count).ToList();
    }

    static object? EvalTail(object?[] args)
    {
        if (args.Length < 1 || args[0] is not List<object?> list) return null;
        return list.Count <= 1 ? new List<object?>() : list.Skip(1).ToList();
    }

    static object? EvalTake(object?[] args)
    {
        if (args.Length < 2 || args[0] is not List<object?> list) return null;
        if (args[1] is not int count) return new List<object?>();
        if (count < 0) count = 0;
        return list.Take(count).ToList();
    }

    // Parsing helpers

    static CqlDateTime? ParseDateTime(string text)
    {
        var parts = text.Split('T');
        var datePart = parts[0];
        var timePart = parts.Length > 1 ? parts[1] : null;
        var datePieces = datePart.Split('-');
        int year = int.Parse(datePieces[0]);
        int? month = datePieces.Length > 1 ? int.Parse(datePieces[1]) : null;
        int? day = datePieces.Length > 2 ? int.Parse(datePieces[2]) : null;
        if (string.IsNullOrEmpty(timePart))
            return new CqlDateTime(year, month, day);

        // Extract timezone offset
        decimal? tzOffset = null;
        if (timePart.EndsWith("Z"))
        {
            tzOffset = 0m;
            timePart = timePart[..^1];
        }
        else
        {
            var plusIdx = timePart.IndexOf('+');
            var minusIdx = timePart.LastIndexOf('-');
            var tzIdx = plusIdx >= 0 ? plusIdx : (minusIdx > 0 ? minusIdx : -1);
            if (tzIdx >= 0)
            {
                var tzStr = timePart[tzIdx..];
                timePart = timePart[..tzIdx];
                var tzParts = tzStr.Split(':');
                var tzHours = int.Parse(tzParts[0]);
                var tzMins = tzParts.Length > 1 ? int.Parse(tzParts[1]) : 0;
                tzOffset = tzHours + (tzHours >= 0 ? tzMins : -tzMins) / 60m;
            }
        }

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
            {
                if (secParts[1].Length > 3) return null;
                ms = int.Parse(secParts[1].PadRight(3, '0')[..3]);
            }
        }
        return new CqlDateTime(year, month, day, hour, minute, second, ms, tzOffset);
    }

    static CqlDate ParseDate(string text)
    {
        var pieces = text.Split('-');
        int year = int.Parse(pieces[0]);
        int? month = pieces.Length > 1 ? int.Parse(pieces[1]) : null;
        int? day = pieces.Length > 2 ? int.Parse(pieces[2]) : null;
        return new CqlDate(year, month, day);
    }

    static CqlTime? ParseTime(string text)
    {
        // Time must contain colons
        if (!text.Contains(':')) return null;

        // Strip timezone offset (Z, +HH:MM, -HH:MM)
        // Timezone appears after seconds/ms portion, look for +/- after a digit
        var tzIdx = text.IndexOf('Z');
        if (tzIdx < 0)
        {
            // Find + or - that's after the main time (after digit following last . or :)
            for (int i = text.Length - 1; i > 0; i--)
            {
                if (text[i] == '+' || text[i] == '-')
                {
                    // Only treat as timezone if preceded by a digit
                    if (i > 0 && char.IsDigit(text[i - 1])) { tzIdx = i; break; }
                }
            }
        }
        if (tzIdx >= 0) text = text[..tzIdx];

        var pieces = text.Split(':');
        if (!int.TryParse(pieces[0], out int hour)) return null;
        int? minute = pieces.Length > 1 ? int.TryParse(pieces[1], out var m) ? m : (int?)null : null;
        int? second = null;
        int? ms = null;
        if (pieces.Length > 2)
        {
            var secParts = pieces[2].Split('.');
            if (!int.TryParse(secParts[0], out var s)) return null;
            second = s;
            if (secParts.Length > 1)
            {
                if (secParts[1].Length > 3) return null;
                ms = int.Parse(secParts[1].PadRight(3, '0')[..3]);
            }
        }
        if (hour > 23 || minute is > 59 || second is > 59 || ms is > 999) return null;
        return new CqlTime(hour, minute, second, ms);
    }
}

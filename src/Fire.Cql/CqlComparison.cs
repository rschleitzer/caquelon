namespace Fire.Cql;

public static class CqlComparison
{
    public static bool Equal(object? left, object? right)
    {
        if (left is null || right is null) return false;
        return (left, right) switch
        {
            (int a, int b) => a == b,
            (long a, long b) => a == b,
            (int a, long b) => a == b,
            (long a, int b) => a == b,
            (decimal a, decimal b) => a == b,
            (decimal a, int b) => a == b,
            (int a, decimal b) => a == b,
            (decimal a, long b) => a == b,
            (long a, decimal b) => a == b,
            (bool a, bool b) => a == b,
            (string a, string b) => a == b,
            (CqlDateTime a, CqlDateTime b) => Compare(a, b) == 0,
            (CqlDate a, CqlDate b) => Compare(a, b) == 0,
            (CqlTime a, CqlTime b) => Compare(a, b) == 0,
            (CqlQuantity a, CqlQuantity b) when a.Unit == b.Unit => a.Value == b.Value,
            (CqlQuantity a, CqlQuantity b) => ConvertToCommonUnit(a, b) is { } c && c.a == c.b,
            (CqlInterval a, CqlInterval b) => Equal(a.Low, b.Low) && Equal(a.High, b.High)
                && a.LowClosed == b.LowClosed && a.HighClosed == b.HighClosed,
            (List<object?> a, List<object?> b) => ListEqual(a, b),
            (CqlTuple a, CqlTuple b) => CqlTuple.TupleEqual(a, b) == true,
            _ => Equals(left, right),
        };
    }

    static bool ListEqual(List<object?> a, List<object?> b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
            if (!Equal(a[i], b[i])) return false;
        return true;
    }

    public static int? Compare(object? left, object? right)
    {
        if (left is null || right is null) return null;
        return (left, right) switch
        {
            (int a, int b) => a.CompareTo(b),
            (long a, long b) => a.CompareTo(b),
            (int a, long b) => ((long)a).CompareTo(b),
            (long a, int b) => a.CompareTo((long)b),
            (decimal a, decimal b) => a.CompareTo(b),
            (decimal a, int b) => a.CompareTo((decimal)b),
            (int a, decimal b) => ((decimal)a).CompareTo(b),
            (decimal a, long b) => a.CompareTo((decimal)b),
            (long a, decimal b) => ((decimal)a).CompareTo(b),
            (string a, string b) => string.Compare(a, b, StringComparison.Ordinal),
            (CqlQuantity a, CqlQuantity b) when a.Unit == b.Unit => a.Value.CompareTo(b.Value),
            (CqlQuantity a, CqlQuantity b) => CompareQuantitiesWithConversion(a, b),
            (CqlDateTime a, CqlDateTime b) => CompareDateTimes(a, b),
            (CqlDate a, CqlDate b) => CompareDates(a, b),
            (CqlTime a, CqlTime b) => CompareTimes(a, b),
            _ => null,
        };
    }

    static int? CompareDateTimes(CqlDateTime a, CqlDateTime b)
    {
        if (a.TimezoneOffset is not null && b.TimezoneOffset is not null)
        {
            a = a.ToUtc();
            b = b.ToUtc();
        }
        var cmp = a.Year.CompareTo(b.Year);
        if (cmp != 0) return cmp;
        if (a.Month is null || b.Month is null) return a.Month == b.Month ? 0 : null;
        cmp = a.Month.Value.CompareTo(b.Month.Value);
        if (cmp != 0) return cmp;
        if (a.Day is null || b.Day is null) return a.Day == b.Day ? 0 : null;
        cmp = a.Day.Value.CompareTo(b.Day.Value);
        if (cmp != 0) return cmp;
        if (a.Hour is null || b.Hour is null) return a.Hour == b.Hour ? 0 : null;
        cmp = a.Hour.Value.CompareTo(b.Hour.Value);
        if (cmp != 0) return cmp;
        if (a.Minute is null || b.Minute is null) return a.Minute == b.Minute ? 0 : null;
        cmp = a.Minute.Value.CompareTo(b.Minute.Value);
        if (cmp != 0) return cmp;
        if (a.Second is null || b.Second is null) return a.Second == b.Second ? 0 : null;
        cmp = a.Second.Value.CompareTo(b.Second.Value);
        if (cmp != 0) return cmp;
        if (a.Millisecond is null || b.Millisecond is null) return a.Millisecond == b.Millisecond ? 0 : null;
        return a.Millisecond.Value.CompareTo(b.Millisecond.Value);
    }

    static int? CompareDates(CqlDate a, CqlDate b)
    {
        var cmp = a.Year.CompareTo(b.Year);
        if (cmp != 0) return cmp;
        if (a.Month is null || b.Month is null) return a.Month == b.Month ? 0 : null;
        cmp = a.Month.Value.CompareTo(b.Month.Value);
        if (cmp != 0) return cmp;
        if (a.Day is null || b.Day is null) return a.Day == b.Day ? 0 : null;
        return a.Day.Value.CompareTo(b.Day.Value);
    }

    static int? CompareTimes(CqlTime a, CqlTime b)
    {
        var cmp = a.Hour.CompareTo(b.Hour);
        if (cmp != 0) return cmp;
        if (a.Minute is null || b.Minute is null) return a.Minute == b.Minute ? 0 : null;
        cmp = a.Minute.Value.CompareTo(b.Minute.Value);
        if (cmp != 0) return cmp;
        if (a.Second is null || b.Second is null) return a.Second == b.Second ? 0 : null;
        cmp = a.Second.Value.CompareTo(b.Second.Value);
        if (cmp != 0) return cmp;
        if (a.Millisecond is null || b.Millisecond is null) return a.Millisecond == b.Millisecond ? 0 : null;
        return a.Millisecond.Value.CompareTo(b.Millisecond.Value);
    }

    public static object? TruncateToPrecision(object? value, Elm.DateTimePrecision precision)
    {
        return value switch
        {
            CqlDateTime dt => precision switch
            {
                Elm.DateTimePrecision.Year => new CqlDateTime(dt.Year, TimezoneOffset: dt.TimezoneOffset),
                Elm.DateTimePrecision.Month => new CqlDateTime(dt.Year, dt.Month, TimezoneOffset: dt.TimezoneOffset),
                Elm.DateTimePrecision.Day => new CqlDateTime(dt.Year, dt.Month, dt.Day, TimezoneOffset: dt.TimezoneOffset),
                Elm.DateTimePrecision.Hour => new CqlDateTime(dt.Year, dt.Month, dt.Day, dt.Hour, TimezoneOffset: dt.TimezoneOffset),
                Elm.DateTimePrecision.Minute => new CqlDateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, TimezoneOffset: dt.TimezoneOffset),
                Elm.DateTimePrecision.Second => new CqlDateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, TimezoneOffset: dt.TimezoneOffset),
                Elm.DateTimePrecision.Millisecond => dt,
                _ => dt,
            },
            CqlDate d => precision switch
            {
                Elm.DateTimePrecision.Year => new CqlDate(d.Year),
                Elm.DateTimePrecision.Month => new CqlDate(d.Year, d.Month),
                Elm.DateTimePrecision.Day => d,
                _ => d,
            },
            CqlTime t => precision switch
            {
                Elm.DateTimePrecision.Hour => new CqlTime(t.Hour),
                Elm.DateTimePrecision.Minute => new CqlTime(t.Hour, t.Minute),
                Elm.DateTimePrecision.Second => new CqlTime(t.Hour, t.Minute, t.Second),
                Elm.DateTimePrecision.Millisecond => t,
                _ => t,
            },
            _ => value,
        };
    }

    static int? CompareQuantitiesWithConversion(CqlQuantity a, CqlQuantity b)
    {
        var converted = ConvertToCommonUnit(a, b);
        if (converted is null) return null;
        return converted.Value.a.CompareTo(converted.Value.b);
    }

    public static (decimal a, decimal b)? ConvertToCommonUnit(CqlQuantity a, CqlQuantity b)
    {
        var factorA = GetUcumFactor(a.Unit);
        var factorB = GetUcumFactor(b.Unit);
        if (factorA is null || factorB is null) return null;
        if (factorA.Value.baseUnit != factorB.Value.baseUnit) return null;
        return (a.Value * factorA.Value.factor, b.Value * factorB.Value.factor);
    }

    static (decimal factor, string baseUnit)? GetUcumFactor(string unit) => unit switch
    {
        "m" => (1m, "m"),
        "cm" => (0.01m, "m"),
        "mm" => (0.001m, "m"),
        "km" => (1000m, "m"),
        "g" => (1m, "g"),
        "kg" => (1000m, "g"),
        "mg" => (0.001m, "g"),
        _ => null,
    };
}

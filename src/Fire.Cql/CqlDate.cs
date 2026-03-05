namespace Fire.Cql;

public sealed record CqlDate(int Year, int? Month = null, int? Day = null)
{
    public override string ToString()
    {
        var s = $"@{Year:D4}";
        if (Month is null) return s;
        s += $"-{Month:D2}";
        if (Day is null) return s;
        s += $"-{Day:D2}";
        return s;
    }
}

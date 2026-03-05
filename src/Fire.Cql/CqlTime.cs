namespace Fire.Cql;

public sealed record CqlTime(int Hour, int? Minute = null, int? Second = null, int? Millisecond = null)
{
    public override string ToString()
    {
        var s = $"@T{Hour:D2}";
        if (Minute is null) return s;
        s += $":{Minute:D2}";
        if (Second is null) return s;
        s += $":{Second:D2}";
        if (Millisecond is null) return s;
        s += $".{Millisecond:D3}";
        return s;
    }
}

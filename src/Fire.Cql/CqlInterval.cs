namespace Fire.Cql;

public sealed record CqlInterval(object? Low, object? High, bool LowClosed, bool HighClosed)
{
    public override string ToString()
    {
        var open = LowClosed ? "[" : "(";
        var close = HighClosed ? "]" : ")";
        return $"Interval{open}{Low}, {High}{close}";
    }
}

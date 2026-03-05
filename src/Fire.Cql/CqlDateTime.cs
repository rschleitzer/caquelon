namespace Fire.Cql;

public sealed record CqlDateTime(int Year, int? Month = null, int? Day = null,
    int? Hour = null, int? Minute = null, int? Second = null, int? Millisecond = null,
    decimal? TimezoneOffset = null)
{
    public override string ToString()
    {
        var s = $"@{Year:D4}";
        if (Month is null) return s + "T";
        s += $"-{Month:D2}";
        if (Day is null) return s + "T";
        s += $"-{Day:D2}T";
        if (Hour is null) return s;
        s += $"{Hour:D2}";
        if (Minute is null) return s;
        s += $":{Minute:D2}";
        if (Second is null) return s;
        s += $":{Second:D2}";
        if (Millisecond is null) return s;
        s += $".{Millisecond:D3}";
        return s;
    }

    /// <summary>
    /// Returns a new CqlDateTime normalized to UTC if TimezoneOffset is set.
    /// </summary>
    public CqlDateTime ToUtc()
    {
        if (TimezoneOffset is null || Hour is null) return this;
        var dto = new DateTimeOffset(Year, Month ?? 1, Day ?? 1,
            Hour ?? 0, Minute ?? 0, Second ?? 0, Millisecond ?? 0,
            TimeSpan.FromHours((double)TimezoneOffset.Value));
        var utc = dto.UtcDateTime;
        return new CqlDateTime(utc.Year, Month is not null ? utc.Month : null,
            Day is not null ? utc.Day : null, utc.Hour,
            Minute is not null ? utc.Minute : null,
            Second is not null ? utc.Second : null,
            Millisecond is not null ? utc.Millisecond : null);
    }
}

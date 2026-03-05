namespace Fire.Cql;

public sealed class CqlTuple
{
    public string? TypeName { get; set; }
    public Dictionary<string, object?> Elements { get; } = new();

    public override int GetHashCode() => Elements.Count;

    public override bool Equals(object? obj)
    {
        if (obj is not CqlTuple other) return false;
        return TupleEqual(this, other) == true;
    }

    /// <summary>
    /// CQL tuple equality with three-valued logic.
    /// Returns true if all elements are equal, false if any differ, null if any comparison is null with no false.
    /// </summary>
    public static bool? TupleEqual(CqlTuple a, CqlTuple b)
    {
        if (a.Elements.Count != b.Elements.Count) return false;
        bool hasNull = false;
        foreach (var (key, value) in a.Elements)
        {
            if (!b.Elements.TryGetValue(key, out var otherValue)) return false;
            if (value is null || otherValue is null)
            {
                if (value is null && otherValue is null) continue;
                hasNull = true;
                continue;
            }
            if (!CqlComparison.Equal(value, otherValue)) return false;
        }
        return hasNull ? null : true;
    }
}

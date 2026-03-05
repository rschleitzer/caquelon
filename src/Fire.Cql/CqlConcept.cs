namespace Fire.Cql;

public sealed class CqlConcept(List<CqlCode> codes, string? display = null)
{
    public List<CqlCode> Codes { get; } = codes;
    public string? Display { get; } = display;

    public override bool Equals(object? obj)
    {
        if (obj is not CqlConcept other) return false;
        if (Codes.Count != other.Codes.Count) return false;
        for (int i = 0; i < Codes.Count; i++)
            if (!Codes[i].Equals(other.Codes[i])) return false;
        return true;
    }

    public override int GetHashCode() => Codes.Count;
}

namespace Fire.Cql;

public sealed class CqlCode(string code, string? system = null, string? display = null)
{
    public string CodeValue { get; } = code;
    public string? System { get; } = system;
    public string? Display { get; } = display;

    public override bool Equals(object? obj) =>
        obj is CqlCode other && CodeValue == other.CodeValue && System == other.System;

    public override int GetHashCode() => CodeValue.GetHashCode();
}

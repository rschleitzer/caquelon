namespace Fire.Cql;

public sealed record CqlQuantity(decimal Value, string Unit)
{
    public override string ToString() => $"{Value} '{Unit}'";
}

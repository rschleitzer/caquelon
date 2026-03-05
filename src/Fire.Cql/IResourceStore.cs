namespace Fire.Cql;

public interface IResourceStore
{
    List<ITypedElement> Retrieve(string resourceType);
    List<ITypedElement> Retrieve(string resourceType, Dictionary<string, string> searchParams)
        => Retrieve(resourceType);
}

public interface ITypedElement
{
    string TypeName { get; }
    object? Value { get; }
    ITypedElement? Property(string name);
    List<ITypedElement>? PropertyList(string name);
    ITypedElement? Index(int index);
}

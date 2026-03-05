namespace Fire.Cql;

public class FhirResource : ITypedElement
{
    readonly Dictionary<string, object?> _properties = new();
    public string TypeName { get; }
    public object? Value => null;

    public FhirResource(string typeName) => TypeName = typeName;

    public FhirResource Set(string name, object? value)
    {
        _properties[name] = value;
        return this;
    }

    public ITypedElement? Property(string name) =>
        _properties.TryGetValue(name, out var val) ? Wrap(val) : null;

    public List<ITypedElement>? PropertyList(string name) =>
        _properties.TryGetValue(name, out var val) ? WrapList(val) : null;

    public ITypedElement? Index(int index) => null;

    static ITypedElement? Wrap(object? val) => val switch
    {
        null => null,
        ITypedElement te => te,
        List<object?> list => new ListElement(list),
        List<ITypedElement> list => new ListElement(list.Cast<object?>().ToList()),
        _ => new PrimitiveElement(val),
    };

    static List<ITypedElement>? WrapList(object? val) => val switch
    {
        List<ITypedElement> list => list,
        List<object?> list => list.Where(x => x is not null).Select(x => Wrap(x)!).ToList(),
        null => null,
        _ => [Wrap(val)!],
    };
}

public class PrimitiveElement : ITypedElement
{
    public PrimitiveElement(object? value) => Value = value;
    public string TypeName => Value?.GetType().Name ?? "Null";
    public object? Value { get; }
    public ITypedElement? Property(string name) => null;
    public List<ITypedElement>? PropertyList(string name) => null;
    public ITypedElement? Index(int index) => null;
}

public class ListElement : ITypedElement
{
    readonly List<object?> _items;
    public ListElement(List<object?> items) => _items = items;
    public string TypeName => "List";
    public object? Value => _items;
    public ITypedElement? Property(string name) => null;
    public List<ITypedElement>? PropertyList(string name) => null;
    public ITypedElement? Index(int index) =>
        index >= 0 && index < _items.Count
            ? _items[index] switch { ITypedElement te => te, var v => new PrimitiveElement(v) }
            : null;
}

public class InMemoryResourceStore : IResourceStore
{
    readonly Dictionary<string, List<ITypedElement>> _resources = new();

    public void Add(ITypedElement resource)
    {
        var type = resource.TypeName;
        if (!_resources.TryGetValue(type, out var list))
        {
            list = [];
            _resources[type] = list;
        }
        list.Add(resource);
    }

    public List<ITypedElement> Retrieve(string resourceType) =>
        _resources.TryGetValue(resourceType, out var list) ? list : [];
}

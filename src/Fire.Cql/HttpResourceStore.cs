using System.Text.Json;

namespace Fire.Cql;

public class HttpResourceStore : IResourceStore
{
    readonly HttpClient _client;
    readonly string _baseUrl;

    public HttpResourceStore(string baseUrl, HttpClient? client = null)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _client = client ?? new HttpClient();
        if (_client.DefaultRequestHeaders.Accept.Count == 0)
            _client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/fhir+json"));
    }

    public List<ITypedElement> Retrieve(string resourceType)
        => Retrieve(resourceType, new Dictionary<string, string>());

    public List<ITypedElement> Retrieve(string resourceType, Dictionary<string, string> searchParams)
    {
        var url = $"{_baseUrl}/{Uri.EscapeDataString(resourceType)}";
        if (searchParams.Count > 0)
        {
            var query = string.Join("&", searchParams.Select(
                kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            url = $"{url}?{query}";
        }
        var response = _client.GetAsync(url).GetAwaiter().GetResult();
        if (!response.IsSuccessStatusCode)
            return [];
        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        return ParseBundle(json);
    }

    static List<ITypedElement> ParseBundle(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.TryGetProperty("entry", out var entries) && entries.ValueKind == JsonValueKind.Array)
        {
            var results = new List<ITypedElement>();
            foreach (var entry in entries.EnumerateArray())
                if (entry.TryGetProperty("resource", out var resource) && resource.ValueKind == JsonValueKind.Object)
                    results.Add(ToTypedElement(resource));
            return results;
        }

        if (root.TryGetProperty("resourceType", out _) && root.ValueKind == JsonValueKind.Object)
            return [ToTypedElement(root)];

        return [];
    }

    static ITypedElement ToTypedElement(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Object => ObjectToElement(element),
        JsonValueKind.Array => new ListElement(
            element.EnumerateArray().Select(e => (object?)ToTypedElement(e)).ToList()),
        JsonValueKind.String => new PrimitiveElement(element.GetString()),
        JsonValueKind.Number => element.TryGetInt32(out var i) ? new PrimitiveElement(i)
            : new PrimitiveElement(element.GetDecimal()),
        JsonValueKind.True => new PrimitiveElement(true),
        JsonValueKind.False => new PrimitiveElement(false),
        _ => new PrimitiveElement(null),
    };

    static FhirResource ObjectToElement(JsonElement obj)
    {
        var typeName = obj.TryGetProperty("resourceType", out var rt) ? rt.GetString() ?? "Object" : "Object";
        var resource = new FhirResource(typeName);
        foreach (var prop in obj.EnumerateObject())
        {
            if (prop.Name == "resourceType") continue;
            resource.Set(prop.Name, prop.Value.ValueKind switch
            {
                JsonValueKind.Array => prop.Value.EnumerateArray()
                    .Select(e => (ITypedElement)ToTypedElement(e)).ToList(),
                JsonValueKind.Object => ToTypedElement(prop.Value),
                _ => ToTypedElement(prop.Value),
            });
        }
        return resource;
    }
}

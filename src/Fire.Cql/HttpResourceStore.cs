using System.Text.Json;

namespace Fire.Cql;

public class HttpResourceStore : IResourceStore
{
    readonly HttpClient _client;
    readonly string _baseUrl;
    // Maps propertyPath → searchParamName, per resource type
    readonly Dictionary<string, Dictionary<string, string>> _pathToParam = new();

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
            var resolved = ResolveSearchParams(resourceType, searchParams);
            if (resolved.Count > 0)
            {
                var query = string.Join("&", resolved.Select(
                    kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
                url = $"{url}?{query}";
            }
        }
        var response = _client.GetAsync(url).GetAwaiter().GetResult();
        if (!response.IsSuccessStatusCode)
            return [];
        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        return ParseBundle(json);
    }

    Dictionary<string, string> ResolveSearchParams(string resourceType, Dictionary<string, string> propertyParams)
    {
        var mapping = GetSearchParamMapping(resourceType);
        var resolved = new Dictionary<string, string>();
        foreach (var (path, value) in propertyParams)
        {
            if (mapping.TryGetValue(path, out var paramName))
                resolved[paramName] = value;
        }
        return resolved;
    }

    Dictionary<string, string> GetSearchParamMapping(string resourceType)
    {
        if (_pathToParam.TryGetValue(resourceType, out var cached))
            return cached;
        var mapping = LoadSearchParameters(resourceType);
        _pathToParam[resourceType] = mapping;
        return mapping;
    }

    Dictionary<string, string> LoadSearchParameters(string resourceType)
    {
        var result = new Dictionary<string, string>();
        try
        {
            var url = $"{_baseUrl}/SearchParameter?base={Uri.EscapeDataString(resourceType)}";
            var response = _client.GetAsync(url).GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode) return result;
            var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("entry", out var entries)) return result;
            foreach (var entry in entries.EnumerateArray())
            {
                var sp = entry.TryGetProperty("resource", out var r) ? r : entry;
                if (!sp.TryGetProperty("code", out var code)) continue;
                if (!sp.TryGetProperty("expression", out var expr)) continue;
                var paramName = code.GetString() ?? "";
                var expression = expr.GetString() ?? "";

                // Expression like "Patient.name.family | Practitioner.name.family"
                foreach (var part in expression.Split('|'))
                {
                    var trimmed = part.Trim();
                    var prefix = resourceType + ".";
                    if (!trimmed.StartsWith(prefix)) continue;
                    var propertyPath = trimmed[prefix.Length..];
                    result.TryAdd(propertyPath, paramName);
                }
            }
        }
        catch { }
        return result;
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

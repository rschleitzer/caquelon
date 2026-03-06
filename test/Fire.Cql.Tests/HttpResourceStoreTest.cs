using System.Net;
using System.Text;
using Xunit;

namespace Fire.Cql.Tests;

public class HttpResourceStoreTest
{
    const string SearchParameters = """
    {
        "resourceType": "Bundle",
        "entry": [
            {"resource": {"resourceType": "SearchParameter", "code": "active", "base": ["Patient"], "expression": "Patient.active"}},
            {"resource": {"resourceType": "SearchParameter", "code": "gender", "base": ["Patient"], "expression": "Patient.gender"}},
            {"resource": {"resourceType": "SearchParameter", "code": "birthdate", "base": ["Patient"], "expression": "Patient.birthDate"}},
            {"resource": {"resourceType": "SearchParameter", "code": "family", "base": ["Patient"], "expression": "Patient.name.family | Practitioner.name.family"}},
            {"resource": {"resourceType": "SearchParameter", "code": "status", "base": ["Observation"], "expression": "Observation.status"}}
        ]
    }
    """;

    static int _counter;
    static MockHandler CreateHandler(string bundleJson) => new(bundleJson, SearchParameters);
    static string UniqueUrl() => $"http://fhir{Interlocked.Increment(ref _counter)}.example.com";

    [Fact]
    public void RetrieveFromBundle()
    {
        var json = """
        {
            "resourceType": "Bundle",
            "entry": [
                {"resource": {"resourceType": "Patient", "active": true}},
                {"resource": {"resourceType": "Patient", "active": false}}
            ]
        }
        """;
        var handler = CreateHandler(json);
        var store = new HttpResourceStore(UniqueUrl(), new HttpClient(handler));
        var result = ElmInterpreter.Evaluate("exists [Patient] P where P.active = true", store);
        Assert.Equal(true, result);
    }

    [Fact]
    public void PropertyNavigation()
    {
        var json = """
        {
            "resourceType": "Bundle",
            "entry": [{"resource": {
                "resourceType": "Patient",
                "name": [{"family": "Doe", "given": ["Jane"]}]
            }}]
        }
        """;
        var handler = CreateHandler(json);
        var store = new HttpResourceStore(UniqueUrl(), new HttpClient(handler));
        var result = ElmInterpreter.Evaluate(
            "exists [Patient] P where P.name[0].family = 'Doe'", store);
        Assert.Equal(true, result);
    }

    [Fact]
    public void EmptyBundle()
    {
        var handler = CreateHandler("""{"resourceType": "Bundle", "entry": []}""");
        var store = new HttpResourceStore(UniqueUrl(), new HttpClient(handler));
        var result = ElmInterpreter.Evaluate("exists [Patient]", store);
        Assert.Equal(false, result);
    }

    [Fact]
    public void ServerError()
    {
        var handler = new MockHandler("", SearchParameters, HttpStatusCode.InternalServerError);
        var store = new HttpResourceStore(UniqueUrl(), new HttpClient(handler));
        var result = ElmInterpreter.Evaluate("exists [Patient]", store);
        Assert.Equal(false, result);
    }

    [Fact]
    public void PushdownDirectProperty()
    {
        var json = """
        {
            "resourceType": "Bundle",
            "entry": [{"resource": {"resourceType": "Patient", "active": true, "gender": "male"}}]
        }
        """;
        var handler = CreateHandler(json);
        var store = new HttpResourceStore(UniqueUrl(), new HttpClient(handler));
        ElmInterpreter.Evaluate(
            "exists [Patient] P where P.active = true and P.gender = 'male'", store);
        var searchUrl = handler.ResourceUrls.Single();
        Assert.Contains("active=true", searchUrl);
        Assert.Contains("gender=male", searchUrl);
    }

    [Fact]
    public void PushdownCamelCaseProperty()
    {
        // P.birthDate maps to "birthdate" via expression "Patient.birthDate"
        var json = """
        {
            "resourceType": "Bundle",
            "entry": [{"resource": {"resourceType": "Patient", "birthDate": "1990-01-01"}}]
        }
        """;
        var handler = CreateHandler(json);
        var store = new HttpResourceStore(UniqueUrl(), new HttpClient(handler));
        ElmInterpreter.Evaluate(
            "exists [Patient] P where P.birthDate = '1990-01-01'", store);
        var searchUrl = handler.ResourceUrls.Single();
        Assert.Contains("birthdate=1990-01-01", searchUrl);
    }

    [Fact]
    public void PushdownUnknownPropertySkipped()
    {
        var json = """
        {
            "resourceType": "Bundle",
            "entry": [{"resource": {"resourceType": "Patient", "someField": "x"}}]
        }
        """;
        var handler = CreateHandler(json);
        var store = new HttpResourceStore(UniqueUrl(), new HttpClient(handler));
        ElmInterpreter.Evaluate(
            "exists [Patient] P where P.someField = 'x'", store);
        var searchUrl = handler.ResourceUrls.Single();
        Assert.DoesNotContain("?", searchUrl);
    }

    [Fact]
    public void PushdownNestedPropertyPath()
    {
        // P.name[0].family should resolve path "name.family" → search param "family"
        var json = """
        {
            "resourceType": "Bundle",
            "entry": [{"resource": {
                "resourceType": "Patient",
                "name": [{"family": "Smith"}]
            }}]
        }
        """;
        var handler = CreateHandler(json);
        var store = new HttpResourceStore(UniqueUrl(), new HttpClient(handler));
        ElmInterpreter.Evaluate(
            "exists [Patient] P where P.name[0].family = 'Smith'", store);
        var searchUrl = handler.ResourceUrls.Single();
        Assert.Contains("family=Smith", searchUrl);
    }

    [Fact]
    public void SearchParametersFetchedOncePerResourceType()
    {
        var json = """
        {
            "resourceType": "Bundle",
            "entry": [{"resource": {"resourceType": "Patient", "active": true}}]
        }
        """;
        var handler = CreateHandler(json);
        var store = new HttpResourceStore(UniqueUrl(), new HttpClient(handler));
        ElmInterpreter.Evaluate("exists [Patient] P where P.active = true", store);
        ElmInterpreter.Evaluate("exists [Patient] P where P.gender = 'male'", store);
        // SearchParameter?base=Patient fetched once, two resource queries
        Assert.Single(handler.SearchParameterUrls);
        Assert.Contains("base=Patient", handler.SearchParameterUrls[0]);
        Assert.Equal(2, handler.ResourceUrls.Count);
    }

    class MockHandler : HttpMessageHandler
    {
        readonly string _bundleJson;
        readonly string _searchParamJson;
        readonly HttpStatusCode _resourceStatus;
        public List<string> ResourceUrls { get; } = new();
        public List<string> SearchParameterUrls { get; } = new();

        public MockHandler(string bundleJson, string searchParamJson,
            HttpStatusCode resourceStatus = HttpStatusCode.OK)
        {
            _bundleJson = bundleJson;
            _searchParamJson = searchParamJson;
            _resourceStatus = resourceStatus;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var url = request.RequestUri?.ToString() ?? "";
            if (url.Contains("/SearchParameter"))
            {
                SearchParameterUrls.Add(url);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_searchParamJson, Encoding.UTF8, "application/fhir+json"),
                });
            }

            ResourceUrls.Add(url);
            return Task.FromResult(new HttpResponseMessage(_resourceStatus)
            {
                Content = new StringContent(_bundleJson, Encoding.UTF8, "application/fhir+json"),
            });
        }
    }
}

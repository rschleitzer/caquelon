using System.Net;
using System.Text;
using Xunit;

namespace Fire.Cql.Tests;

public class HttpResourceStoreTest
{
    static HttpClient MockClient(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        var handler = new MockHandler(json, status);
        return new HttpClient(handler);
    }

    [Fact]
    public void RetrieveFromBundle()
    {
        var json = """
        {
            "resourceType": "Bundle",
            "type": "searchset",
            "entry": [
                {
                    "resource": {
                        "resourceType": "Patient",
                        "id": "1",
                        "active": true,
                        "name": [{"family": "Smith", "given": ["John"]}]
                    }
                },
                {
                    "resource": {
                        "resourceType": "Patient",
                        "id": "2",
                        "active": false
                    }
                }
            ]
        }
        """;
        var store = new HttpResourceStore("http://fhir.example.com", MockClient(json));
        var result = ElmInterpreter.Evaluate("exists [Patient] P where P.active = true", store);
        Assert.Equal(true, result);
    }

    [Fact]
    public void PropertyNavigation()
    {
        var json = """
        {
            "resourceType": "Bundle",
            "entry": [{
                "resource": {
                    "resourceType": "Patient",
                    "name": [{"family": "Doe", "given": ["Jane"]}]
                }
            }]
        }
        """;
        var store = new HttpResourceStore("http://fhir.example.com", MockClient(json));
        var result = ElmInterpreter.Evaluate(
            "exists [Patient] P where P.name[0].family = 'Doe'", store);
        Assert.Equal(true, result);
    }

    [Fact]
    public void NumericProperty()
    {
        var json = """
        {
            "resourceType": "Bundle",
            "entry": [{
                "resource": {
                    "resourceType": "Observation",
                    "status": "final",
                    "valueInteger": 120
                }
            }]
        }
        """;
        var store = new HttpResourceStore("http://fhir.example.com", MockClient(json));
        var result = ElmInterpreter.Evaluate(
            "exists [Observation] O where O.valueInteger = 120", store);
        Assert.Equal(true, result);
    }

    [Fact]
    public void EmptyBundle()
    {
        var json = """{"resourceType": "Bundle", "entry": []}""";
        var store = new HttpResourceStore("http://fhir.example.com", MockClient(json));
        var result = ElmInterpreter.Evaluate("exists [Patient]", store);
        Assert.Equal(false, result);
    }

    [Fact]
    public void ServerError()
    {
        var store = new HttpResourceStore("http://fhir.example.com",
            MockClient("", HttpStatusCode.InternalServerError));
        var result = ElmInterpreter.Evaluate("exists [Patient]", store);
        Assert.Equal(false, result);
    }

    [Fact]
    public void SearchParamPushdown()
    {
        var json = """
        {
            "resourceType": "Bundle",
            "entry": [{
                "resource": {
                    "resourceType": "Patient",
                    "active": true,
                    "gender": "male"
                }
            }]
        }
        """;
        var handler = new MockHandler(json, HttpStatusCode.OK);
        var store = new HttpResourceStore("http://fhir.example.com", new HttpClient(handler));
        ElmInterpreter.Evaluate(
            "exists [Patient] P where P.active = true and P.gender = 'male'", store);
        Assert.Single(handler.AllUrls);
        Assert.Contains("active=true", handler.LastUrl!);
        Assert.Contains("gender=male", handler.LastUrl!);
    }

    [Fact]
    public void SearchParamPushdownSingleCondition()
    {
        var json = """
        {
            "resourceType": "Bundle",
            "entry": [{
                "resource": { "resourceType": "Patient", "gender": "female" }
            }]
        }
        """;
        var handler = new MockHandler(json, HttpStatusCode.OK);
        var store = new HttpResourceStore("http://fhir.example.com", new HttpClient(handler));
        ElmInterpreter.Evaluate(
            "exists [Patient] P where P.gender = 'female'", store);
        Assert.Contains("gender=female", handler.LastUrl);
    }

    class MockHandler(string json, HttpStatusCode status) : HttpMessageHandler
    {
        public string? LastUrl { get; private set; }
        public int CallCount { get; private set; }
        public List<string> AllUrls { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastUrl = request.RequestUri?.ToString();
            AllUrls.Add(LastUrl ?? "");
            CallCount++;
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/fhir+json"),
            });
        }
    }
}

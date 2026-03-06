using System.Net.Http;
using System.Text;
using Xunit;

namespace Fire.Cql.Tests;

public class FireIntegrationTest : IAsyncLifetime
{
    const string BaseUrl = "http://localhost:5080/fire";
    static readonly HttpClient Http = new();

    static HttpResourceStore Store() => new(BaseUrl);

    public async Task InitializeAsync()
    {
        await CreateResource("Patient", """
            {"resourceType":"Patient","active":true,"gender":"male",
             "name":[{"family":"Jackson","given":["Samuel"]}]}
            """);
        await CreateResource("Observation", """
            {"resourceType":"Observation","status":"final",
             "code":{"coding":[{"system":"http://loinc.org","code":"85354-9"}]}}
            """);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static async Task CreateResource(string type, string json)
    {
        var content = new StringContent(json, Encoding.UTF8, "application/fhir+json");
        await Http.PostAsync($"{BaseUrl}/{type}", content);
    }

    [Fact]
    public void ExistsPatient()
    {
        var result = ElmInterpreter.Evaluate("exists [Patient]", Store());
        Assert.Equal(true, result);
    }

    [Fact]
    public void ExistsPatientWithActiveTrue()
    {
        var result = ElmInterpreter.Evaluate(
            "exists [Patient] P where P.active = true", Store());
        Assert.Equal(true, result);
    }

    [Fact]
    public void ExistsPatientWithGender()
    {
        var result = ElmInterpreter.Evaluate(
            "exists [Patient] P where P.gender = 'male'", Store());
        Assert.Equal(true, result);
    }

    [Fact]
    public void ExistsPatientByFamilyName()
    {
        var result = ElmInterpreter.Evaluate(
            "exists [Patient] P where P.name[0].family = 'Jackson'", Store());
        Assert.Equal(true, result);
    }

    [Fact]
    public void ExistsPatientWithFamilyNotNull()
    {
        var result = ElmInterpreter.Evaluate(
            "exists [Patient] P where P.name[0].family is not null", Store());
        Assert.Equal(true, result);
    }

    [Fact]
    public void ExistsObservation()
    {
        var result = ElmInterpreter.Evaluate("exists [Observation]", Store());
        Assert.Equal(true, result);
    }

    [Fact]
    public void ExistsNonexistentResource()
    {
        var result = ElmInterpreter.Evaluate("exists [MedicationRequest]", Store());
        Assert.Equal(false, result);
    }
}

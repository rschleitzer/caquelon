using Xunit;

namespace Fire.Cql.Tests;

public class FireIntegrationTest
{
    const string BaseUrl = "http://localhost:5080/fire";

    static HttpResourceStore Store() => new(BaseUrl);

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

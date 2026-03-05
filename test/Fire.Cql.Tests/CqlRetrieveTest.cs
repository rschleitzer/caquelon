using Xunit;

namespace Fire.Cql.Tests;

public class CqlRetrieveTest
{
    static InMemoryResourceStore CreateStore()
    {
        var store = new InMemoryResourceStore();

        store.Add(new FhirResource("Patient")
            .Set("active", true)
            .Set("birthDate", "1990-01-01")
            .Set("name", new List<ITypedElement>
            {
                new FhirResource("HumanName")
                    .Set("family", "Smith")
                    .Set("given", "John"),
            }));

        store.Add(new FhirResource("Patient")
            .Set("active", false)
            .Set("birthDate", "1985-06-15")
            .Set("name", new List<ITypedElement>
            {
                new FhirResource("HumanName")
                    .Set("family", "Doe")
                    .Set("given", "Jane"),
            }));

        store.Add(new FhirResource("Observation")
            .Set("status", "final")
            .Set("value", 120));

        return store;
    }

    [Fact]
    public void ExistsPatient()
    {
        var store = CreateStore();
        var result = ElmInterpreter.Evaluate("exists [Patient]", store);
        Assert.Equal(true, result);
    }

    [Fact]
    public void ExistsEmptyResourceType()
    {
        var store = new InMemoryResourceStore();
        var result = ElmInterpreter.Evaluate("exists [Patient]", store);
        Assert.Equal(false, result);
    }

    [Fact]
    public void ExistsWithWhereClause()
    {
        var store = CreateStore();
        var result = ElmInterpreter.Evaluate("exists [Patient] P where P.active = true", store);
        Assert.Equal(true, result);
    }

    [Fact]
    public void ExistsWithWhereClauseFalse()
    {
        var store = CreateStore();
        var result = ElmInterpreter.Evaluate(
            "exists [Patient] P where P.birthDate = '2099-01-01'", store);
        Assert.Equal(false, result);
    }

    [Fact]
    public void RetrieveWithPropertyNavigation()
    {
        var store = CreateStore();
        var result = ElmInterpreter.Evaluate(
            "exists [Observation] O where O.status = 'final'", store);
        Assert.Equal(true, result);
    }

    [Fact]
    public void RetrieveWithIndexedProperty()
    {
        var store = CreateStore();
        var result = ElmInterpreter.Evaluate(
            "exists [Patient] P where P.name[0].family = 'Smith'", store);
        Assert.Equal(true, result);
    }

    [Fact]
    public void RetrieveNoStore()
    {
        var result = ElmInterpreter.Evaluate("exists [Patient]");
        Assert.Equal(false, result);
    }
}

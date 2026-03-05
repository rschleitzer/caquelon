using Xunit;

namespace Fire.Cql.Tests;

public class CqlQueryTest
{
    [Fact]
    public async Task SimpleQueries()
    {
        Assert.True(await Helpers.CheckBool("(4) l = 4")); // NonListSource
        Assert.True(await Helpers.CheckBool("(4) l return 'Hello World' = 'Hello World'")); // NonListSourceWithReturn
        Assert.True(await Helpers.CheckBool("from ({2, 3}) A, ({5, 6}) B = {{ A: 2, B: 5 }, { A: 2, B: 6 }, { A: 3, B: 5 }, { A: 3, B: 6 }}")); // MultiSource
    }

    [Fact]
    public async Task Sort()
    {
        Assert.True(await Helpers.CheckBool("({1, 2, 3}) l sort desc = {3, 2, 1}")); // IntegerDescending
        Assert.True(await Helpers.CheckBool("({1, 3, 2}) l sort ascending = {1, 2, 3}")); // IntegerAscending
        Assert.True(await Helpers.CheckBool("({@2013-01-02T00:00:00.000Z, @2014-01-02T00:00:00.000Z, @2015-01-02T00:00:00.000Z}) l sort desc = {@2015-01-02T00:00:00.000Z, @2014-01-02T00:00:00.000Z, @2013-01-02T00:00:00.000Z}")); // DateTimeDescending
        Assert.True(await Helpers.CheckBool("({@2013-01-02T00:00:00.000Z, @2015-01-02T00:00:00.000Z, @2014-01-02T00:00:00.000Z}) l sort ascending = {@2013-01-02T00:00:00.000Z, @2014-01-02T00:00:00.000Z, @2015-01-02T00:00:00.000Z}")); // DateTimeAscending
    }

    [Fact]
    public async Task Aggregate()
    {
        Assert.True(await Helpers.CheckBool("(({1, 2, 3, 3, 4}) L aggregate A starting 1: A * L) = 72")); // MultiplyIntegers
        Assert.True(await Helpers.CheckBool("(({1, 2, 3, 3, 4}) L aggregate all A starting 1: A * L) = 72")); // MultiplyIntegersAll
        Assert.True(await Helpers.CheckBool("(({1, 2, 3, 3, 4}) L aggregate distinct A starting 1: A * L) = 24")); // MultiplyIntegersDistinct
        Assert.True(await Helpers.CheckBool("(({1, 2, 3}) L aggregate A : A * L) is null")); // MultiplyIntegersNoStartingExpression
        Assert.True(await Helpers.CheckBool("(from ({1, 2, 3}) B, (4) C aggregate A : A + B + C) is null")); // Multi-Source
    }
}

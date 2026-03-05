using Xunit;

namespace Fire.Cql.Tests;

public class CqlAggregateFunctionsTest
{
    [Fact]
    public async Task AllTrue()
    {
        Assert.True(await Helpers.CheckBool("AllTrue({true,true})")); // AllTrueAllTrue
        Assert.False(await Helpers.CheckBool("AllTrue({true,false})")); // AllTrueTrueFirst
        Assert.False(await Helpers.CheckBool("AllTrue({false,true})")); // AllTrueFalseFirst
        Assert.False(await Helpers.CheckBool("AllTrue({true,false,true})")); // AllTrueAllTrueFalseTrue
        Assert.False(await Helpers.CheckBool("AllTrue({false,true,false})")); // AllTrueAllFalseTrueFalse
        Assert.True(await Helpers.CheckBool("AllTrue({null,true,true})")); // AllTrueNullFirst
        Assert.True(await Helpers.CheckBool("AllTrue({})")); // AllTrueEmptyList
        Assert.True(await Helpers.CheckBool("AllTrue(null)")); // AllTrueIsTrueWhenNull
    }

    [Fact]
    public async Task AnyTrue()
    {
        Assert.True(await Helpers.CheckBool("AnyTrue({true,true})")); // AnyTrueAllTrue
        Assert.False(await Helpers.CheckBool("AnyTrue({false,false})")); // AnyTrueAllFalse
        Assert.True(await Helpers.CheckBool("AnyTrue({true,false,true})")); // AnyTrueAllTrueFalseTrue
        Assert.True(await Helpers.CheckBool("AnyTrue({false,true,false})")); // AnyTrueAllFalseTrueFalse
        Assert.True(await Helpers.CheckBool("AnyTrue({true,false})")); // AnyTrueTrueFirst
        Assert.True(await Helpers.CheckBool("AnyTrue({false,true})")); // AnyTrueFalseFirst
        Assert.True(await Helpers.CheckBool("AnyTrue({null,true})")); // AnyTrueNullFirstThenTrue
        Assert.False(await Helpers.CheckBool("AnyTrue({null,false})")); // AnyTrueNullFirstThenFalse
        Assert.False(await Helpers.CheckBool("AnyTrue({})")); // AnyTrueEmptyList
        Assert.False(await Helpers.CheckBool("AnyTrue(null)")); // AnyTrueIsFalseWhenNull
    }

    [Fact]
    public async Task Avg()
    {
        Assert.True(await Helpers.CheckBool("Avg({ 1.0, 2.0, 3.0, 6.0 }) = 3.0")); // AvgTest1
    }

    [Fact]
    public async Task Product()
    {
        Assert.True(await Helpers.CheckBool("Product({5L, 4L, 5L}) = 100L")); // ProductLong
    }

    [Fact]
    public async Task Count()
    {
        Assert.True(await Helpers.CheckBool("Count({ 15, 5, 99, null, 1 }) = 4")); // CountTest1
        Assert.True(await Helpers.CheckBool("Count({ DateTime(2014), DateTime(2001), DateTime(2010) }) = 3")); // CountTestDateTime
        Assert.True(await Helpers.CheckBool("Count({ @T15:59:59.999, @T05:59:59.999, @T20:59:59.999 }) = 3")); // CountTestTime
        Assert.True(await Helpers.CheckBool("Count({}) = 0")); // CountTestNull
    }

    [Fact]
    public async Task Max()
    {
        Assert.True(await Helpers.CheckBool("Max({ 5, 12, 1, 15, 0, 4, 90, 44 }) = 90")); // MaxTestInteger
        Assert.True(await Helpers.CheckBool("Max({ 5L, 12L, 1L, 15L, 0L, 4L, 90L, 44L }) = 90L")); // MaxTestLong
        Assert.True(await Helpers.CheckBool("Max({ 'hi', 'bye', 'zebra' }) = 'zebra'")); // MaxTestString
        Assert.True(await Helpers.CheckBool("Max({ DateTime(2012, 10, 5), DateTime(2012, 9, 5), DateTime(2012, 10, 6) }) = @2012-10-06T")); // MaxTestDateTime
        Assert.True(await Helpers.CheckBool("Max({ @T15:59:59.999, @T05:59:59.999, @T20:59:59.999 }) = @T20:59:59.999")); // MaxTestTime
    }

    [Fact]
    public async Task Median()
    {
        Assert.True(await Helpers.CheckBool("Median({6.0, 5.0, 4.0, 3.0, 2.0, 1.0}) = 3.5")); // MedianTestDecimal
    }

    [Fact]
    public async Task Min()
    {
        Assert.True(await Helpers.CheckBool("Min({5, 12, 1, 15, 0, 4, 90, 44}) = 0")); // MinTestInteger
        Assert.True(await Helpers.CheckBool("Min({5L, 12L, 1L, 15L, 0L, 4L, 90L, 44L}) = 0L")); // MinTestLong
        Assert.True(await Helpers.CheckBool("Min({'hi', 'bye', 'zebra'}) = 'bye'")); // MinTestString
        Assert.True(await Helpers.CheckBool("Min({ DateTime(2012, 10, 5), DateTime(2012, 9, 5), DateTime(2012, 10, 6) }) = @2012-09-05T")); // MinTestDateTime
        Assert.True(await Helpers.CheckBool("Min({ @T15:59:59.999, @T05:59:59.999, @T20:59:59.999 }) = @T05:59:59.999")); // MinTestTime
    }

    [Fact]
    public async Task Mode()
    {
        Assert.True(await Helpers.CheckBool("Mode({ 2, 1, 8, 2, 9, 1, 9, 9 }) = 9")); // ModeTestInteger
        Assert.True(await Helpers.CheckBool("Mode({ DateTime(2012, 10, 5), DateTime(2012, 9, 5), DateTime(2012, 10, 6), DateTime(2012, 9, 5) }) = @2012-09-05T")); // ModeTestDateTime
        Assert.True(await Helpers.CheckBool("Mode({ DateTime(2012, 10, 5), DateTime(2012, 10, 5), DateTime(2012, 10, 6), DateTime(2012, 9, 5) }) = @2012-10-05T")); // ModeTestDateTime2
        Assert.True(await Helpers.CheckBool("Mode({ @T15:59:59.999, @T05:59:59.999, @T20:59:59.999, @T05:59:59.999 }) = @T05:59:59.999")); // ModeTestTime
    }

    [Fact]
    public async Task PopulationStdDev()
    {
        Assert.True(await Helpers.CheckBool("PopulationStdDev({ 1.0, 2.0, 3.0, 4.0, 5.0 }) = 1.41421356")); // PopStdDevTest1
        Assert.True(await Helpers.CheckBool("(PopulationStdDev({ null as Quantity, null as Quantity, null as Quantity })) is null")); // PopulationStdDevIsNull
    }

    [Fact]
    public async Task PopulationVariance()
    {
        Assert.True(await Helpers.CheckBool("PopulationVariance({ 1.0, 2.0, 3.0, 4.0, 5.0 }) = 2.0")); // PopVarianceTest1
        Assert.True(await Helpers.CheckBool("(PopulationVariance({ null as Quantity, null as Quantity, null as Quantity })) is null")); // PopVarianceIsNull
    }

    [Fact]
    public async Task StdDev()
    {
        Assert.True(await Helpers.CheckBool("StdDev({ 1.0, 2.0, 3.0, 4.0, 5.0 }) = 1.58113883")); // StdDevTest1
        Assert.True(await Helpers.CheckBool("(StdDev({ null as Quantity, null as Quantity, null as Quantity })) is null")); // StdDevIsNull
    }

    [Fact]
    public async Task Sum()
    {
        Assert.True(await Helpers.CheckBool("Sum({ 6.0, 2.0, 3.0, 4.0, 5.0 }) = 20.0")); // SumTest1
        Assert.True(await Helpers.CheckBool("Sum({ 6L, 2L, 3L, 4L, 5L }) = 20L")); // SumTestLong
        Assert.True(await Helpers.CheckBool("Sum({1 'ml',2 'ml',3 'ml',4 'ml',5 'ml'}) = 15 'ml'")); // SumTestQuantity
        Assert.True(await Helpers.CheckBool("Sum({ null, 1, null }) = 1")); // SumTestNull
    }

    [Fact]
    public async Task Variance()
    {
        Assert.True(await Helpers.CheckBool("Variance({ 1.0, 2.0, 3.0, 4.0, 5.0 }) = 2.5")); // VarianceTest1
    }
}

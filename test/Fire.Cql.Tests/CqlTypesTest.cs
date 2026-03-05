using Xunit;

namespace Fire.Cql.Tests;

public class CqlTypesTest
{
    [Fact]
    public async Task Any()
    {
        Assert.True(await Helpers.CheckBool("5.0 'g' = 5.0'g'")); // AnyQuantity
        Assert.True(await Helpers.CheckBool("DateTime(2012, 4, 4) = @2012-04-04T")); // AnyDateTime
        Assert.True(await Helpers.CheckBool("@T09:00:00.000 = @T09:00:00.000")); // AnyTime
        Assert.True(await Helpers.CheckBool("Interval[2, 7] = Interval[2, 7]")); // AnyInterval
        Assert.True(await Helpers.CheckBool("{1, 2, 3} = {1, 2, 3}")); // AnyList
        Assert.True(await Helpers.CheckBool("Tuple { id: 5, name: 'Chris'} = Tuple { id: 5, name: 'Chris'}")); // AnyTuple
        Assert.True(await Helpers.CheckBool("Tuple { id: 5, name: 'Chris'}.name = 'Chris'")); // AnyString
    }

    [Fact]
    public async Task Boolean()
    {
    }

    [Fact]
    public async Task DateTime()
    {
        Assert.True(await Helpers.CheckBool("(DateTime(null)) is null")); // DateTimeNull
        Assert.True(await Helpers.CheckBool("DateTime(10000, 12, 31, 23, 59, 59, 999) = ")); // DateTimeUpperBoundExcept
        Assert.True(await Helpers.CheckBool("DateTime(0000, 1, 1, 0, 0, 0, 0) = ")); // DateTimeLowerBoundExcept
        Assert.True(await Helpers.CheckBool("DateTime(2016, 7, 7, 6, 25, 33, 910) = @2016-07-07T06:25:33.910")); // DateTimeProper
        Assert.True(await Helpers.CheckBool("DateTime(2015, 2, 10) = @2015-02-10T")); // DateTimeIncomplete
        Assert.True(await Helpers.CheckBool("days between DateTime(2015, 2, 10) and DateTime(2015, 3) = Interval [ 18, 49 ]")); // DateTimeUncertain
        Assert.True(await Helpers.CheckBool("DateTime(0001, 1, 1, 0, 0, 0, 0) = @0001-01-01T00:00:00.000")); // DateTimeMin
        Assert.True(await Helpers.CheckBool("DateTime(9999, 12, 31, 23, 59, 59, 999) = @9999-12-31T23:59:59.999")); // DateTimeMax
        Assert.True(await Helpers.CheckBool("hour from @2015-02-10T is null")); // DateTimeTimeUnspecified
    }

    [Fact]
    public async Task Decimal()
    {
    }

    [Fact]
    public async Task Integer()
    {
    }

    [Fact]
    public async Task Interval()
    {
    }

    [Fact]
    public async Task Quantity()
    {
        Assert.True(await Helpers.CheckBool("150.2 '[lb_av]' = 150.2 '[lb_av]'")); // QuantityTest
        Assert.True(await Helpers.CheckBool("2.5589 '{eskimo kisses}' = 2.5589 '{eskimo kisses}'")); // QuantityTest2
        Assert.True(await Helpers.CheckBool("5.999999999 'g' = 5.999999999 'g'")); // QuantityFractionalTooBig
    }

    [Fact]
    public async Task String()
    {
        Assert.True(await Helpers.CheckBool("'\\'I start with a single quote and end with a double quote\\\"' = '\\u0027I start with a single quote and end with a double quote\\u0022'")); // StringTestEscapeQuotes
        Assert.True(await Helpers.CheckBool("'\\u0048\\u0069' = 'Hi'")); // StringUnicodeTest
    }

    [Fact]
    public async Task Time()
    {
        Assert.True(await Helpers.CheckBool("@T24:59:59.999 = ")); // TimeUpperBoundHours
        Assert.True(await Helpers.CheckBool("@T23:60:59.999 = ")); // TimeUpperBoundMinutes
        Assert.True(await Helpers.CheckBool("@T23:59:60.999 = ")); // TimeUpperBoundSeconds
        Assert.True(await Helpers.CheckBool("@T23:59:59.10000 = ")); // TimeUpperBoundMillis
        Assert.True(await Helpers.CheckBool("@T10:25:12.863 = @T10:25:12.863")); // TimeProper
        Assert.True(await Helpers.CheckBool("@T23:59:59.999 = @T23:59:59.999")); // TimeAllMax
        Assert.True(await Helpers.CheckBool("@T00:00:00.000 = @T00:00:00.000")); // TimeAllMin
    }
}

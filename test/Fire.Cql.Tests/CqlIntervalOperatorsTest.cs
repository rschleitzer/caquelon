using Xunit;

namespace Fire.Cql.Tests;

public class CqlIntervalOperatorsTest
{
    [Fact]
    public async Task After()
    {
        Assert.True(await Helpers.CheckBool("((null as Integer) after Interval[1, 10]) is null")); // TestAfterNull
        Assert.True(await Helpers.CheckBool("Interval[11, 20] after Interval[1, 10]")); // IntegerIntervalAfterTrue
        Assert.False(await Helpers.CheckBool("Interval[1, 10] after Interval[11, 20]")); // IntegerIntervalAfterFalse
        Assert.True(await Helpers.CheckBool("12 after Interval[1, 10]")); // IntegerIntervalPointAfterTrue
        Assert.False(await Helpers.CheckBool("9 after Interval[1, 10]")); // IntegerIntervalPointAfterFalse
        Assert.True(await Helpers.CheckBool("Interval[11, 20] after 5")); // IntegerIntervalAfterPointTrue
        Assert.False(await Helpers.CheckBool("Interval[11, 20] after 12")); // IntegerIntervalAfterPointFalse
        Assert.True(await Helpers.CheckBool("Interval[11.0, 20.0] after Interval[1.0, 10.0]")); // DecimalIntervalAfterTrue
        Assert.False(await Helpers.CheckBool("Interval[1.0, 10.0] after Interval[11.0, 20.0]")); // DecimalIntervalAfterFalse
        Assert.True(await Helpers.CheckBool("12.0 after Interval[1.0, 10.0]")); // DecimalIntervalPointAfterTrue
        Assert.False(await Helpers.CheckBool("9.0 after Interval[1.0, 10.0]")); // DecimalIntervalPointAfterFalse
        Assert.True(await Helpers.CheckBool("Interval[11.0, 20.0] after 5.0")); // DecimalIntervalAfterPointTrue
        Assert.False(await Helpers.CheckBool("Interval[11.0, 20.0] after 12.0")); // DecimalIntervalAfterPointFalse
        Assert.True(await Helpers.CheckBool("Interval[11.0 'g', 20.0 'g'] after Interval[1.0 'g', 10.0 'g']")); // QuantityIntervalAfterTrue
        Assert.False(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] after Interval[11.0 'g', 20.0 'g']")); // QuantityIntervalAfterFalse
        Assert.True(await Helpers.CheckBool("12.0'g' after Interval[1.0 'g', 10.0 'g']")); // QuantityIntervalPointAfterTrue
        Assert.False(await Helpers.CheckBool("9.0'g' after Interval[1.0 'g', 10.0 'g']")); // QuantityIntervalPointAfterFalse
        Assert.True(await Helpers.CheckBool("Interval[11.0 'g', 20.0 'g'] after 5.0'g'")); // QuantityIntervalAfterPointTrue
        Assert.False(await Helpers.CheckBool("Interval[11.0 'g', 20.0 'g'] after 12.0'g'")); // QuantityIntervalAfterPointFalse
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 1), DateTime(2012, 1, 15)] after DateTime(2011, 12, 31)")); // DateTimeAfterTrue
        Assert.False(await Helpers.CheckBool("Interval[DateTime(2012, 1, 1), DateTime(2012, 1, 15)] after DateTime(2012, 12, 31)")); // DateTimeAfterFalse
        Assert.True(await Helpers.CheckBool("Interval[@T15:59:59.999, @T20:59:59.999] after @T12:59:59.999")); // TimeAfterTrue
        Assert.False(await Helpers.CheckBool("Interval[@T15:59:59.999, @T20:59:59.999] after @T17:59:59.999")); // TimeAfterFalse
    }

    [Fact]
    public async Task Before()
    {
        Assert.True(await Helpers.CheckBool("((null as Integer) before Interval[1, 10]) is null")); // TestBeforeNull
        Assert.False(await Helpers.CheckBool("Interval[11, 20] before Interval[1, 10]")); // IntegerIntervalBeforeFalse
        Assert.True(await Helpers.CheckBool("Interval[1, 10] before Interval[11, 20]")); // IntegerIntervalBeforeTrue
        Assert.True(await Helpers.CheckBool("9 before Interval[11, 20]")); // IntegerIntervalPointBeforeTrue
        Assert.False(await Helpers.CheckBool("9 before Interval[1, 10]")); // IntegerIntervalPointBeforeFalse
        Assert.True(await Helpers.CheckBool("Interval[1, 10] before 11")); // IntegerIntervalBeforePointTrue
        Assert.False(await Helpers.CheckBool("Interval[1, 10] before 8")); // IntegerIntervalBeforePointFalse
        Assert.False(await Helpers.CheckBool("Interval[11.0, 20.0] before Interval[1.0, 10.0]")); // DecimalIntervalBeforeFalse
        Assert.True(await Helpers.CheckBool("Interval[1.0, 10.0] before Interval[11.0, 20.0]")); // DecimalIntervalBeforeTrue
        Assert.True(await Helpers.CheckBool("9.0 before Interval[11.0, 20.0]")); // DecimalIntervalPointBeforeTrue
        Assert.False(await Helpers.CheckBool("9.0 before Interval[1.0, 10.0]")); // DecimalIntervalPointBeforeFalse
        Assert.True(await Helpers.CheckBool("Interval[1.0, 10.0] before 11.0")); // DecimalIntervalBeforePointTrue
        Assert.False(await Helpers.CheckBool("Interval[1.0, 10.0] before 8.0")); // DecimalIntervalBeforePointFalse
        Assert.True(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] before Interval[11.0 'g', 20.0 'g']")); // QuantityIntervalBeforeTrue
        Assert.False(await Helpers.CheckBool("Interval[11.0 'g', 20.0 'g'] before Interval[1.0 'g', 10.0 'g']")); // QuantityIntervalBeforeFalse
        Assert.True(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] before 12.0'g'")); // QuantityIntervalPointBeforeTrue
        Assert.False(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] before 9.0'g'")); // QuantityIntervalPointBeforeFalse
        Assert.True(await Helpers.CheckBool("5.0'g' before Interval[11.0 'g', 20.0 'g']")); // QuantityIntervalBeforePointTrue
        Assert.False(await Helpers.CheckBool("12.0'g' before Interval[11.0 'g', 20.0 'g']")); // QuantityIntervalBeforePointFalse
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 1), DateTime(2012, 1, 15)] before DateTime(2012, 2, 27)")); // DateTimeBeforeTrue
        Assert.False(await Helpers.CheckBool("Interval[DateTime(2012, 1, 1), DateTime(2012, 1, 15)] before DateTime(2011, 12, 31)")); // DateTimeBeforeFalse
        Assert.True(await Helpers.CheckBool("Interval[@T15:59:59.999, @T20:59:59.999] before @T22:59:59.999")); // TimeBeforeTrue
        Assert.False(await Helpers.CheckBool("Interval[@T15:59:59.999, @T20:59:59.999] before @T10:59:59.999")); // TimeBeforeFalse
    }

    [Fact]
    public async Task Collapse()
    {
        Assert.True(await Helpers.CheckBool("collapse {Interval(null, null)} = { }")); // TestCollapseNull
        Assert.True(await Helpers.CheckBool("collapse { Interval[1,5], Interval[3,7], Interval[12,19], Interval[7,10] } = {Interval [ 1, 10 ], Interval [ 12, 19 ]}")); // IntegerIntervalCollapse
        Assert.True(await Helpers.CheckBool("collapse { Interval[1,2], Interval[3,7], Interval[10,19], Interval[7,10] } = {Interval [ 1, 19 ]}")); // IntegerIntervalCollapse2
        Assert.True(await Helpers.CheckBool("collapse { Interval[4,6], Interval[7,8] } = {Interval [ 4, 8 ]}")); // IntegerIntervalCollapse3
        Assert.True(await Helpers.CheckBool("collapse { Interval[1.0,5.0], Interval[3.0,7.0], Interval[12.0,19.0], Interval[7.0,10.0] } = {Interval [ 1.0, 10.0 ], Interval [ 12.0, 19.0 ]}")); // DecimalIntervalCollapse
        Assert.True(await Helpers.CheckBool("collapse { Interval[4.0,6.0], Interval[6.00000001,8.0] } = {Interval [ 4.0, 8.0 ]}")); // DecimalIntervalCollapse2
        Assert.True(await Helpers.CheckBool("collapse { Interval[1.0 'g',5.0 'g'], Interval[3.0 'g',7.0 'g'], Interval[12.0 'g',19.0 'g'], Interval[7.0 'g',10.0 'g'] } = {Interval [ 1.0 'g', 10.0 'g' ], Interval [ 12.0 'g', 19.0 'g' ]}")); // QuantityIntervalCollapse
        Assert.True(await Helpers.CheckBool("collapse { Interval[DateTime(2012, 1, 1), DateTime(2012, 1, 15)], Interval[DateTime(2012, 1, 10), DateTime(2012, 1, 25)], Interval[DateTime(2012, 5, 10), DateTime(2012, 5, 25)], Interval[DateTime(2012, 5, 20), DateTime(2012, 5, 30)] } = {Interval [ @2012-01-01T, @2012-01-25T ], Interval [ @2012-05-10T, @2012-05-30T ]}")); // DateTimeCollapse
        Assert.True(await Helpers.CheckBool("collapse { Interval[DateTime(2012, 1, 1), DateTime(2012, 1, 15)], Interval[DateTime(2012, 1, 16), DateTime(2012, 5, 25)] } = {Interval [ @2012-01-01T, @2012-05-25T ]}")); // DateTimeCollapse2
        Assert.True(await Helpers.CheckBool("collapse { Interval[@T01:59:59.999, @T10:59:59.999], Interval[@T08:59:59.999, @T15:59:59.999], Interval[@T17:59:59.999, @T20:59:59.999], Interval[@T18:59:59.999, @T22:59:59.999] } = {Interval [ @T01:59:59.999, @T15:59:59.999 ], Interval [ @T17:59:59.999, @T22:59:59.999 ]}")); // TimeCollapse
        Assert.True(await Helpers.CheckBool("collapse { Interval[@T01:59:59.999, @T10:59:59.999], Interval[@T11:00:00.000, @T15:59:59.999] } = {Interval [ @T01:59:59.999, @T15:59:59.999 ]}")); // TimeCollapse2
    }

    [Fact]
    public async Task Expand()
    {
        Assert.True(await Helpers.CheckBool("(expand null) is null")); // ExpandNull
        Assert.True(await Helpers.CheckBool("expand { } = { }")); // ExpandEmptyList
        Assert.True(await Helpers.CheckBool("expand { null } = { }")); // ExpandListWithNull
        Assert.True(await Helpers.CheckBool("expand { Interval[@2018-01-01, @2018-01-04] } per day = { Interval[@2018-01-01, @2018-01-01], Interval[@2018-01-02, @2018-01-02], Interval[@2018-01-03, @2018-01-03], Interval[@2018-01-04, @2018-01-04] }")); // ExpandPerDay
        Assert.True(await Helpers.CheckBool("expand Interval[@2018-01-01, @2018-01-04] per day = { @2018-01-01, @2018-01-02, @2018-01-03, @2018-01-04 }")); // ExpandPerDayIntervalOverload
        Assert.True(await Helpers.CheckBool("expand { Interval[@2018-01-01, @2018-01-04] } per 2 days = { Interval[@2018-01-01, @2018-01-02], Interval[@2018-01-03, @2018-01-04] }")); // ExpandPer2Days
        Assert.True(await Helpers.CheckBool("expand Interval[@2018-01-01, @2018-01-04] per 2 days = { @2018-01-01, @2018-01-03 }")); // ExpandPer2DaysIntervalOverload
        Assert.True(await Helpers.CheckBool("expand { Interval[@T10:00, @T12:30] } per hour = { Interval[@T10, @T10], Interval[@T11, @T11], Interval[@T12, @T12] }")); // ExpandPerHour
        Assert.True(await Helpers.CheckBool("expand Interval[@T10:00, @T12:30] per hour = { @T10, @T11, @T12 }")); // ExpandPerHourIntervalOverload
        Assert.True(await Helpers.CheckBool("expand { Interval[@T10:00, @T12:30) } per hour = { Interval[@T10, @T10], Interval[@T11, @T11], Interval[@T12, @T12] }")); // ExpandPerHourOpen
        Assert.True(await Helpers.CheckBool("expand Interval[@T10:00, @T12:30) per hour = { @T10, @T11, @T12 }")); // ExpandPerHourOpenIntervalOverload
        Assert.True(await Helpers.CheckBool("expand { Interval[10.0, 12.5] } per 1 = { Interval[10, 10], Interval[11, 11], Interval[12, 12] }")); // ExpandPer1
        Assert.True(await Helpers.CheckBool("expand Interval[10.0, 12.5] per 1 = { 10, 11, 12 }")); // ExpandPer1IntervalOverload
        Assert.True(await Helpers.CheckBool("expand { Interval[10.0, 12.5) } per 1 = { Interval[10, 10], Interval[11, 11], Interval[12, 12] }")); // ExpandPer1Open
        Assert.True(await Helpers.CheckBool("expand Interval[10.0, 12.5) per 1 = { 10, 11, 12 }")); // ExpandPer1OpenIntervalOverload
        Assert.True(await Helpers.CheckBool("expand { Interval[@T10, @T10] } per minute = { }")); // ExpandPerMinute
        Assert.True(await Helpers.CheckBool("expand Interval[@T10, @T10] per minute = { }")); // ExpandPerMinuteIntervalOverload
        Assert.True(await Helpers.CheckBool("expand { Interval[10, 10] } per 0.1 = { Interval[10.0, 10.0], Interval[10.1, 10.1], Interval[10.2, 10.2], Interval[10.3, 10.3], Interval[10.4, 10.4], Interval[10.5, 10.5], Interval[10.6, 10.6], Interval[10.7, 10.7], Interval[10.8, 10.8], Interval[10.9, 10.9] }")); // ExpandPer0D1
        Assert.True(await Helpers.CheckBool("expand Interval[10, 10] per 0.1 = { 10.0, 10.1, 10.2, 10.3, 10.4, 10.5, 10.6, 10.7, 10.8, 10.9 }")); // ExpandPer0D1IntervalOverload
        Assert.True(await Helpers.CheckBool("expand { Interval[1, 10] } = { Interval[1, 1], Interval[2, 2], Interval[3, 3], Interval[4, 4], Interval[5, 5], Interval[6, 6], Interval[7, 7], Interval[8, 8], Interval[9, 9], Interval[10, 10] }")); // ExpandInterval
        Assert.True(await Helpers.CheckBool("expand Interval[1, 10] = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }")); // ExpandIntegerIntervalOverload
        Assert.True(await Helpers.CheckBool("expand { Interval[1, 10) } = { Interval[1, 1], Interval[2, 2], Interval[3, 3], Interval[4, 4], Interval[5, 5], Interval[6, 6], Interval[7, 7], Interval[8, 8], Interval[9, 9] }")); // ExpandIntervalOpen
        Assert.True(await Helpers.CheckBool("expand Interval[1, 10) = { 1, 2, 3, 4, 5, 6, 7, 8, 9 }")); // ExpandIntegerOpenIntervalOverload
        Assert.True(await Helpers.CheckBool("expand { Interval[1, 10] } per 2 = { Interval[1, 2], Interval[3, 4], Interval[5, 6], Interval[7, 8], Interval[9, 10] }")); // ExpandIntervalPer2
        Assert.True(await Helpers.CheckBool("expand Interval[1, 10] per 2 = { 1, 3, 5, 7, 9 }")); // ExpandIntervalPer2IntervalOverload
        Assert.True(await Helpers.CheckBool("expand { Interval[1, 10) } per 2 = { Interval[1, 2], Interval[3, 4], Interval[5, 6], Interval[7, 8] }")); // ExpandIntervalOpenPer2
        Assert.True(await Helpers.CheckBool("expand Interval[1, 10) per 2 = { 1, 3, 5, 7 }")); // ExpandIntervalOpenPer2IntervalOverload
    }

    [Fact]
    public async Task Contains()
    {
        Assert.True(await Helpers.CheckBool("(Interval[1, 10] contains null) is null")); // TestContainsNull
        Assert.False(await Helpers.CheckBool("null contains 5")); // TestNullElement1
        Assert.False(await Helpers.CheckBool("Interval[null, 5] contains 10")); // TestNullElement2
        Assert.True(await Helpers.CheckBool("Interval[1, 10] contains 5")); // IntegerIntervalContainsTrue
        Assert.False(await Helpers.CheckBool("Interval[1, 10] contains 25")); // IntegerIntervalContainsFalse
        Assert.True(await Helpers.CheckBool("Interval[1.0, 10.0] contains 8.0")); // DecimalIntervalContainsTrue
        Assert.False(await Helpers.CheckBool("Interval[1.0, 10.0] contains 255.0")); // DecimalIntervalContainsFalse
        Assert.True(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] contains 2.0 'g'")); // QuantityIntervalContainsTrue
        Assert.False(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] contains 100.0 'g'")); // QuantityIntervalContainsFalse
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 1), DateTime(2012, 1, 15)] contains DateTime(2012, 1, 10)")); // DateTimeContainsTrue
        Assert.False(await Helpers.CheckBool("Interval[DateTime(2012, 1, 1), DateTime(2012, 1, 15)] contains DateTime(2012, 1, 16)")); // DateTimeContainsFalse
        Assert.True(await Helpers.CheckBool("Interval[@T01:59:59.999, @T10:59:59.999] contains @T05:59:59.999")); // TimeContainsTrue
        Assert.False(await Helpers.CheckBool("Interval[@T01:59:59.999, @T10:59:59.999] contains @T15:59:59.999")); // TimeContainsFalse
    }

    [Fact]
    public async Task End()
    {
        Assert.True(await Helpers.CheckBool("end of Interval[1, 10] = 10")); // IntegerIntervalEnd
        Assert.True(await Helpers.CheckBool("end of Interval[1.0, 10.0] = 10.0")); // DecimalIntervalEnd
        Assert.True(await Helpers.CheckBool("end of Interval[1.0 'g', 10.0 'g'] = 10.0'g'")); // QuantityIntervalEnd
        Assert.True(await Helpers.CheckBool("end of Interval[@2016-05-01T00:00:00.000, @2016-05-02T00:00:00.000] = @2016-05-02T00:00:00.000")); // DateTimeIntervalEnd
        Assert.True(await Helpers.CheckBool("end of Interval[@T00:00:00.000, @T23:59:59.599] = @T23:59:59.599")); // TimeIntervalEnd
    }

    [Fact]
    public async Task Ends()
    {
        Assert.True(await Helpers.CheckBool("(Interval[1, 10] ends Interval(null, null)) is null")); // TestEndsNull
        Assert.True(await Helpers.CheckBool("Interval[4, 10] ends Interval[1, 10]")); // IntegerIntervalEndsTrue
        Assert.False(await Helpers.CheckBool("Interval[44, 50] ends Interval[1, 10]")); // IntegerIntervalEndsFalse
        Assert.True(await Helpers.CheckBool("Interval[4.0, 10.0] ends Interval[1.0, 10.0]")); // DecimalIntervalEndsTrue
        Assert.False(await Helpers.CheckBool("Interval[11.0, 20.0] ends Interval[1.0, 10.0]")); // DecimalIntervalEndsFalse
        Assert.True(await Helpers.CheckBool("Interval[5.0 'g', 10.0 'g'] ends Interval[1.0 'g', 10.0 'g']")); // QuantityIntervalEndsTrue
        Assert.False(await Helpers.CheckBool("Interval[11.0 'g', 20.0 'g'] ends Interval[1.0 'g', 10.0 'g']")); // QuantityIntervalEndsFalse
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 15)] ends Interval[DateTime(2012, 1, 1), DateTime(2012, 1, 15)]")); // DateTimeEndsTrue
        Assert.False(await Helpers.CheckBool("Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 15)] ends Interval[DateTime(2012, 1, 1), DateTime(2012, 1, 16)]")); // DateTimeEndsFalse
        Assert.True(await Helpers.CheckBool("Interval[@T05:59:59.999, @T10:59:59.999] ends Interval[@T01:59:59.999, @T10:59:59.999]")); // TimeEndsTrue
        Assert.False(await Helpers.CheckBool("Interval[@T05:59:59.999, @T10:59:59.999] ends Interval[@T01:59:59.999, @T11:59:59.999]")); // TimeEndsFalse
    }

    [Fact]
    public async Task Equal()
    {
        Assert.True(await Helpers.CheckBool("(Interval[1, 10] = Interval(null, null)) is null")); // TestEqualNull
        Assert.True(await Helpers.CheckBool("Interval[1, 10] = Interval[1, 10]")); // IntegerIntervalEqualTrue
        Assert.False(await Helpers.CheckBool("Interval[1, 10] = Interval[11, 20]")); // IntegerIntervalEqualFalse
        Assert.True(await Helpers.CheckBool("Interval[1.0, 10.0] = Interval[1.0, 10.0]")); // DecimalIntervalEqualTrue
        Assert.False(await Helpers.CheckBool("Interval[1.0, 10.0] = Interval[11.0, 20.0]")); // DecimalIntervalEqualFalse
        Assert.True(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] = Interval[1.0 'g', 10.0 'g']")); // QuantityIntervalEqualTrue
        Assert.False(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] = Interval[11.0 'g', 20.0 'g']")); // QuantityIntervalEqualFalse
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 5, 0, 0, 0, 0), DateTime(2012, 1, 15, 0, 0, 0, 0)] = Interval[DateTime(2012, 1, 5, 0, 0, 0, 0), DateTime(2012, 1, 15, 0, 0, 0, 0)]")); // DateTimeEqualTrue
        Assert.False(await Helpers.CheckBool("Interval[DateTime(2012, 1, 5, 0, 0, 0, 0), DateTime(2012, 1, 15, 0, 0, 0, 0)] = Interval[DateTime(2012, 1, 5, 0, 0, 0, 0), DateTime(2012, 1, 16, 0, 0, 0, 0)]")); // DateTimeEqualFalse
        Assert.True(await Helpers.CheckBool("Interval[@T05:59:59.999, @T10:59:59.999] = Interval[@T05:59:59.999, @T10:59:59.999]")); // TimeEqualTrue
        Assert.False(await Helpers.CheckBool("Interval[@T05:59:59.999, @T10:59:59.999] = Interval[@T05:59:59.999, @T10:58:59.999]")); // TimeEqualFalse
    }

    [Fact]
    public async Task Except()
    {
        Assert.True(await Helpers.CheckBool("(Interval[null, null]) is null")); // NullInterval
        Assert.True(await Helpers.CheckBool("(Interval[null, null] except Interval[null, null]) is null")); // TestExceptNull
        Assert.True(await Helpers.CheckBool("Interval[1, 10] except Interval[4, 10] = Interval [ 1, 3 ]")); // IntegerIntervalExcept1to3
        Assert.True(await Helpers.CheckBool("(Interval[1, 10] except Interval[3, 7]) is null")); // IntegerIntervalExceptNull
        Assert.True(await Helpers.CheckBool("Interval[1.0, 10.0] except Interval[4.0, 10.0] = Interval [ 1.0, 3.99999999 ]")); // DecimalIntervalExcept1to3
        Assert.True(await Helpers.CheckBool("(Interval[1.0, 10.0] except Interval[3.0, 7.0]) is null")); // DecimalIntervalExceptNull
        Assert.True(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] except Interval[5.0 'g', 10.0 'g'] = Interval [ 1.0 'g', 4.99999999 'g' ]")); // QuantityIntervalExcept1to4
        Assert.True(await Helpers.CheckBool("Interval[1, 4] except Interval[3, 6] = Interval [ 1, 2 ]")); // Except12
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 15)] except Interval[DateTime(2012, 1, 7), DateTime(2012, 1, 15)] = Interval [ @2012-01-05T, @2012-01-06T ]")); // ExceptDateTimeInterval
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 7), DateTime(2012, 1, 16)] except Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 12)] = Interval [ @2012-01-13T, @2012-01-16T ]")); // ExceptDateTime2
        Assert.True(await Helpers.CheckBool("Interval[@T05:59:59.999, @T10:59:59.999] except Interval[@T08:59:59.999, @T10:59:59.999] = Interval [ @T05:59:59.999, @T08:59:59.998 ]")); // ExceptTimeInterval
        Assert.True(await Helpers.CheckBool("Interval[@T08:59:59.999, @T11:59:59.999] except Interval[@T05:59:59.999, @T10:59:59.999] = Interval [ @T11:00:00.000, @T11:59:59.999 ]")); // ExceptTime2
    }

    [Fact]
    public async Task In()
    {
        Assert.False(await Helpers.CheckBool("5 in Interval[null, null]")); // TestInNullBoundaries
        Assert.True(await Helpers.CheckBool("5 in Interval[1, 10]")); // IntegerIntervalInTrue
        Assert.False(await Helpers.CheckBool("500 in Interval[1, 10]")); // IntegerIntervalInFalse
        Assert.True(await Helpers.CheckBool("9.0 in Interval[1.0, 10.0]")); // DecimalIntervalInTrue
        Assert.False(await Helpers.CheckBool("-2.0 in Interval[1.0, 10.0]")); // DecimalIntervalInFalse
        Assert.True(await Helpers.CheckBool("1.0 'g' in Interval[1.0 'g', 10.0 'g']")); // QuantityIntervalInTrue
        Assert.False(await Helpers.CheckBool("55.0 'g' in Interval[1.0 'g', 10.0 'g']")); // QuantityIntervalInFalse
        Assert.True(await Helpers.CheckBool("DateTime(2012, 1, 7) in Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 15)]")); // DateTimeInTrue
        Assert.False(await Helpers.CheckBool("DateTime(2012, 1, 17) in Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 15)]")); // DateTimeInFalse
        Assert.True(await Helpers.CheckBool("DateTime(2012, 1, 7) in Interval[DateTime(2012, 1, 5), null]")); // DateTimeInNullTrue
        Assert.True(await Helpers.CheckBool("@T07:59:59.999 in Interval[@T05:59:59.999, @T10:59:59.999]")); // TimeInTrue
        Assert.False(await Helpers.CheckBool("@T17:59:59.999 in Interval[@T05:59:59.999, @T10:59:59.999]")); // TimeInFalse
        Assert.True(await Helpers.CheckBool("(null in Interval[@T05:59:59.999, @T10:59:59.999]) is null")); // TimeInNull
        Assert.True(await Helpers.CheckBool("Interval[@2017-12-20T11:00:00, @2017-12-21T21:00:00] = Interval [ @2017-12-20T11:00:00, @2017-12-21T21:00:00 ]")); // TestPeriod1
        Assert.True(await Helpers.CheckBool("Interval[@2017-12-20T10:30:00, @2017-12-20T12:00:00] = Interval [ @2017-12-20T10:30:00, @2017-12-20T12:00:00 ]")); // TestPeriod2
        Assert.True(await Helpers.CheckBool("  Interval[@2017-12-20T10:30:00, @2017-12-20T12:00:00]  starts 1 day or less on or after day of start of  Interval[@2017-12-20T11:00:00, @2017-12-21T21:00:00]  ")); // Issue32Interval
    }

    [Fact]
    public async Task Includes()
    {
        Assert.True(await Helpers.CheckBool("(Interval[1, 10] includes null) is null")); // TestIncludesNull
        Assert.True(await Helpers.CheckBool("Interval[1, 10] includes Interval[4, 10]")); // IntegerIntervalIncludesTrue
        Assert.False(await Helpers.CheckBool("Interval[1, 10] includes Interval[44, 50]")); // IntegerIntervalIncludesFalse
        Assert.True(await Helpers.CheckBool("Interval[1.0, 10.0] includes Interval[4.0, 10.0]")); // DecimalIntervalIncludesTrue
        Assert.False(await Helpers.CheckBool("Interval[1.0, 10.0] includes Interval[11.0, 20.0]")); // DecimalIntervalIncludesFalse
        Assert.True(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] includes Interval[5.0 'g', 10.0 'g']")); // QuantityIntervalIncludesTrue
        Assert.False(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] includes Interval[11.0 'g', 20.0 'g']")); // QuantityIntervalIncludesFalse
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 15)] includes Interval[DateTime(2012, 1, 7), DateTime(2012, 1, 14)]")); // DateTimeIncludesTrue
        Assert.False(await Helpers.CheckBool("Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 15)] includes Interval[DateTime(2012, 1, 4), DateTime(2012, 1, 14)]")); // DateTimeIncludesFalse
        Assert.True(await Helpers.CheckBool("Interval[@T05:59:59.999, @T10:59:59.999] includes Interval[@T06:59:59.999, @T09:59:59.999]")); // TimeIncludesTrue
        Assert.False(await Helpers.CheckBool("Interval[@T05:59:59.999, @T10:59:59.999] includes Interval[@T04:59:59.999, @T09:59:59.999]")); // TimeIncludesFalse
    }

    [Fact]
    public async Task IncludedIn()
    {
        Assert.True(await Helpers.CheckBool("(null included in Interval[1, 10]) is null")); // TestIncludedInNull
        Assert.True(await Helpers.CheckBool("Interval[4, 10] included in Interval[1, 10]")); // IntegerIntervalIncludedInTrue
        Assert.False(await Helpers.CheckBool("Interval[44, 50] included in Interval[1, 10]")); // IntegerIntervalIncludedInFalse
        Assert.True(await Helpers.CheckBool("Interval[4.0, 10.0] included in Interval[1.0, 10.0]")); // DecimalIntervalIncludedInTrue
        Assert.False(await Helpers.CheckBool("Interval[11.0, 20.0] included in Interval[1.0, 10.0]")); // DecimalIntervalIncludedInFalse
        Assert.True(await Helpers.CheckBool("Interval[5.0 'g', 10.0 'g'] included in Interval[1.0 'g', 10.0 'g']")); // QuantityIntervalIncludedInTrue
        Assert.False(await Helpers.CheckBool("Interval[11.0 'g', 20.0 'g'] included in Interval[1.0 'g', 10.0 'g']")); // QuantityIntervalIncludedInFalse
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 7), DateTime(2012, 1, 14)] included in Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 15)]")); // DateTimeIncludedInTrue
        Assert.False(await Helpers.CheckBool("Interval[DateTime(2012, 1, 4), DateTime(2012, 1, 14)] included in Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 15)]")); // DateTimeIncludedInFalse
        Assert.True(await Helpers.CheckBool("Interval[@T06:59:59.999, @T09:59:59.999] included in Interval[@T05:59:59.999, @T10:59:59.999]")); // TimeIncludedInTrue
        Assert.False(await Helpers.CheckBool("Interval[@T04:59:59.999, @T09:59:59.999] included in Interval[@T05:59:59.999, @T10:59:59.999]")); // TimeIncludedInFalse
        Assert.True(await Helpers.CheckBool("(Interval [@2017-09-01T00:00:00, @2017-09-01T00:00:00] included in Interval [@2017-09-01T00:00:00.000, @2017-12-30T23:59:59.999]) is null")); // DateTimeIncludedInNull
        Assert.True(await Helpers.CheckBool("Interval [@2017-09-01T00:00:00, @2017-09-01T00:00:00] included in day of Interval [@2017-09-01T00:00:00.000, @2017-12-30T23:59:59.999]")); // DateTimeIncludedInPrecisionTrue
        Assert.True(await Helpers.CheckBool("(Interval [@2017-09-01T00:00:00, @2017-09-01T00:00:00] included in millisecond of Interval [@2017-09-01T00:00:00.000, @2017-12-30T23:59:59.999]) is null")); // DateTimeIncludedInPrecisionNull
    }

    [Fact]
    public async Task Intersect()
    {
        Assert.True(await Helpers.CheckBool("Interval[1, 10] intersect Interval[5, null) = Interval[5, null)")); // TestIntersectNull
        Assert.True(await Helpers.CheckBool("start of (Interval[1, 10] intersect Interval[5, null)) <= 10")); // TestIntersectNull1
        Assert.True(await Helpers.CheckBool("start of (Interval[1, 10] intersect Interval[5, null)) >= 5")); // TestIntersectNull2
        Assert.False(await Helpers.CheckBool("start of (Interval[1, 10] intersect Interval[5, null)) > 10")); // TestIntersectNull3
        Assert.False(await Helpers.CheckBool("start of (Interval[1, 10] intersect Interval[5, null)) < 5")); // TestIntersectNull4
        Assert.True(await Helpers.CheckBool("Interval[1, 10] intersect Interval[4, 10] = Interval [ 4, 10 ]")); // IntegerIntervalIntersectTest4to10
        Assert.True(await Helpers.CheckBool("(Interval[1, 10] intersect Interval[11, 20]) is null")); // IntegerIntervalIntersectTestNull
        Assert.True(await Helpers.CheckBool("Interval[1.0, 10.0] intersect Interval[4.0, 10.0] = Interval [ 4.0, 10.0 ]")); // DecimalIntervalIntersectTest4to10
        Assert.True(await Helpers.CheckBool("(Interval[1.0, 10.0] intersect Interval[11.0, 20.0]) is null")); // DecimalIntervalIntersectTestNull
        Assert.True(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] intersect Interval[5.0 'g', 10.0 'g'] = Interval [ 5.0 'g', 10.0 'g' ]")); // QuantityIntervalIntersectTest5to10
        Assert.True(await Helpers.CheckBool("(Interval[1.0 'g', 10.0 'g'] intersect Interval[11.0 'g', 20.0 'g']) is null")); // QuantityIntervalIntersectTestNull
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 7), DateTime(2012, 1, 14)] intersect Interval[DateTime(2012, 1, 7), DateTime(2012, 1, 10)] = Interval [ @2012-01-07T, @2012-01-10T ]")); // DateTimeIntersect
        Assert.True(await Helpers.CheckBool("Interval[@T04:59:59.999, @T09:59:59.999] intersect Interval[@T04:59:59.999, @T06:59:59.999] = Interval [ @T04:59:59.999, @T06:59:59.999 ]")); // TimeIntersect
    }

    [Fact]
    public async Task Equivalent()
    {
        Assert.True(await Helpers.CheckBool("Interval[1, 10] ~ Interval[1, 10]")); // IntegerIntervalEquivalentTrue
        Assert.False(await Helpers.CheckBool("Interval[44, 50] ~ Interval[1, 10]")); // IntegerIntervalEquivalentFalse
        Assert.True(await Helpers.CheckBool("Interval[1.0, 10.0] ~ Interval[1.0, 10.0]")); // DecimalIntervalEquivalentTrue
        Assert.False(await Helpers.CheckBool("Interval[11.0, 20.0] ~ Interval[1.0, 10.0]")); // DecimalIntervalEquivalentFalse
        Assert.True(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] ~ Interval[1.0 'g', 10.0 'g']")); // QuantityIntervalEquivalentTrue
        Assert.False(await Helpers.CheckBool("Interval[11.0 'g', 20.0 'g'] ~ Interval[1.0 'g', 10.0 'g']")); // QuantityIntervalEquivalentFalse
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 7), DateTime(2012, 1, 14)] ~ Interval[DateTime(2012, 1, 7), DateTime(2012, 1, 14)]")); // DateTimeEquivalentTrue
        Assert.False(await Helpers.CheckBool("Interval[DateTime(2012, 1, 7), DateTime(2012, 1, 14)] ~ Interval[DateTime(2012, 1, 7), DateTime(2012, 1, 15)]")); // DateTimeEquivalentFalse
        Assert.True(await Helpers.CheckBool("Interval[@T04:59:59.999, @T09:59:59.999] ~ Interval[@T04:59:59.999, @T09:59:59.999]")); // TimeEquivalentTrue
        Assert.False(await Helpers.CheckBool("Interval[@T04:59:59.999, @T09:59:59.999] ~ Interval[@T04:58:59.999, @T09:59:59.999]")); // TimeEquivalentFalse
    }

    [Fact]
    public async Task Meets()
    {
        Assert.True(await Helpers.CheckBool("(Interval(null, 5] meets Interval(null, 15)) is null")); // TestMeetsNull
        Assert.True(await Helpers.CheckBool("Interval[1, 10] meets Interval[11, 20]")); // IntegerIntervalMeetsTrue
        Assert.False(await Helpers.CheckBool("Interval[1, 10] meets Interval[44, 50]")); // IntegerIntervalMeetsFalse
        Assert.True(await Helpers.CheckBool("Interval[3.01, 5.00000001] meets Interval[5.00000002, 8.50]")); // DecimalIntervalMeetsTrue
        Assert.False(await Helpers.CheckBool("Interval[3.01, 5.00000001] meets Interval[5.5, 8.50]")); // DecimalIntervalMeetsFalse
        Assert.True(await Helpers.CheckBool("Interval[3.01 'g', 5.00000001 'g'] meets Interval[5.00000002 'g', 8.50 'g']")); // QuantityIntervalMeetsTrue
        Assert.False(await Helpers.CheckBool("Interval[3.01 'g', 5.00000001 'g'] meets Interval[5.5 'g', 8.50 'g']")); // QuantityIntervalMeetsFalse
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 7), DateTime(2012, 1, 14)] meets Interval[DateTime(2012, 1, 15), DateTime(2012, 1, 25)]")); // DateTimeMeetsTrue
        Assert.False(await Helpers.CheckBool("Interval[DateTime(2012, 1, 7), DateTime(2012, 1, 14)] meets Interval[DateTime(2012, 1, 20), DateTime(2012, 1, 25)]")); // DateTimeMeetsFalse
        Assert.True(await Helpers.CheckBool("Interval[@T04:59:59.999, @T09:59:59.999] meets Interval[@T10:00:00.000, @T19:59:59.999]")); // TimeMeetsTrue
        Assert.False(await Helpers.CheckBool("Interval[@T04:59:59.999, @T09:59:59.999] meets Interval[@T10:12:00.000, @T19:59:59.999]")); // TimeMeetsFalse
    }

    [Fact]
    public async Task MeetsBefore()
    {
        Assert.True(await Helpers.CheckBool("(Interval(null, 5] meets before Interval(null, 25]) is null")); // TestMeetsBeforeNull
        Assert.True(await Helpers.CheckBool("Interval[1, 10] meets before Interval[11, 20]")); // IntegerIntervalMeetsBeforeTrue
        Assert.False(await Helpers.CheckBool("Interval[1, 10] meets before Interval[44, 50]")); // IntegerIntervalMeetsBeforeFalse
        Assert.True(await Helpers.CheckBool("Interval[3.50000001, 5.00000011] meets before Interval[5.00000012, 8.50]")); // DecimalIntervalMeetsBeforeTrue
        Assert.False(await Helpers.CheckBool("Interval[8.01, 15.00000001] meets before Interval[15.00000000, 18.50]")); // DecimalIntervalMeetsBeforeFalse
        Assert.True(await Helpers.CheckBool("Interval[3.50000001 'g', 5.00000011 'g'] meets before Interval[5.00000012 'g', 8.50 'g']")); // QuantityIntervalMeetsBeforeTrue
        Assert.False(await Helpers.CheckBool("Interval[8.01 'g', 15.00000001 'g'] meets before Interval[15.00000000 'g', 18.50 'g']")); // QuantityIntervalMeetsBeforeFalse
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 7), DateTime(2012, 1, 14)] meets Interval[DateTime(2012, 1, 15), DateTime(2012, 1, 25)]")); // DateTimeMeetsBeforeTrue
        Assert.False(await Helpers.CheckBool("Interval[DateTime(2012, 1, 7), DateTime(2012, 1, 14)] meets Interval[DateTime(2012, 1, 20), DateTime(2012, 1, 25)]")); // DateTimeMeetsBeforeFalse
        Assert.True(await Helpers.CheckBool("Interval[@T04:59:59.999, @T09:59:59.999] meets Interval[@T10:00:00.000, @T19:59:59.999]")); // TimeMeetsBeforeTrue
        Assert.False(await Helpers.CheckBool("Interval[@T04:59:59.999, @T09:59:59.999] meets Interval[@T10:12:00.000, @T19:59:59.999]")); // TimeMeetsBeforeFalse
    }

    [Fact]
    public async Task MeetsAfter()
    {
        Assert.False(await Helpers.CheckBool("Interval(null, 5] meets after Interval[11, null)")); // TestMeetsAfterNull
        Assert.True(await Helpers.CheckBool("Interval[11, 20] meets after Interval[1, 10]")); // IntegerIntervalMeetsAfterTrue
        Assert.False(await Helpers.CheckBool("Interval[44, 50] meets after Interval[1, 10]")); // IntegerIntervalMeetsAfterFalse
        Assert.True(await Helpers.CheckBool("Interval[55.00000123, 128.032156] meets after Interval[12.00258, 55.00000122]")); // DecimalIntervalMeetsAfterTrue
        Assert.False(await Helpers.CheckBool("Interval[55.00000124, 150.222222] meets after Interval[12.00258, 55.00000122]")); // DecimalIntervalMeetsAfterFalse
        Assert.True(await Helpers.CheckBool("Interval[55.00000123 'g', 128.032156 'g'] meets after Interval[12.00258 'g', 55.00000122 'g']")); // QuantityIntervalMeetsAfterTrue
        Assert.False(await Helpers.CheckBool("Interval[55.00000124 'g', 150.222222 'g'] meets after Interval[12.00258 'g', 55.00000122 'g']")); // QuantityIntervalMeetsAfterFalse
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 15), DateTime(2012, 1, 25)] meets Interval[DateTime(2012, 1, 7), DateTime(2012, 1, 14)]")); // DateTimeMeetsAfterTrue
        Assert.False(await Helpers.CheckBool("Interval[DateTime(2012, 1, 20), DateTime(2012, 1, 25)] meets Interval[DateTime(2012, 1, 7), DateTime(2012, 1, 14)]")); // DateTimeMeetsAfterFalse
        Assert.True(await Helpers.CheckBool("Interval[@T10:00:00.000, @T19:59:59.999] meets Interval[@T04:59:59.999, @T09:59:59.999]")); // TimeMeetsAfterTrue
        Assert.False(await Helpers.CheckBool("Interval[@T10:12:00.000, @T19:59:59.999] meets Interval[@T04:59:59.999, @T09:59:59.999]")); // TimeMeetsAfterFalse
    }

    [Fact]
    public async Task NotEqual()
    {
        Assert.True(await Helpers.CheckBool("Interval[1, 10] != Interval[11, 20]")); // IntegerIntervalNotEqualTrue
        Assert.False(await Helpers.CheckBool("Interval[1, 10] != Interval[1, 10]")); // IntegerIntervalNotEqualFalse
        Assert.True(await Helpers.CheckBool("Interval[1.0, 10.0] != Interval[11.0, 20.0]")); // DecimalIntervalNotEqualTrue
        Assert.False(await Helpers.CheckBool("Interval[1.0, 10.0] != Interval[1.0, 10.0]")); // DecimalIntervalNotEqualFalse
        Assert.True(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] != Interval[11.0 'g', 20.0 'g']")); // QuantityIntervalNotEqualTrue
        Assert.False(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] != Interval[1.0 'g', 10.0 'g']")); // QuantityIntervalNotEqualFalse
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 15, 0, 0, 0, 0), DateTime(2012, 1, 25, 0, 0, 0, 0)] != Interval[DateTime(2012, 1, 15, 0, 0, 0, 0), DateTime(2012, 1, 25, 0, 0, 0, 22)]")); // DateTimeNotEqualTrue
        Assert.False(await Helpers.CheckBool("Interval[DateTime(2012, 1, 15, 0, 0, 0, 0), DateTime(2012, 1, 25, 0, 0, 0, 0)] != Interval[DateTime(2012, 1, 15, 0, 0, 0, 0), DateTime(2012, 1, 25, 0, 0, 0, 0)]")); // DateTimeNotEqualFalse
        Assert.True(await Helpers.CheckBool("Interval[@T10:00:00.000, @T19:59:59.999] != Interval[@T10:10:00.000, @T19:59:59.999]")); // TimeNotEqualTrue
        Assert.False(await Helpers.CheckBool("Interval[@T10:00:00.000, @T19:59:59.999] != Interval[@T10:00:00.000, @T19:59:59.999]")); // TimeNotEqualFalse
    }

    [Fact]
    public async Task OnOrAfter()
    {
        Assert.True(await Helpers.CheckBool("(Interval[@2012-12-01, @2013-12-01] on or after (null as Interval<Date>)) is null")); // TestOnOrAfterNull
        Assert.True(await Helpers.CheckBool("Interval[@2012-12-01, @2013-12-01] on or after month of @2012-11-15")); // TestOnOrAfterDateTrue
        Assert.False(await Helpers.CheckBool("@2012-11-15 on or after month of Interval[@2012-12-01, @2013-12-01]")); // TestOnOrAfterDateFalse
        Assert.True(await Helpers.CheckBool("Interval[@T10:00:00.000, @T19:59:59.999] on or after hour of Interval[@T08:00:00.000, @T09:59:59.999]")); // TestOnOrAfterTimeTrue
        Assert.False(await Helpers.CheckBool("Interval[@T10:00:00.000, @T19:59:59.999] on or after hour of Interval[@T08:00:00.000, @T11:59:59.999]")); // TestOnOrAfterTimeFalse
        Assert.True(await Helpers.CheckBool("Interval[6, 10] on or after 6")); // TestOnOrAfterIntegerTrue
        Assert.False(await Helpers.CheckBool("2.5 on or after Interval[1.666, 2.50000001]")); // TestOnOrAfterDecimalFalse
        Assert.True(await Helpers.CheckBool("2.5 'mg' on or after Interval[1.666 'mg', 2.50000000 'mg']")); // TestOnOrAfterQuantityTrue
    }

    [Fact]
    public async Task OnOrBefore()
    {
        Assert.True(await Helpers.CheckBool("(Interval[@2012-12-01, @2013-12-01] on or before (null as Interval<Date>)) is null")); // TestOnOrBeforeNull
        Assert.True(await Helpers.CheckBool("Interval[@2012-10-01, @2012-11-01] on or before month of @2012-11-15")); // TestOnOrBeforeDateTrue
        Assert.False(await Helpers.CheckBool("@2012-11-15 on or before month of Interval[@2012-10-01, @2013-12-01]")); // TestOnOrBeforeDateFalse
        Assert.True(await Helpers.CheckBool("Interval[@T05:00:00.000, @T07:59:59.999] on or before hour of Interval[@T08:00:00.000, @T09:59:59.999]")); // TestOnOrBeforeTimeTrue
        Assert.False(await Helpers.CheckBool("Interval[@T10:00:00.000, @T19:59:59.999] on or before hour of Interval[@T08:00:00.000, @T11:59:59.999]")); // TestOnOrBeforeTimeFalse
        Assert.True(await Helpers.CheckBool("Interval[4, 6] on or before 6")); // TestOnOrBeforeIntegerTrue
        Assert.False(await Helpers.CheckBool("1.6667 on or before Interval[1.666, 2.50000001]")); // TestOnOrBeforeDecimalFalse
        Assert.True(await Helpers.CheckBool("1.666 'mg' on or before Interval[1.666 'mg', 2.50000000 'mg']")); // TestOnOrBeforeQuantityTrue
    }

    [Fact]
    public async Task Overlaps()
    {
        Assert.True(await Helpers.CheckBool("(Interval[null, null] overlaps Interval[1, 10]) is null")); // TestOverlapsNull
        Assert.True(await Helpers.CheckBool("Interval[1, 10] overlaps Interval[4, 10]")); // IntegerIntervalOverlapsTrue
        Assert.True(await Helpers.CheckBool("Interval[4, 10] overlaps Interval[4, 10]")); // IntegerIntervalOverlapsTrue2
        Assert.True(await Helpers.CheckBool("Interval[10, 15] overlaps Interval[4, 10]")); // IntegerIntervalOverlapsTrue3
        Assert.False(await Helpers.CheckBool("Interval[1, 10] overlaps Interval[11, 20]")); // IntegerIntervalOverlapsFalse
        Assert.True(await Helpers.CheckBool("Interval[4, 10) overlaps Interval[4, 10)")); // IntegerIntervalExclusiveOverlapsTrue
        Assert.True(await Helpers.CheckBool("Interval[4, 11) overlaps Interval[10, 20]")); // IntegerIntervalExclusiveOverlapsTrue2
        Assert.True(await Helpers.CheckBool("Interval[4, 10] overlaps Interval(9, 20]")); // IntegerIntervalExclusiveOverlapsTrue3
        Assert.True(await Helpers.CheckBool("Interval[4, 11) overlaps Interval(9, 20]")); // IntegerIntervalExclusiveOverlapsTrue4
        Assert.False(await Helpers.CheckBool("Interval[4, 10] overlaps Interval(10, 20]")); // IntegerIntervalExclusiveOverlapsFalse
        Assert.False(await Helpers.CheckBool("Interval[4, 10) overlaps Interval[10, 20]")); // IntegerIntervalExclusiveOverlapsFalse2
        Assert.False(await Helpers.CheckBool("Interval[4, 10) overlaps Interval(10, 20]")); // IntegerIntervalExclusiveOverlapsFalse3
        Assert.False(await Helpers.CheckBool("Interval[4, 10) overlaps Interval(9, 20]")); // IntegerIntervalExclusiveOverlapsFalse4
        Assert.True(await Helpers.CheckBool("Interval[1.0, 10.0] overlaps Interval[4.0, 10.0]")); // DecimalIntervalOverlapsTrue
        Assert.False(await Helpers.CheckBool("Interval[1.0, 10.0] overlaps Interval[11.0, 20.0]")); // DecimalIntervalOverlapsFalse
        Assert.True(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] overlaps Interval[5.0 'g', 10.0 'g']")); // QuantityIntervalOverlapsTrue
        Assert.False(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] overlaps Interval[11.0 'g', 20.0 'g']")); // QuantityIntervalOverlapsFalse
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 25)] overlaps Interval[DateTime(2012, 1, 15), DateTime(2012, 1, 28)]")); // DateTimeOverlapsTrue
        Assert.False(await Helpers.CheckBool("Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 25)] overlaps Interval[DateTime(2012, 1, 26), DateTime(2012, 1, 28)]")); // DateTimeOverlapsFalse
        Assert.True(await Helpers.CheckBool("(Interval[DateTime(2012, 2, 25), DateTime(2012, 3, 26)] overlaps Interval[DateTime(2012, 1, 10), DateTime(2012, 2)]) is null")); // DateTimeOverlapsPrecisionLeftPossiblyStartsDuringRight
        Assert.True(await Helpers.CheckBool("(Interval[DateTime(2012, 1, 25), DateTime(2012, 2, 26)] overlaps Interval[DateTime(2012, 2), DateTime(2012, 3, 28)]) is null")); // DateTimeOverlapsPrecisioLeftPossiblyEndsDuringRight
        Assert.True(await Helpers.CheckBool("(Interval[DateTime(2012, 2), DateTime(2012, 3)] overlaps Interval[DateTime(2011, 1, 10), DateTime(2012)]) is null")); // DateTimeOverlapsPrecisionLeftPossiblyStartsAndEndsDuringRight
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012), DateTime(2013, 3)] overlaps Interval[DateTime(2012, 2), DateTime(2013, 2)]")); // DateTimeOverlapsPrecisionRightPossiblyStartsDuringLeftButEndsDuringLeft
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 2), DateTime(2013)] overlaps Interval[DateTime(2012, 3), DateTime(2013, 2)]")); // DateTimeOverlapsPrecisionRightStartsDuringLeftAndPossiblyEndsDuringLeft
        Assert.True(await Helpers.CheckBool("Interval[@T10:00:00.000, @T19:59:59.999] overlaps Interval[@T12:00:00.000, @T21:59:59.999]")); // TimeOverlapsTrue
        Assert.False(await Helpers.CheckBool("Interval[@T10:00:00.000, @T19:59:59.999] overlaps Interval[@T20:00:00.000, @T21:59:59.999]")); // TimeOverlapsFalse
    }

    [Fact]
    public async Task OverlapsBefore()
    {
        Assert.True(await Helpers.CheckBool("(Interval[null, null] overlaps before Interval[1, 10]) is null")); // TestOverlapsBeforeNull
        Assert.True(await Helpers.CheckBool("Interval[1, 10] overlaps before Interval[4, 10]")); // IntegerIntervalOverlapsBeforeTrue
        Assert.False(await Helpers.CheckBool("Interval[4, 10] overlaps before Interval[1, 10]")); // IntegerIntervalOverlapsBeforeFalse
        Assert.False(await Helpers.CheckBool("Interval[4, 10] overlaps before Interval[4, 10]")); // IntegerIntervalOverlapsBeforeFalse2
        Assert.True(await Helpers.CheckBool("Interval[4, 10] overlaps before Interval(4, 10]")); // IntegerIntervalExclusiveOverlapsBeforeTrue
        Assert.True(await Helpers.CheckBool("Interval(3, 10] overlaps before Interval(4, 10]")); // IntegerIntervalExclusiveOverlapsBeforeTrue2
        Assert.True(await Helpers.CheckBool("Interval(3, 10] overlaps before Interval[5, 10]")); // IntegerIntervalExclusiveOverlapsBeforeTrue3
        Assert.False(await Helpers.CheckBool("Interval(3, 10] overlaps before Interval(3, 10]")); // IntegerIntervalExclusiveOverlapsBeforeFalse
        Assert.False(await Helpers.CheckBool("Interval(3, 10] overlaps before Interval[4, 10]")); // IntegerIntervalExclusiveOverlapsBeforeFalse2
        Assert.False(await Helpers.CheckBool("Interval[4, 10] overlaps before Interval(3, 10]")); // IntegerIntervalExclusiveOverlapsBeforeFalse3
        Assert.True(await Helpers.CheckBool("Interval[1.0, 10.0] overlaps before Interval[4.0, 10.0]")); // DecimalIntervalOverlapsBeforeTrue
        Assert.False(await Helpers.CheckBool("Interval[4.0, 10.0] overlaps before Interval[1.0, 10.0]")); // DecimalIntervalOverlapsBeforeFalse
        Assert.True(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] overlaps before Interval[5.0 'g', 10.0 'g']")); // QuantityIntervalOverlapsBeforeTrue
        Assert.False(await Helpers.CheckBool("Interval[5.0 'g', 10.0 'g'] overlaps before Interval[1.0 'g', 10.0 'g']")); // QuantityIntervalOverlapsBeforeFalse
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 25)] overlaps Interval[DateTime(2012, 1, 15), DateTime(2012, 1, 28)]")); // DateTimeOverlapsBeforeTrue
        Assert.False(await Helpers.CheckBool("Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 25)] overlaps Interval[DateTime(2012, 1, 26), DateTime(2012, 1, 28)]")); // DateTimeOverlapsBeforeFalse
        Assert.True(await Helpers.CheckBool("Interval[@T10:00:00.000, @T19:59:59.999] overlaps Interval[@T12:00:00.000, @T21:59:59.999]")); // TimeOverlapsBeforeTrue
        Assert.False(await Helpers.CheckBool("Interval[@T10:00:00.000, @T19:59:59.999] overlaps Interval[@T20:00:00.000, @T21:59:59.999]")); // TimeOverlapsBeforeFalse
    }

    [Fact]
    public async Task OverlapsAfter()
    {
        Assert.True(await Helpers.CheckBool("(Interval[null, null] overlaps after Interval[1, 10]) is null")); // TestOverlapsAfterNull
        Assert.True(await Helpers.CheckBool("Interval[4, 15] overlaps after Interval[1, 10]")); // IntegerIntervalOverlapsAfterTrue
        Assert.False(await Helpers.CheckBool("Interval[4, 10] overlaps after Interval[1, 10]")); // IntegerIntervalOverlapsAfterFalse
        Assert.False(await Helpers.CheckBool("Interval[4, 10] overlaps after Interval[4, 10]")); // IntegerIntervalOverlapsAfterFalse2
        Assert.True(await Helpers.CheckBool("Interval[4, 11) overlaps after Interval[4, 9]")); // IntegerIntervalExclusiveOverlapsAfterTrue
        Assert.True(await Helpers.CheckBool("Interval[4, 11) overlaps after Interval[4, 10)")); // IntegerIntervalExclusiveOverlapsAfterTrue2
        Assert.True(await Helpers.CheckBool("Interval[4, 10] overlaps after Interval[4, 10)")); // IntegerIntervalExclusiveOverlapsAfterTrue3
        Assert.False(await Helpers.CheckBool("Interval[4, 11) overlaps after Interval[4, 11)")); // IntegerIntervalExclusiveOverlapsAfterFalse
        Assert.False(await Helpers.CheckBool("Interval[4, 11) overlaps after Interval[4, 10]")); // IntegerIntervalExclusiveOverlapsAfterFalse2
        Assert.False(await Helpers.CheckBool("Interval[4, 10] overlaps after Interval[4, 11)")); // IntegerIntervalExclusiveOverlapsAfterFalse3
        Assert.True(await Helpers.CheckBool("Interval[4.0, 15.0] overlaps after Interval[1.0, 10.0]")); // DecimalIntervalOverlapsAfterTrue
        Assert.False(await Helpers.CheckBool("Interval[4.0, 10.0] overlaps after Interval[1.0, 10.0]")); // DecimalIntervalOverlapsAfterFalse
        Assert.True(await Helpers.CheckBool("Interval[5.0 'g', 15.0 'g'] overlaps after Interval[1.0 'g', 10.0 'g']")); // QuantityIntervalOverlapsAfterTrue
        Assert.False(await Helpers.CheckBool("Interval[5.0 'g', 10.0 'g'] overlaps after Interval[1.0 'g', 10.0 'g']")); // QuantityIntervalOverlapsAfterFalse
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 15), DateTime(2012, 1, 28)] overlaps Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 25)]")); // DateTimeOverlapsAfterTrue
        Assert.False(await Helpers.CheckBool("Interval[DateTime(2012, 1, 26), DateTime(2012, 1, 28)] overlaps Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 25)]")); // DateTimeOverlapsAfterFalse
        Assert.True(await Helpers.CheckBool("Interval[@T12:00:00.000, @T21:59:59.999] overlaps Interval[@T10:00:00.000, @T19:59:59.999]")); // TimeOverlapsAfterTrue
        Assert.False(await Helpers.CheckBool("Interval[@T20:00:00.000, @T21:59:59.999] overlaps Interval[@T10:00:00.000, @T19:59:59.999]")); // TimeOverlapsAfterFalse
    }

    [Fact]
    public async Task PointFrom()
    {
        Assert.True(await Helpers.CheckBool("(point from Interval[null, null]) is null")); // TestPointFromNull
        Assert.True(await Helpers.CheckBool("point from Interval[1, 1] = 1")); // TestPointFromInteger
        Assert.True(await Helpers.CheckBool("point from Interval[1.0, 1.0] = 1.0")); // TestPointFromDecimal
        Assert.True(await Helpers.CheckBool("point from Interval[1.0 'cm', 1.0 'cm'] = 1.0'cm'")); // TestPointFromQuantity
    }

    [Fact]
    public async Task ProperContains()
    {
        Assert.True(await Helpers.CheckBool("Interval[@T12:00:00.000, @T21:59:59.999] properly includes @T12:00:00.001")); // TimeProperContainsTrue
        Assert.False(await Helpers.CheckBool("Interval[@T12:00:00.000, @T21:59:59.999] properly includes @T12:00:00.000")); // TimeProperContainsFalse
        Assert.True(await Helpers.CheckBool("(Interval[@T12:00:00.001, @T21:59:59.999] properly includes @T12:00:00) is null")); // TimeProperContainsNull
        Assert.True(await Helpers.CheckBool("Interval[@T12:00:00.000, @T21:59:59.999] properly includes second of @T12:00:01")); // TimeProperContainsPrecisionTrue
        Assert.False(await Helpers.CheckBool("Interval[@T12:00:00.001, @T21:59:59.999] properly includes second of @T12:00:00")); // TimeProperContainsPrecisionFalse
        Assert.True(await Helpers.CheckBool("(Interval[@T12:00:00.001, @T21:59:59.999] properly includes millisecond of @T12:00:00) is null")); // TimeProperContainsPrecisionNull
    }

    [Fact]
    public async Task ProperIn()
    {
        Assert.True(await Helpers.CheckBool("@T12:00:00.001 properly included in Interval[@T12:00:00.000, @T21:59:59.999]")); // TimeProperInTrue
        Assert.False(await Helpers.CheckBool("@T12:00:00.000 properly included in Interval[@T12:00:00.000, @T21:59:59.999]")); // TimeProperInFalse
        Assert.True(await Helpers.CheckBool("(@T12:00:00 properly included in Interval[@T12:00:00.001, @T21:59:59.999]) is null")); // TimeProperInNull
        Assert.True(await Helpers.CheckBool("@T12:00:01 properly included in second of Interval[@T12:00:00.000, @T21:59:59.999]")); // TimeProperInPrecisionTrue
        Assert.False(await Helpers.CheckBool("@T12:00:00 properly included in second of Interval[@T12:00:00.001, @T21:59:59.999]")); // TimeProperInPrecisionFalse
        Assert.True(await Helpers.CheckBool("(@T12:00:00 properly included in millisecond of Interval[@T12:00:00.001, @T21:59:59.999]) is null")); // TimeProperInPrecisionNull
    }

    [Fact]
    public async Task ProperlyIncludes()
    {
        Assert.True(await Helpers.CheckBool("Interval[null as Integer, null as Integer] properly includes Interval[1, 10]")); // NullBoundariesProperlyIncludesIntegerInterval
        Assert.True(await Helpers.CheckBool("Interval[1, 10] properly includes Interval[4, 10]")); // IntegerIntervalProperlyIncludesTrue
        Assert.False(await Helpers.CheckBool("Interval[1, 10] properly includes Interval[4, 15]")); // IntegerIntervalProperlyIncludesFalse
        Assert.True(await Helpers.CheckBool("Interval[1.0, 10.0] properly includes Interval[4.0, 10.0]")); // DecimalIntervalProperlyIncludesTrue
        Assert.False(await Helpers.CheckBool("Interval[1.0, 10.0] properly includes Interval[4.0, 15.0]")); // DecimalIntervalProperlyIncludesFalse
        Assert.True(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] properly includes Interval[5.0 'g', 10.0 'g']")); // QuantityIntervalProperlyIncludesTrue
        Assert.False(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] properly includes Interval[5.0 'g', 15.0 'g']")); // QuantityIntervalProperlyIncludesFalse
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 15), DateTime(2012, 1, 28)] properly includes Interval[DateTime(2012, 1, 16), DateTime(2012, 1, 27)]")); // DateTimeProperlyIncludesTrue
        Assert.False(await Helpers.CheckBool("Interval[DateTime(2012, 1, 15), DateTime(2012, 1, 28)] properly includes Interval[DateTime(2012, 1, 16), DateTime(2012, 1, 29)]")); // DateTimeProperlyIncludesFalse
        Assert.True(await Helpers.CheckBool("Interval[@T12:00:00.000, @T21:59:59.999] properly includes Interval[@T12:01:01.000, @T21:59:59.998]")); // TimeProperlyIncludesTrue
        Assert.False(await Helpers.CheckBool("Interval[@T12:00:00.000, @T21:59:59.999] properly includes Interval[@T12:01:01.000, @T22:00:00.000]")); // TimeProperlyIncludesFalse
    }

    [Fact]
    public async Task ProperlyIncludedIn()
    {
        Assert.True(await Helpers.CheckBool("Interval[1, 10] properly included in Interval[null, null]")); // IntegerIntervalProperlyIncludedInNullBoundaries
        Assert.True(await Helpers.CheckBool("Interval[4, 10] properly included in Interval[1, 10]")); // IntegerIntervalProperlyIncludedInTrue
        Assert.False(await Helpers.CheckBool("Interval[4, 15] properly included in Interval[1, 10]")); // IntegerIntervalProperlyIncludedInFalse
        Assert.True(await Helpers.CheckBool("Interval[4.0, 10.0] properly included in Interval[1.0, 10.0]")); // DecimalIntervalProperlyIncludedInTrue
        Assert.False(await Helpers.CheckBool("Interval[4.0, 15.0] properly included in Interval[1.0, 10.0]")); // DecimalIntervalProperlyIncludedInFalse
        Assert.True(await Helpers.CheckBool("Interval[5.0 'g', 10.0 'g'] properly included in Interval[1.0 'g', 10.0 'g']")); // QuantityIntervalProperlyIncludedInTrue
        Assert.False(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] properly included in Interval[5.0 'g', 15.0 'g']")); // QuantityIntervalProperlyIncludedInFalse
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 16), DateTime(2012, 1, 27)] properly included in Interval[DateTime(2012, 1, 15), DateTime(2012, 1, 28)]")); // DateTimeProperlyIncludedInTrue
        Assert.False(await Helpers.CheckBool("Interval[DateTime(2012, 1, 16), DateTime(2012, 1, 29)] properly included in Interval[DateTime(2012, 1, 15), DateTime(2012, 1, 28)]")); // DateTimeProperlyIncludedInFalse
        Assert.True(await Helpers.CheckBool("Interval[@T12:01:01.000, @T21:59:59.998] properly included in Interval[@T12:00:00.000, @T21:59:59.999]")); // TimeProperlyIncludedInTrue
        Assert.False(await Helpers.CheckBool("Interval[@T12:01:01.000, @T22:00:00.000] properly included in Interval[@T12:00:00.000, @T21:59:59.999]")); // TimeProperlyIncludedInFalse
    }

    [Fact]
    public async Task Start()
    {
        Assert.True(await Helpers.CheckBool("start of Interval[1, 10] = 1")); // IntegerIntervalStart
        Assert.True(await Helpers.CheckBool("start of Interval[1.0, 10.0] = 1.0")); // DecimalIntervalStart
        Assert.True(await Helpers.CheckBool("start of Interval[1.0 'g', 10.0 'g'] = 1.0'g'")); // QuantityIntervalStart
        Assert.True(await Helpers.CheckBool("start of Interval[@2016-05-01T00:00:00.000, @2016-05-02T00:00:00.000] = @2016-05-01T00:00:00.000")); // DateTimeIntervalStart
        Assert.True(await Helpers.CheckBool("start of Interval[@T00:00:00.000, @T23:59:59.599] = @T00:00:00.000")); // TimeIntervalStart
    }

    [Fact]
    public async Task Starts()
    {
        Assert.True(await Helpers.CheckBool("(Interval[null, null] starts Interval[1, 10]) is null")); // TestStartsNull
        Assert.True(await Helpers.CheckBool("Interval[4, 10] starts Interval[4, 15]")); // IntegerIntervalStartsTrue
        Assert.False(await Helpers.CheckBool("Interval[1, 10] starts Interval[4, 10]")); // IntegerIntervalStartsFalse
        Assert.True(await Helpers.CheckBool("Interval[4.0, 10.0] starts Interval[4.0, 15.0]")); // DecimalIntervalStartsTrue
        Assert.False(await Helpers.CheckBool("Interval[1.0, 10.0] starts Interval[4.0, 10.0]")); // DecimalIntervalStartsFalse
        Assert.True(await Helpers.CheckBool("Interval[5.0 'g', 10.0 'g'] starts Interval[5.0 'g', 15.0 'g']")); // QuantityIntervalStartsTrue
        Assert.False(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] starts Interval[5.0 'g', 10.0 'g']")); // QuantityIntervalStartsFalse
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 25)] starts Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 27)]")); // DateTimeStartsTrue
        Assert.False(await Helpers.CheckBool("Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 25)] starts Interval[DateTime(2012, 1, 6), DateTime(2012, 1, 27)]")); // DateTimeStartsFalse
        Assert.True(await Helpers.CheckBool("Interval[@T05:59:59.999, @T15:59:59.999] starts Interval[@T05:59:59.999, @T17:59:59.999]")); // TimeStartsTrue
        Assert.False(await Helpers.CheckBool("Interval[@T05:59:59.999, @T15:59:59.999] starts Interval[@T04:59:59.999, @T17:59:59.999]")); // TimeStartsFalse
    }

    [Fact]
    public async Task Union()
    {
        Assert.True(await Helpers.CheckBool("(Interval[null, null] union Interval[1, 10]) is null")); // TestUnionNull
        Assert.True(await Helpers.CheckBool("Interval[1, 10] union Interval[4, 15] = Interval [ 1, 15 ]")); // IntegerIntervalUnion1To15
        Assert.True(await Helpers.CheckBool("(Interval[1, 10] union Interval[44, 50]) is null")); // IntegerIntervalUnionNull
        Assert.True(await Helpers.CheckBool("Interval[1.0, 10.0] union Interval[4.0, 15.0] = Interval [ 1.0, 15.0 ]")); // DecimalIntervalUnion1To15
        Assert.True(await Helpers.CheckBool("(Interval[1.0, 10.0] union Interval[14.0, 15.0]) is null")); // DecimalIntervalUnionNull
        Assert.True(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] union Interval[5.0 'g', 15.0 'g'] = Interval [ 1.0 'g', 15.0 'g' ]")); // QuantityIntervalUnion1To15
        Assert.True(await Helpers.CheckBool("(Interval[1.0 'g', 10.0 'g'] union Interval[14.0 'g', 15.0 'g']) is null")); // QuantityIntervalUnionNull
        Assert.True(await Helpers.CheckBool("Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 25)] union Interval[DateTime(2012, 1, 25), DateTime(2012, 1, 28)] = Interval [ @2012-01-05T, @2012-01-28T ]")); // DateTimeUnion
        Assert.True(await Helpers.CheckBool("(Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 25)] union Interval[DateTime(2012, 1, 27), DateTime(2012, 1, 28)]) is null")); // DateTimeUnionNull
        Assert.True(await Helpers.CheckBool("Interval[@T05:59:59.999, @T15:59:59.999] union Interval[@T10:59:59.999, @T20:59:59.999] = Interval [ @T05:59:59.999, @T20:59:59.999 ]")); // TimeUnion
        Assert.True(await Helpers.CheckBool("(Interval[@T05:59:59.999, @T15:59:59.999] union Interval[@T16:59:59.999, @T20:59:59.999]) is null")); // TimeUnionNull
    }

    [Fact]
    public async Task Width()
    {
        Assert.True(await Helpers.CheckBool("width of Interval[1, 10] = 9")); // IntegerIntervalTestWidth9
        Assert.True(await Helpers.CheckBool("(width of (null as Interval<Any>)) is null")); // IntervalTestWidthNull
        Assert.True(await Helpers.CheckBool("width of Interval[4.0, 15.0] = 11.0")); // DecimalIntervalTestWidth11
        Assert.True(await Helpers.CheckBool("width of Interval[5.0 'g', 10.0 'g'] = 5.0'g'")); // QuantityIntervalTestWidth5
        Assert.True(await Helpers.CheckBool("width of Interval[DateTime(2012, 1, 5), DateTime(2012, 1, 25)] = ")); // DateTimeWidth
        Assert.True(await Helpers.CheckBool("width of Interval[@T05:59:59.999, @T15:59:59.999] = ")); // TimeWidth
    }

    [Fact]
    public async Task Interval()
    {
        Assert.True(await Helpers.CheckBool("Interval[1, 10] = Interval[1, 10]")); // IntegerIntervalTest
        Assert.True(await Helpers.CheckBool("Interval[11, 20] = Interval[11, 20]")); // IntegerIntervalTest2
        Assert.True(await Helpers.CheckBool("Interval[44, 50] = Interval[44, 50]")); // IntegerIntervalTest3
        Assert.True(await Helpers.CheckBool("Interval[4, 10] = Interval[4, 10]")); // IntegerIntervalTest4
        Assert.True(await Helpers.CheckBool("Interval[4, 15] = Interval[4, 15]")); // IntegerIntervalTest5
        Assert.True(await Helpers.CheckBool("Interval[1.0, 10.0] = Interval[1.0, 10.0]")); // DecimalIntervalTest
        Assert.True(await Helpers.CheckBool("Interval[11.0, 20.0] = Interval[11.0, 20.0]")); // DecimalIntervalTest2
        Assert.True(await Helpers.CheckBool("Interval[4.0, 10.0] = Interval[4.0, 10.0]")); // DecimalIntervalTest3
        Assert.True(await Helpers.CheckBool("Interval[4.0, 15.0] = Interval[4.0, 15.0]")); // DecimalIntervalTest4
        Assert.True(await Helpers.CheckBool("Interval[14.0, 15.0] = Interval[14.0, 15.0]")); // DecimalIntervalTest5
        Assert.True(await Helpers.CheckBool("Interval[1.0 'g', 10.0 'g'] = Interval[1.0 'g', 10.0 'g']")); // QuantityIntervalTest
        Assert.True(await Helpers.CheckBool("Interval[11.0 'g', 20.0 'g'] = Interval[11.0 'g', 20.0 'g']")); // QuantityIntervalTest2
        Assert.True(await Helpers.CheckBool("Interval[5.0 'g', 10.0 'g'] = Interval[5.0 'g', 10.0 'g']")); // QuantityIntervalTest3
        Assert.True(await Helpers.CheckBool("Interval[5.0 'g', 15.0 'g'] = Interval[5.0 'g', 15.0 'g']")); // QuantityIntervalTest4
        Assert.True(await Helpers.CheckBool("Interval[14.0 'g', 15.0 'g'] = Interval[14.0 'g', 15.0 'g']")); // QuantityIntervalTest5
        Assert.True(await Helpers.CheckBool("Interval[@2016-05-01T00:00:00.000, @2016-05-02T00:00:00.000] = Interval[@2016-05-01T00:00:00.000, @2016-05-02T00:00:00.000]")); // DateTimeIntervalTest
        Assert.True(await Helpers.CheckBool("Interval[@T00:00:00.000, @T23:59:59.599] = Interval[@T00:00:00.000, @T23:59:59.599]")); // TimeIntervalTest
        Assert.True(await Helpers.CheckBool("{Interval[1, 10], Interval[11, 20], Interval[44, 50]} = {Interval[1, 10], Interval[11, 20], Interval[44, 50]}")); // CollapseIntervalTestInteger
        Assert.True(await Helpers.CheckBool("Interval[5, 3] = ")); // InvalidIntegerInterval
        Assert.True(await Helpers.CheckBool("Interval[5, 5) = ")); // InvalidIntegerIntervalA
    }
}

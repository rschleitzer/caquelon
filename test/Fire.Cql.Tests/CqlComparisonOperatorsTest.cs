using Xunit;

namespace Fire.Cql.Tests;

public class CqlComparisonOperatorsTest
{
    [Fact]
    public async Task Between()
    {
        Assert.True(await Helpers.CheckBool("4 between 2 and 6")); // BetweenIntTrue
    }

    [Fact]
    public async Task Equal()
    {
        Assert.True(await Helpers.CheckBool("true = true")); // SimpleEqTrueTrue
        Assert.False(await Helpers.CheckBool("true = false")); // SimpleEqTrueFalse
        Assert.True(await Helpers.CheckBool("false = false")); // SimpleEqFalseFalse
        Assert.False(await Helpers.CheckBool("false = true")); // SimpleEqFalseTrue
        Assert.True(await Helpers.CheckBool("(null as String = null) is null")); // SimpleEqNullNull
        Assert.True(await Helpers.CheckBool("(true = null) is null")); // SimpleEqTrueNull
        Assert.True(await Helpers.CheckBool("(null = true) is null")); // SimpleEqNullTrue
        Assert.True(await Helpers.CheckBool("1 = 1")); // SimpleEqInt1Int1
        Assert.False(await Helpers.CheckBool("1 = 2")); // SimpleEqInt1Int2
        Assert.True(await Helpers.CheckBool("'a' = 'a'")); // SimpleEqStringAStringA
        Assert.False(await Helpers.CheckBool("'a' = 'b'")); // SimpleEqStringAStringB
        Assert.True(await Helpers.CheckBool("1.0 = 1.0")); // SimpleEqFloat1Float1
        Assert.False(await Helpers.CheckBool("1.0 = 2.0")); // SimpleEqFloat1Float2
        Assert.True(await Helpers.CheckBool("1.0 = 1")); // SimpleEqFloat1Int1
        Assert.False(await Helpers.CheckBool("1.0 = 2")); // SimpleEqFloat1Int2
        Assert.True(await Helpers.CheckBool("1'cm' = 1'cm'")); // QuantityEqCM1CM1
        Assert.True(await Helpers.CheckBool("1'cm' = 0.01'm'")); // QuantityEqCM1M01
        Assert.True(await Helpers.CheckBool("2.0'cm' = 2.00'cm'")); // QuantityEqDiffPrecision
        Assert.True(await Helpers.CheckBool("Tuple { Id : 1, Name : 'John' } = Tuple { Id : 1, Name : 'John' }")); // TupleEqJohnJohn
        Assert.False(await Helpers.CheckBool("Tuple { Id : 1, Name : 'John' } = Tuple { Id : 2, Name : 'Jane' }")); // TupleEqJohnJane
        Assert.False(await Helpers.CheckBool("Tuple { Id : 1, Name : 'John' } = Tuple { Id : 2, Name : 'John' }")); // TupleEqJohn1John2
        Assert.False(await Helpers.CheckBool("Tuple { Id : 1, Name : 'John' } = Tuple { Id : 2, Name : null }")); // TupleEqJohn1John2WithNullName
        Assert.False(await Helpers.CheckBool("Tuple { Id : null, Name : 'John' } = Tuple { Id : 1, Name : 'James' }")); // TupleEqDifferentNamesWithOneNullId
        Assert.True(await Helpers.CheckBool("Tuple { Id : 1, Name : null } = Tuple { Id : 1, Name : null }")); // TupleEqJohn1John1WithBothNamesNull
        Assert.True(await Helpers.CheckBool("Tuple { Id : null, Name : 'John' } = Tuple { Id : null, Name : 'John' }")); // TupleEqJohnJohnWithBothIdsNull
        Assert.True(await Helpers.CheckBool("(Tuple { Id : 1, Name : 'John' } = Tuple { Id : 1, Name : null }) is null")); // TupleEqJohn1John1WithNullName
        Assert.True(await Helpers.CheckBool("Tuple { dateId: 1, Date: DateTime(2012, 10, 5, 0, 0, 0, 0) } = Tuple { dateId: 1, Date: DateTime(2012, 10, 5, 0, 0, 0, 0) }")); // TupleEqDateTimeTrue
        Assert.False(await Helpers.CheckBool("Tuple { dateId: 1, Date: DateTime(2012, 10, 5, 0, 0, 0, 0) } = Tuple { dateId: 1, Date: DateTime(2012, 10, 5, 5, 0, 0, 0) }")); // TupleEqDateTimeFalse
        Assert.True(await Helpers.CheckBool("Tuple { timeId: 55, TheTime: @T05:15:15.541 } = Tuple { timeId: 55, TheTime: @T05:15:15.541 }")); // TupleEqTimeTrue
        Assert.False(await Helpers.CheckBool("Tuple { timeId: 55, TheTime: @T05:15:15.541 } = Tuple { timeId: 55, TheTime: @T05:15:15.540 }")); // TupleEqTimeFalse
        Assert.True(await Helpers.CheckBool("Today() = Today()")); // DateTimeEqTodayToday
        Assert.False(await Helpers.CheckBool("Today() = Today() - 1 days")); // DateTimeEqTodayYesterday
        Assert.True(await Helpers.CheckBool("DateTime(2014, 1, 5, 5, 0, 0, 0, 0) = DateTime(2014, 1, 5, 5, 0, 0, 0, 0)")); // DateTimeEqJanJan
        Assert.False(await Helpers.CheckBool("DateTime(2014, 1, 5, 5, 0, 0, 0, 0) = DateTime(2014, 7, 5, 5, 0, 0, 0, 0)")); // DateTimeEqJanJuly
        Assert.True(await Helpers.CheckBool("(DateTime(null) = DateTime(null)) is null")); // DateTimeEqNull
        Assert.True(await Helpers.CheckBool("@2014-01-25T14:30:14.559+01:00 = @2014-01-25T14:30:14.559+01:00")); // DateTimeUTC
        Assert.True(await Helpers.CheckBool("@2022-02-22T00:00:00.000-05:00 same day as @2022-02-22T04:59:00.000Z")); // DateTimeDayCompare
        Assert.True(await Helpers.CheckBool("@T10:00:00.000 = @T10:00:00.000")); // TimeEq10A10A
        Assert.False(await Helpers.CheckBool("@T10:00:00.000 = @T22:00:00.000")); // TimeEq10A10P
    }

    [Fact]
    public async Task Greater()
    {
        Assert.False(await Helpers.CheckBool("0 > 0")); // GreaterZZ
        Assert.False(await Helpers.CheckBool("0 > 1")); // GreaterZ1
        Assert.True(await Helpers.CheckBool("0 > -1")); // GreaterZNeg1
        Assert.False(await Helpers.CheckBool("0.0 > 0.0")); // GreaterDecZZ
        Assert.False(await Helpers.CheckBool("0.0 > 1.0")); // GreaterDecZ1
        Assert.True(await Helpers.CheckBool("0.0 > -1.0")); // GreaterDecZNeg1
        Assert.False(await Helpers.CheckBool("1.0 > 2")); // GreaterDec1Int2
        Assert.False(await Helpers.CheckBool("0'cm' > 0'cm'")); // GreaterCM0CM0
        Assert.False(await Helpers.CheckBool("0'cm' > 1'cm'")); // GreaterCM0CM1
        Assert.True(await Helpers.CheckBool("0'cm' > -1'cm'")); // GreaterCM0NegCM1
        Assert.True(await Helpers.CheckBool("1'm' > 1'cm'")); // GreaterM1CM1
        Assert.True(await Helpers.CheckBool("1'm' > 10'cm'")); // GreaterM1CM10
        Assert.False(await Helpers.CheckBool("'a' > 'a'")); // GreaterAA
        Assert.False(await Helpers.CheckBool("'a' > 'b'")); // GreaterAB
        Assert.True(await Helpers.CheckBool("'b' > 'a'")); // GreaterBA
        Assert.False(await Helpers.CheckBool("'a' > 'aa'")); // GreaterAThanAA
        Assert.True(await Helpers.CheckBool("'aa' > 'a'")); // GreaterAAThanA
        Assert.False(await Helpers.CheckBool("'Jack' > 'Jill'")); // GreaterJackJill
        Assert.True(await Helpers.CheckBool("DateTime(2012, 2, 12) > DateTime(2012, 2, 10)")); // DateTimeGreaterTrue
        Assert.False(await Helpers.CheckBool("DateTime(2012, 2, 12) > DateTime(2012, 2, 13)")); // DateTimeGreaterFalse
        Assert.True(await Helpers.CheckBool("@T10:00:00.001 > @T10:00:00.000")); // TimeGreaterTrue
        Assert.False(await Helpers.CheckBool("@T10:00:00.000 > @T10:00:00.001")); // TimeGreaterFalse
        Assert.True(await Helpers.CheckBool("(DateTime(2014) > DateTime(2014, 2, 15)) is null")); // UncertaintyGreaterNull
        Assert.True(await Helpers.CheckBool("DateTime(2015) > DateTime(2014, 2, 15)")); // UncertaintyGreaterTrue
        Assert.False(await Helpers.CheckBool("DateTime(2013) > DateTime(2014, 2, 15)")); // UncertaintyGreaterFalse
    }

    [Fact]
    public async Task GreaterOrEqual()
    {
        Assert.True(await Helpers.CheckBool("0 >= 0")); // GreaterOrEqualZZ
        Assert.False(await Helpers.CheckBool("0 >= 1")); // GreaterOrEqualZ1
        Assert.True(await Helpers.CheckBool("0 >= -1")); // GreaterOrEqualZNeg1
        Assert.True(await Helpers.CheckBool("0.0 >= 0.0")); // GreaterOrEqualDecZZ
        Assert.False(await Helpers.CheckBool("0.0 >= 1.0")); // GreaterOrEqualDecZ1
        Assert.True(await Helpers.CheckBool("0.0 >= -1.0")); // GreaterOrEqualDecZNeg1
        Assert.False(await Helpers.CheckBool("1.0 >= 2")); // GreaterOrEqualDec1Int2
        Assert.True(await Helpers.CheckBool("0'cm' >= 0'cm'")); // GreaterOrEqualCM0CM0
        Assert.False(await Helpers.CheckBool("0'cm' >= 1'cm'")); // GreaterOrEqualCM0CM1
        Assert.True(await Helpers.CheckBool("0'cm' >= -1'cm'")); // GreaterOrEqualCM0NegCM1
        Assert.True(await Helpers.CheckBool("1'm' >= 1'cm'")); // GreaterOrEqualM1CM1
        Assert.True(await Helpers.CheckBool("1'm' >= 10'cm'")); // GreaterOrEqualM1CM10
        Assert.True(await Helpers.CheckBool("'a' >= 'a'")); // GreaterOrEqualAA
        Assert.False(await Helpers.CheckBool("'a' >= 'b'")); // GreaterOrEqualAB
        Assert.True(await Helpers.CheckBool("'b' >= 'a'")); // GreaterOrEqualBA
        Assert.False(await Helpers.CheckBool("'a' >= 'aa'")); // GreaterOrEqualAThanAA
        Assert.True(await Helpers.CheckBool("'aa' >= 'a'")); // GreaterOrEqualAAThanA
        Assert.False(await Helpers.CheckBool("'Jack' >= 'Jill'")); // GreaterOrEqualJackJill
        Assert.True(await Helpers.CheckBool("DateTime(2012, 2, 12, 0, 0, 0, 0) >= DateTime(2012, 2, 10, 0, 0, 0, 0)")); // DateTimeGreaterEqTrue
        Assert.True(await Helpers.CheckBool("DateTime(2012, 2, 12, 0, 0, 0, 0) >= DateTime(2012, 2, 12, 0, 0, 0, 0)")); // DateTimeGreaterEqTrue2
        Assert.False(await Helpers.CheckBool("DateTime(2012, 2, 12, 0, 0, 0, 0) >= DateTime(2012, 2, 13, 0, 0, 0, 0)")); // DateTimeGreaterEqFalse
        Assert.True(await Helpers.CheckBool("@T10:00:00.001 >= @T10:00:00.000")); // TimeGreaterEqTrue
        Assert.True(await Helpers.CheckBool("@T10:00:00.000 >= @T10:00:00.000")); // TimeGreaterEqTrue2
        Assert.False(await Helpers.CheckBool("@T10:00:00.000 >= @T10:00:00.001")); // TimeGreaterEqFalse
        Assert.True(await Helpers.CheckBool("(DateTime(2014) >= DateTime(2014, 2, 15)) is null")); // UncertaintyGreaterEqualNull
        Assert.True(await Helpers.CheckBool("DateTime(2015) >= DateTime(2014, 2, 15)")); // UncertaintyGreaterEqualTrue
        Assert.False(await Helpers.CheckBool("DateTime(2013) >= DateTime(2014, 2, 15)")); // UncertaintyGreaterEqualFalse
    }

    [Fact]
    public async Task Less()
    {
        Assert.False(await Helpers.CheckBool("0 < 0")); // LessZZ
        Assert.True(await Helpers.CheckBool("0 < 1")); // LessZ1
        Assert.False(await Helpers.CheckBool("0 < -1")); // LessZNeg1
        Assert.False(await Helpers.CheckBool("0.0 < 0.0")); // LessDecZZ
        Assert.True(await Helpers.CheckBool("0.0 < 1.0")); // LessDecZ1
        Assert.False(await Helpers.CheckBool("0.0 < -1.0")); // LessDecZNeg1
        Assert.True(await Helpers.CheckBool("1.0 < 2")); // LessDec1Int2
        Assert.False(await Helpers.CheckBool("0'cm' < 0'cm'")); // LessCM0CM0
        Assert.True(await Helpers.CheckBool("0'cm' < 1'cm'")); // LessCM0CM1
        Assert.False(await Helpers.CheckBool("0'cm' < -1'cm'")); // LessCM0NegCM1
        Assert.False(await Helpers.CheckBool("1'm' < 1'cm'")); // LessM1CM1
        Assert.False(await Helpers.CheckBool("1'm' < 10'cm'")); // LessM1CM10
        Assert.False(await Helpers.CheckBool("'a' < 'a'")); // LessAA
        Assert.True(await Helpers.CheckBool("'a' < 'b'")); // LessAB
        Assert.False(await Helpers.CheckBool("'b' < 'a'")); // LessBA
        Assert.True(await Helpers.CheckBool("'a' < 'aa'")); // LessAThanAA
        Assert.False(await Helpers.CheckBool("'aa' < 'a'")); // LessAAThanA
        Assert.True(await Helpers.CheckBool("'Jack' < 'Jill'")); // LessJackJill
        Assert.True(await Helpers.CheckBool("DateTime(2012, 2, 9) < DateTime(2012, 2, 10)")); // DateTimeLessTrue
        Assert.False(await Helpers.CheckBool("DateTime(2012, 2, 14) < DateTime(2012, 2, 13)")); // DateTimeLessFalse
        Assert.True(await Helpers.CheckBool("@T10:00:00.001 < @T10:00:00.002")); // TimeLessTrue
        Assert.False(await Helpers.CheckBool("@T10:10:00.000 < @T10:00:00.001")); // TimeLessFalse
        Assert.True(await Helpers.CheckBool("(DateTime(2014) < DateTime(2014, 2, 15)) is null")); // UncertaintyLessNull
        Assert.True(await Helpers.CheckBool("DateTime(2013) < DateTime(2014, 2, 15)")); // UncertaintyLessTrue
        Assert.False(await Helpers.CheckBool("DateTime(2015) < DateTime(2014, 2, 15)")); // UncertaintyLessFalse
    }

    [Fact]
    public async Task LessOrEqual()
    {
        Assert.True(await Helpers.CheckBool("0 <= 0")); // LessOrEqualZZ
        Assert.True(await Helpers.CheckBool("0 <= 1")); // LessOrEqualZ1
        Assert.False(await Helpers.CheckBool("0 <= -1")); // LessOrEqualZNeg1
        Assert.True(await Helpers.CheckBool("0.0 <= 0.0")); // LessOrEqualDecZZ
        Assert.True(await Helpers.CheckBool("0.0 <= 1.0")); // LessOrEqualDecZ1
        Assert.False(await Helpers.CheckBool("0.0 <= -1.0")); // LessOrEqualDecZNeg1
        Assert.True(await Helpers.CheckBool("1.0 <= 2")); // LessOrEqualDec1Int2
        Assert.True(await Helpers.CheckBool("0'cm' <= 0'cm'")); // LessOrEqualCM0CM0
        Assert.True(await Helpers.CheckBool("0'cm' <= 1'cm'")); // LessOrEqualCM0CM1
        Assert.False(await Helpers.CheckBool("0'cm' <= -1'cm'")); // LessOrEqualCM0NegCM1
        Assert.False(await Helpers.CheckBool("1'm' <= 1'cm'")); // LessOrEqualM1CM1
        Assert.False(await Helpers.CheckBool("1'm' <= 10'cm'")); // LessOrEqualM1CM10
        Assert.True(await Helpers.CheckBool("'a' <= 'a'")); // LessOrEqualAA
        Assert.True(await Helpers.CheckBool("'a' <= 'b'")); // LessOrEqualAB
        Assert.False(await Helpers.CheckBool("'b' <= 'a'")); // LessOrEqualBA
        Assert.True(await Helpers.CheckBool("'a' <= 'aa'")); // LessOrEqualAThanAA
        Assert.False(await Helpers.CheckBool("'aa' <= 'a'")); // LessOrEqualAAThanA
        Assert.True(await Helpers.CheckBool("'Jack' <= 'Jill'")); // LessOrEqualJackJill
        Assert.True(await Helpers.CheckBool("DateTime(2012, 2, 9, 0, 0, 0, 0) <= DateTime(2012, 2, 10, 0, 0, 0, 0)")); // DateTimeLessEqTrue
        Assert.True(await Helpers.CheckBool("DateTime(2012, 2, 12, 0, 0, 0, 0) <= DateTime(2012, 2, 12, 0, 0, 0, 0)")); // DateTimeLessEqTrue2
        Assert.False(await Helpers.CheckBool("DateTime(2012, 2, 12, 1, 0, 0, 0) <= DateTime(2012, 2, 12, 0, 0, 0, 0)")); // DateTimeLessEqFalse
        Assert.True(await Helpers.CheckBool("@T10:00:00.001 <= @T10:00:00.002")); // TimeLessEqTrue
        Assert.True(await Helpers.CheckBool("@T10:00:00.000 <= @T10:00:00.000")); // TimeLessEqTrue2
        Assert.False(await Helpers.CheckBool("@T10:00:00.002 <= @T10:00:00.001")); // TimeLessEqFalse
        Assert.True(await Helpers.CheckBool("(DateTime(2014) <= DateTime(2014, 2, 15)) is null")); // UncertaintyLessEqualNull
        Assert.True(await Helpers.CheckBool("DateTime(2013) <= DateTime(2014, 2, 15)")); // UncertaintyLessEqualTrue
        Assert.False(await Helpers.CheckBool("DateTime(2015) <= DateTime(2014, 2, 15)")); // UncertaintyLessEqualFalse
    }

    [Fact]
    public async Task Equivalent()
    {
        Assert.True(await Helpers.CheckBool("true ~ true")); // EquivTrueTrue
        Assert.False(await Helpers.CheckBool("true ~ false")); // EquivTrueFalse
        Assert.True(await Helpers.CheckBool("false ~ false")); // EquivFalseFalse
        Assert.False(await Helpers.CheckBool("false ~ true")); // EquivFalseTrue
        Assert.True(await Helpers.CheckBool("null as String ~ null")); // EquivNullNull
        Assert.False(await Helpers.CheckBool("true ~ null")); // EquivTrueNull
        Assert.False(await Helpers.CheckBool("null ~ true")); // EquivNullTrue
        Assert.True(await Helpers.CheckBool("1 ~ 1")); // EquivInt1Int1
        Assert.False(await Helpers.CheckBool("1 ~ 2")); // EquivInt1Int2
        Assert.True(await Helpers.CheckBool("'a' ~ 'a'")); // EquivStringAStringA
        Assert.False(await Helpers.CheckBool("'a' ~ 'b'")); // EquivStringAStringB
        Assert.True(await Helpers.CheckBool("1.0 ~ 1.0")); // EquivFloat1Float1
        Assert.False(await Helpers.CheckBool("1.0 ~ 2.0")); // EquivFloat1Float2
        Assert.True(await Helpers.CheckBool("1.0 ~ 1")); // EquivFloat1Int1
        Assert.False(await Helpers.CheckBool("1.0 ~ 2")); // EquivFloat1Int2
        Assert.True(await Helpers.CheckBool("1'cm' ~ 1'cm'")); // EquivEqCM1CM1
        Assert.True(await Helpers.CheckBool("1'cm' ~ 0.01'm'")); // EquivEqCM1M01
        Assert.True(await Helpers.CheckBool("Tuple { Id : 1, Name : 'John' } ~ Tuple { Id : 1, Name : 'John' }")); // EquivTupleJohnJohn
        Assert.True(await Helpers.CheckBool("Tuple { Id : 1, Name : 'John', Position: null } ~ Tuple { Id : 1, Name : 'John', Position: null }")); // EquivTupleJohnJohnWithNulls
        Assert.False(await Helpers.CheckBool("Tuple { Id : 1, Name : 'John' } ~ Tuple { Id : 2, Name : 'Jane' }")); // EquivTupleJohnJane
        Assert.False(await Helpers.CheckBool("Tuple { Id : 1, Name : 'John' } ~ Tuple { Id : 2, Name : 'John' }")); // EquivTupleJohn1John2
        Assert.True(await Helpers.CheckBool("Today() ~ Today()")); // EquivDateTimeTodayToday
        Assert.False(await Helpers.CheckBool("Today() ~ Today() - 1 days")); // EquivDateTimeTodayYesterday
        Assert.True(await Helpers.CheckBool("@T10:00:00.000 ~ @T10:00:00.000")); // EquivTime10A10A
        Assert.False(await Helpers.CheckBool("@T10:00:00.000 ~ @T22:00:00.000")); // EquivTime10A10P
    }

    [Fact]
    public async Task NotEqual()
    {
        Assert.False(await Helpers.CheckBool("true != true")); // SimpleNotEqTrueTrue
        Assert.True(await Helpers.CheckBool("true != false")); // SimpleNotEqTrueFalse
        Assert.False(await Helpers.CheckBool("false != false")); // SimpleNotEqFalseFalse
        Assert.True(await Helpers.CheckBool("false != true")); // SimpleNotEqFalseTrue
        Assert.True(await Helpers.CheckBool("(null as String != null) is null")); // SimpleNotEqNullNull
        Assert.True(await Helpers.CheckBool("(true != null) is null")); // SimpleNotEqTrueNull
        Assert.True(await Helpers.CheckBool("(null != true) is null")); // SimpleNotEqNullTrue
        Assert.False(await Helpers.CheckBool("1 != 1")); // SimpleNotEqInt1Int1
        Assert.True(await Helpers.CheckBool("1 != 2")); // SimpleNotEqInt1Int2
        Assert.False(await Helpers.CheckBool("'a' != 'a'")); // SimpleNotEqStringAStringA
        Assert.True(await Helpers.CheckBool("'a' != 'b'")); // SimpleNotEqStringAStringB
        Assert.False(await Helpers.CheckBool("1.0 != 1.0")); // SimpleNotEqFloat1Float1
        Assert.True(await Helpers.CheckBool("1.0 != 2.0")); // SimpleNotEqFloat1Float2
        Assert.False(await Helpers.CheckBool("1.0 != 1")); // SimpleNotEqFloat1Int1
        Assert.True(await Helpers.CheckBool("1.0 != 2")); // SimpleNotEqFloat1Int2
        Assert.False(await Helpers.CheckBool("1'cm' != 1'cm'")); // QuantityNotEqCM1CM1
        Assert.False(await Helpers.CheckBool("1'cm' != 0.01'm'")); // QuantityNotEqCM1M01
        Assert.False(await Helpers.CheckBool("Tuple{ Id : 1, Name : 'John' } != Tuple{ Id : 1, Name : 'John' }")); // TupleNotEqJohnJohn
        Assert.True(await Helpers.CheckBool("Tuple{ Id : 1, Name : 'John' } != Tuple{ Id : 2, Name : 'Jane' }")); // TupleNotEqJohnJane
        Assert.True(await Helpers.CheckBool("Tuple{ Id : 1, Name : 'John' } != Tuple{ Id : 2, Name : 'John' }")); // TupleNotEqJohn1John2
        Assert.True(await Helpers.CheckBool("Tuple{ Id : 1, Name : 'John' } != Tuple{ Id : 2, Name : null }")); // TupleNotEqJohn1John2WithNullName
        Assert.True(await Helpers.CheckBool("Tuple{ Id : null, Name : 'John' } != Tuple{ Id : 1, Name : 'Joe' }")); // TupleNotEqDifferingNamesWithOneNullId
        Assert.False(await Helpers.CheckBool("Tuple{ Id : 1, Name : null } != Tuple{ Id : 1, Name : null }")); // TupleNotEqJohn1John1WithBothNamesNull
        Assert.False(await Helpers.CheckBool("Tuple{ Id : null, Name : 'John' } != Tuple{ Id : null, Name : 'John' }")); // TupleNotEqMatchingNamesWithNullIDs
        Assert.True(await Helpers.CheckBool("(Tuple{ Id : 1, Name : 'John' } != Tuple{ Id : 1, Name : null }) is null")); // TupleNotEqJohn1John1WithNullName
        Assert.False(await Helpers.CheckBool("Today() != Today()")); // DateTimeNotEqTodayToday
        Assert.True(await Helpers.CheckBool("Today() != Today() - 1 days")); // DateTimeNotEqTodayYesterday
        Assert.False(await Helpers.CheckBool("@T10:00:00.000 != @T10:00:00.000")); // TimeNotEq10A10A
        Assert.True(await Helpers.CheckBool("@T10:00:00.000 != @T22:00:00.000")); // TimeNotEq10A10P
    }
}

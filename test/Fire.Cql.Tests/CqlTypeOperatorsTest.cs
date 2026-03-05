using Xunit;

namespace Fire.Cql.Tests;

public class CqlTypeOperatorsTest
{
    [Fact]
    public async Task As()
    {
        Assert.True(await Helpers.CheckBool("45.5 'g' as Quantity = 45.5 'g'")); // AsQuantity
        Assert.True(await Helpers.CheckBool("cast 45.5 'g' as Quantity = 45.5 'g'")); // CastAsQuantity
        Assert.True(await Helpers.CheckBool("DateTime(2014, 01, 01) as DateTime = @2014-01-01T")); // AsDateTime
    }

    [Fact]
    public async Task Convert()
    {
        Assert.True(await Helpers.CheckBool("convert 5 to Decimal = 5.0")); // IntegerToDecimal
        Assert.True(await Helpers.CheckBool("convert 5 to String = '5'")); // IntegerToString
        Assert.True(await Helpers.CheckBool("(convert 'foo' to Integer) is null")); // StringToIntegerError
        Assert.True(await Helpers.CheckBool("convert '2014-01-01' to DateTime = @2014-01-01T")); // StringToDateTime
        Assert.True(await Helpers.CheckBool("convert 'T14:30:00.0' to Time = @T14:30:00.000")); // StringToTime
        Assert.True(await Helpers.CheckBool("(convert '2014/01/01' to DateTime) is null")); // StringToDateTimeMalformed
    }

    [Fact]
    public async Task Is()
    {
        Assert.True(await Helpers.CheckBool("5 is Integer")); // IntegerIsInteger
        Assert.False(await Helpers.CheckBool("'5' is Integer")); // StringIsInteger
        Assert.True(await Helpers.CheckBool("System.ValueSet{id: '123'} is Vocabulary")); // ValueSetIsVocabulary
    }

    [Fact]
    public async Task ToBoolean()
    {
        Assert.False(await Helpers.CheckBool("ToBoolean('NO')")); // StringNoToBoolean
    }

    [Fact]
    public async Task ToConcept()
    {
        Assert.True(await Helpers.CheckBool("ToConcept(Code { code: '8480-6' }) =   Concept {  codes: Code { code: '8480-6' }  }  ")); // CodeToConcept1
    }

    [Fact]
    public async Task ToDateTime()
    {
        Assert.True(await Helpers.CheckBool("ToDateTime('2014-01-01') = @2014-01-01T")); // ToDateTime1
        Assert.True(await Helpers.CheckBool("ToDateTime('2014-01-01T12:05') = @2014-01-01T12:05")); // ToDateTime2
        Assert.True(await Helpers.CheckBool("ToDateTime('2014-01-01T12:05:05.955') = @2014-01-01T12:05:05.955")); // ToDateTime3
        Assert.True(await Helpers.CheckBool("ToDateTime('2014-01-01T12:05:05.955+01:30') = @2014-01-01T12:05:05.955+01:30")); // ToDateTime4
        Assert.True(await Helpers.CheckBool("ToDateTime('2014-01-01T12:05:05.955-01:15') = @2014-01-01T12:05:05.955-01:15")); // ToDateTime5
        Assert.True(await Helpers.CheckBool("ToDateTime('2014-01-01T12:05:05.955Z') = @2014-01-01T12:05:05.955+00:00")); // ToDateTime6
        Assert.True(await Helpers.CheckBool("(ToDateTime('2014/01/01T12:05:05.955Z')) is null")); // ToDateTimeMalformed
        Assert.True(await Helpers.CheckBool("ToDateTime(@2014-01-01) = @2014-01-01T")); // ToDateTimeDate
        Assert.True(await Helpers.CheckBool("hour from ToDateTime(@2014-01-01) is null")); // ToDateTimeTimeUnspecified
    }

    [Fact]
    public async Task ToDecimal()
    {
        Assert.True(await Helpers.CheckBool("ToDecimal('+25.5') = 25.5")); // String25D5ToDecimal
    }

    [Fact]
    public async Task ToInteger()
    {
        Assert.True(await Helpers.CheckBool("ToInteger('-25') = -25")); // StringNeg25ToInteger
    }

    [Fact]
    public async Task ToQuantity()
    {
        Assert.True(await Helpers.CheckBool("ToQuantity('5.5 \\'cm\\'') = 5.5'cm'")); // String5D5CMToQuantity
    }

    [Fact]
    public async Task CqlToString()
    {
        Assert.True(await Helpers.CheckBool("ToString(-5) = '-5'")); // IntegerNeg5ToString
        Assert.True(await Helpers.CheckBool("ToString(18.55) = '18.55'")); // Decimal18D55ToString
        Assert.True(await Helpers.CheckBool("ToString(5.5 'cm') = '5.5 \\'cm\\''")); // Quantity5D5CMToString
        Assert.True(await Helpers.CheckBool("ToString(true) = 'true'")); // BooleanTrueToString
    }

    [Fact]
    public async Task ToTime()
    {
        Assert.True(await Helpers.CheckBool("ToTime('T14:30:00.0') = @T14:30:00.000")); // ToTime1
        Assert.True(await Helpers.CheckBool("ToTime('T14:30:00.0+05:30') = @T14:30:00.000")); // ToTime2
        Assert.True(await Helpers.CheckBool("ToTime('T14:30:00.0-05:45') = @T14:30:00.000")); // ToTime3
        Assert.True(await Helpers.CheckBool("ToTime('T14:30:00.0Z') = @T14:30:00.000")); // ToTime4
        Assert.True(await Helpers.CheckBool("(ToTime('T14-30-00.0')) is null")); // ToTimeMalformed
    }
}

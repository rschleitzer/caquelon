using Xunit;

namespace Fire.Cql.Tests;

public class ValueLiteralsAndSelectors
{
    [Fact]
    public async Task Null()
    {
        Assert.True(await Helpers.CheckBool("(null) is null")); // Null
    }

    [Fact]
    public async Task Boolean()
    {
        Assert.False(await Helpers.CheckBool("false")); // BooleanFalse
        Assert.True(await Helpers.CheckBool("true")); // BooleanTrue
    }

    [Fact]
    public async Task Integer()
    {
        Assert.True(await Helpers.CheckBool("0 = 0")); // IntegerZero
        Assert.True(await Helpers.CheckBool("+0 = 0")); // IntegerPosZero
        Assert.True(await Helpers.CheckBool("-0 = 0")); // IntegerNegZero
        Assert.True(await Helpers.CheckBool("1 = 1")); // IntegerOne
        Assert.True(await Helpers.CheckBool("+1 = 1")); // IntegerPosOne
        Assert.True(await Helpers.CheckBool("-1 = -1")); // IntegerNegOne
        Assert.True(await Helpers.CheckBool("2 = 2")); // IntegerTwo
        Assert.True(await Helpers.CheckBool("+2 = 2")); // IntegerPosTwo
        Assert.True(await Helpers.CheckBool("-2 = -2")); // IntegerNegTwo
        Assert.True(await Helpers.CheckBool("Power(10,9) = 1000000000")); // Integer10Pow9
        Assert.True(await Helpers.CheckBool("+Power(10,9) = 1000000000")); // IntegerPos10Pow9
        Assert.True(await Helpers.CheckBool("-Power(10,9) = -1000000000")); // IntegerNeg10Pow9
        Assert.True(await Helpers.CheckBool("Power(2,30)-1+Power(2,30) = 2147483647")); // Integer2Pow31ToZero1IntegerMaxValue
        Assert.True(await Helpers.CheckBool("+Power(2,30)-1+Power(2,30) = 2147483647")); // IntegerPos2Pow31ToZero1IntegerMaxValue
        Assert.True(await Helpers.CheckBool("-Power(2,30)+1-Power(2,30) = -2147483647")); // IntegerNeg2Pow31ToZero1
        Assert.True(await Helpers.CheckBool("-Power(2,30)-Power(2,30) = -2147483648")); // IntegerNeg2Pow31IntegerMinValue
    }

    [Fact]
    public async Task Decimal()
    {
        Assert.True(await Helpers.CheckBool("0.0 = 0.0")); // DecimalZero
        Assert.True(await Helpers.CheckBool("+0.0 = 0.0")); // DecimalPosZero
        Assert.True(await Helpers.CheckBool("-0.0 = 0.0")); // DecimalNegZero
        Assert.True(await Helpers.CheckBool("1.0 = 1.0")); // DecimalOne
        Assert.True(await Helpers.CheckBool("+1.0 = 1.0")); // DecimalPosOne
        Assert.True(await Helpers.CheckBool("-1.0 = -1.0")); // DecimalNegOne
        Assert.True(await Helpers.CheckBool("2.0 = 2.0")); // DecimalTwo
        Assert.True(await Helpers.CheckBool("+2.0 = 2.0")); // DecimalPosTwo
        Assert.True(await Helpers.CheckBool("-2.0 = -2.0")); // DecimalNegTwo
        Assert.True(await Helpers.CheckBool("Power(10.0,9.0) = 1000000000.0")); // Decimal10Pow9
        Assert.True(await Helpers.CheckBool("+Power(10.0,9.0) = 1000000000.0")); // DecimalPos10Pow9
        Assert.True(await Helpers.CheckBool("-Power(10.0,9.0) = -1000000000.0")); // DecimalNeg10Pow9
        Assert.True(await Helpers.CheckBool("Power(2.0,30.0)-1+Power(2.0,30.0) = 2147483647.0")); // Decimal2Pow31ToZero1
        Assert.True(await Helpers.CheckBool("+Power(2.0,30.0)-1+Power(2.0,30.0) = 2147483647.0")); // DecimalPos2Pow31ToZero1
        Assert.True(await Helpers.CheckBool("-Power(2.0,30.0)+1.0-Power(2.0,30.0) = -2147483647.0")); // DecimalNeg2Pow31ToZero1
        Assert.True(await Helpers.CheckBool("Power(2.0,30.0)+Power(2.0,30.0) = 2147483648.0")); // Decimal2Pow31
        Assert.True(await Helpers.CheckBool("+Power(2.0,30.0)+Power(2.0,30.0) = 2147483648.0")); // DecimalPos2Pow31
        Assert.True(await Helpers.CheckBool("-Power(2.0,30.0)-Power(2.0,30.0) = -2147483648.0")); // DecimalNeg2Pow31
        Assert.True(await Helpers.CheckBool("Power(2.0,30.0)+1.0+Power(2.0,30.0) = 2147483649.0")); // Decimal2Pow31ToInf1
        Assert.True(await Helpers.CheckBool("+Power(2.0,30.0)+1.0+Power(2.0,30.0) = 2147483649.0")); // DecimalPos2Pow31ToInf1
        Assert.True(await Helpers.CheckBool("-Power(2.0,30.0)-1.0-Power(2.0,30.0) = -2147483649.0")); // DecimalNeg2Pow31ToInf1
        Assert.True(await Helpers.CheckBool("0.00000000 = 0.00000000")); // DecimalZeroStep
        Assert.True(await Helpers.CheckBool("+0.00000000 = 0.00000000")); // DecimalPosZeroStep
        Assert.True(await Helpers.CheckBool("-0.00000000 = 0.00000000")); // DecimalNegZeroStep
        Assert.True(await Helpers.CheckBool("Power(10,-8) = 0.00000001")); // DecimalOneStep
        Assert.True(await Helpers.CheckBool("+Power(10,-8) = 0.00000001")); // DecimalPosOneStep
        Assert.True(await Helpers.CheckBool("-Power(10,-8) = -0.00000001")); // DecimalNegOneStep
        Assert.True(await Helpers.CheckBool("2.0*Power(10,-8) = 0.00000002")); // DecimalTwoStep
        Assert.True(await Helpers.CheckBool("+2.0*Power(10,-8) = 0.00000002")); // DecimalPosTwoStep
        Assert.True(await Helpers.CheckBool("-2.0*Power(10,-8) = -0.00000002")); // DecimalNegTwoStep
        Assert.True(await Helpers.CheckBool("Power(10,-7) = 0.0000001")); // DecimalTenStep
        Assert.True(await Helpers.CheckBool("+Power(10,-7) = 0.0000001")); // DecimalPosTenStep
        Assert.True(await Helpers.CheckBool("-Power(10,-7) = -0.0000001")); // DecimalNegTenStep
        Assert.True(await Helpers.CheckBool("10*1000000000000000000000000000.00000000-0.00000001 = 9999999999999999999999999999.99999999")); // Decimal10Pow28ToZeroOneStepDecimalMaxValue
        Assert.True(await Helpers.CheckBool("+10*1000000000000000000000000000.00000000-0.00000001 = 9999999999999999999999999999.99999999")); // DecimalPos10Pow28ToZeroOneStepDecimalMaxValue
        Assert.True(await Helpers.CheckBool("-10*1000000000000000000000000000.00000000+0.00000001 = -9999999999999999999999999999.99999999")); // DecimalNeg10Pow28ToZeroOneStepDecimalMinValue
    }

    [Fact]
    public async Task String()
    {
    }

    [Fact]
    public async Task DateTime()
    {
    }

    [Fact]
    public async Task Time()
    {
    }

    [Fact]
    public async Task List()
    {
    }

    [Fact]
    public async Task Interval()
    {
    }

    [Fact]
    public async Task Tuple()
    {
    }

    [Fact]
    public async Task Quantity()
    {
    }

    [Fact]
    public async Task Code()
    {
    }

    [Fact]
    public async Task Concept()
    {
    }

    [Fact]
    public async Task Instance()
    {
    }
}

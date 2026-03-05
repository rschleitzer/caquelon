using Xunit;

namespace Fire.Cql.Tests;

public class CqlArithmeticFunctionsTest
{
    [Fact]
    public async Task Abs()
    {
        Assert.True(await Helpers.CheckBool("(Abs(null as Integer)) is null")); // AbsNull
        Assert.True(await Helpers.CheckBool("Abs(0) = 0")); // Abs0
        Assert.True(await Helpers.CheckBool("Abs(-1) = 1")); // AbsNeg1
        Assert.True(await Helpers.CheckBool("Abs(-1.0) = 1.0")); // AbsNeg1Dec
        Assert.True(await Helpers.CheckBool("Abs(0.0) = 0.0")); // Abs0Dec
        Assert.True(await Helpers.CheckBool("Abs(-1.0'cm') = 1.0'cm'")); // Abs1cm
        Assert.True(await Helpers.CheckBool("Abs(-1L) = 1L")); // AbsLong
    }

    [Fact]
    public async Task Add()
    {
        Assert.True(await Helpers.CheckBool("(1 + null) is null")); // AddNull
        Assert.True(await Helpers.CheckBool("1 + 1 = 2")); // Add11
        Assert.True(await Helpers.CheckBool("1L + 2L = 3L")); // Add1L2L
        Assert.True(await Helpers.CheckBool("1.0 + 1.0 = 2.0")); // Add1D1D
        Assert.True(await Helpers.CheckBool("1'g/cm3' + 1'g/cm3' = 2.0'g/cm3'")); // Add1Q1Q
        Assert.True(await Helpers.CheckBool("1 + 2.0 = 3.0")); // AddIAndD
        Assert.True(await Helpers.CheckBool("1L + 1L = 2L")); // Add1L1L
    }

    [Fact]
    public async Task Ceiling()
    {
        Assert.True(await Helpers.CheckBool("(Ceiling(null as Decimal)) is null")); // CeilingNull
        Assert.True(await Helpers.CheckBool("Ceiling(1.0) = 1")); // Ceiling1D
        Assert.True(await Helpers.CheckBool("Ceiling(1.1) = 2")); // Ceiling1D1
        Assert.True(await Helpers.CheckBool("Ceiling(-0.1) = 0")); // CeilingNegD1
        Assert.True(await Helpers.CheckBool("Ceiling(-1.0) = -1")); // CeilingNeg1
        Assert.True(await Helpers.CheckBool("Ceiling(-1.1) = -1")); // CeilingNeg1D1
        Assert.True(await Helpers.CheckBool("Ceiling(1) = 1")); // Ceiling1I
    }

    [Fact]
    public async Task Divide()
    {
        Assert.True(await Helpers.CheckBool("(1 / null) is null")); // DivideNull
        Assert.True(await Helpers.CheckBool("(1 / 0) is null")); // Divide10
        Assert.True(await Helpers.CheckBool("0 / 1 = 0.0")); // Divide01
        Assert.True(await Helpers.CheckBool("1 / 1 = 1.0")); // Divide11
        Assert.True(await Helpers.CheckBool("1L / 1L = 1.0")); // Divide1L1L
        Assert.True(await Helpers.CheckBool("1.0 / 1.0 = 1.0")); // Divide1d1d
        Assert.True(await Helpers.CheckBool("Round(10 / 3, 8) = 3.33333333")); // Divide103
        Assert.True(await Helpers.CheckBool("1'g/cm3' / 1.0 = 1.0'g/cm3'")); // Divide1Q1
        Assert.True(await Helpers.CheckBool("1'g/cm3' / 1'g/cm3' = 1.0'1'")); // Divide1Q1Q
        Assert.True(await Helpers.CheckBool("10 / 5.0 = 2.0")); // Divide10I5D
        Assert.True(await Helpers.CheckBool("10 / 5 = 2.0")); // Divide10I5I
        Assert.True(await Helpers.CheckBool("10.0 'g' / 5 = 2.0'g'")); // Divide10Q5I
    }

    [Fact]
    public async Task Floor()
    {
        Assert.True(await Helpers.CheckBool("(Floor(null as Decimal)) is null")); // FloorNull
        Assert.True(await Helpers.CheckBool("Floor(1) = 1")); // Floor1
        Assert.True(await Helpers.CheckBool("Floor(1.0) = 1")); // Floor1D
        Assert.True(await Helpers.CheckBool("Floor(1.1) = 1")); // Floor1D1
        Assert.True(await Helpers.CheckBool("Floor(-0.1) = -1")); // FloorNegD1
        Assert.True(await Helpers.CheckBool("Floor(-1.0) = -1")); // FloorNeg1
        Assert.True(await Helpers.CheckBool("Floor(-1.1) = -2")); // FloorNeg1D1
        Assert.True(await Helpers.CheckBool("Floor(2) = 2")); // Floor2I
    }

    [Fact]
    public async Task Exp()
    {
        Assert.True(await Helpers.CheckBool("(Exp(null as Decimal)) is null")); // ExpNull
        Assert.True(await Helpers.CheckBool("Exp(0) = 1.0")); // Exp0
        Assert.True(await Helpers.CheckBool("Exp(-0) = 1.0")); // ExpNeg0
        Assert.True(await Helpers.CheckBool("Round(Exp(1), 8) = 2.71828183")); // Exp1
        Assert.True(await Helpers.CheckBool("Round(Exp(1L), 8) = 2.71828183")); // Exp1L
        Assert.True(await Helpers.CheckBool("Round(Exp(-1), 8) = 0.36787944")); // ExpNeg1
        Assert.True(await Helpers.CheckBool("Exp(1000) = ")); // Exp1000
        Assert.True(await Helpers.CheckBool("Exp(1000.0) = ")); // Exp1000D
    }

    [Fact]
    public async Task HighBoundary()
    {
        Assert.True(await Helpers.CheckBool("HighBoundary(1.587, 8) = 1.58799999")); // HighBoundaryDecimal
        Assert.True(await Helpers.CheckBool("HighBoundary(@2014, 6) = @2014-12")); // HighBoundaryDateMonth
        Assert.True(await Helpers.CheckBool("HighBoundary(@2014-01-01T08, 17) = @2014-01-01T08:59:59.999")); // HighBoundaryDateTimeMillisecond
        Assert.True(await Helpers.CheckBool("HighBoundary(@T10:30, 9) = @T10:30:59.999")); // HighBoundaryTimeMillisecond
    }

    [Fact]
    public async Task Log()
    {
        Assert.True(await Helpers.CheckBool("(Log(null, null)) is null")); // LogNullNull
        Assert.True(await Helpers.CheckBool("(Log(1, null)) is null")); // Log1BaseNull
        Assert.True(await Helpers.CheckBool("(Log(1, 1)) is null")); // Log1Base1
        Assert.True(await Helpers.CheckBool("(Log(2, 1)) is null")); // Log2Base1
        Assert.True(await Helpers.CheckBool("Log(1, 2) = 0.0")); // Log1Base2
        Assert.True(await Helpers.CheckBool("Log(1, 100) = 0.0")); // Log1Base100
        Assert.True(await Helpers.CheckBool("Log(1L, 100L) = 0.0")); // Log1Base100L
        Assert.True(await Helpers.CheckBool("Log(16, 2) = 4.0")); // Log16Base2
        Assert.True(await Helpers.CheckBool("Log(0.125, 2) = -3.0")); // LogD125Base2
    }

    [Fact]
    public async Task LowBoundary()
    {
        Assert.True(await Helpers.CheckBool("LowBoundary(1.587, 8) = 1.58700000")); // LowBoundaryDecimal
        Assert.True(await Helpers.CheckBool("LowBoundary(@2014, 6) = @2014-01")); // LowBoundaryDateMonth
        Assert.True(await Helpers.CheckBool("LowBoundary(@2014-01-01T08, 17) = @2014-01-01T08:00:00.000")); // LowBoundaryDateTimeMillisecond
        Assert.True(await Helpers.CheckBool("LowBoundary(@T10:30, 9) = @T10:30:00.000")); // LowBoundaryTimeMillisecond
    }

    [Fact]
    public async Task Ln()
    {
        Assert.True(await Helpers.CheckBool("(Ln(null)) is null")); // LnNull
        Assert.True(await Helpers.CheckBool("Ln(0) = ")); // Ln0
        Assert.True(await Helpers.CheckBool("Ln(-0) = ")); // LnNeg0
        Assert.True(await Helpers.CheckBool("Ln(1) = 0.0")); // Ln1
        Assert.True(await Helpers.CheckBool("Ln(1L) = 0.0")); // Ln1L
        Assert.True(await Helpers.CheckBool("(Ln(-1)) is null")); // LnNeg1
        Assert.True(await Helpers.CheckBool("Round(Ln(1000), 8) = 6.90775528")); // Ln1000
        Assert.True(await Helpers.CheckBool("Round(Ln(1000.0), 8) = 6.90775528")); // Ln1000D
    }

    [Fact]
    public async Task MinValue()
    {
        Assert.True(await Helpers.CheckBool("minimum Integer = -2147483648")); // IntegerMinValue
        Assert.True(await Helpers.CheckBool("minimum Long = -9223372036854775808L")); // LongMinValue
        Assert.True(await Helpers.CheckBool("minimum Decimal = -99999999999999999999.99999999")); // DecimalMinValue
        Assert.True(await Helpers.CheckBool("minimum DateTime = @0001-01-01T00:00:00.000Z")); // DateTimeMinValue
        Assert.True(await Helpers.CheckBool("minimum Date = @0001-01-01")); // DateMinValue
        Assert.True(await Helpers.CheckBool("minimum Time = @T00:00:00.000")); // TimeMinValue
        Assert.True(await Helpers.CheckBool("minimum Boolean = ")); // BooleanMinValue
    }

    [Fact]
    public async Task MaxValue()
    {
        Assert.True(await Helpers.CheckBool("maximum Integer = 2147483647")); // IntegerMaxValue
        Assert.True(await Helpers.CheckBool("maximum Long = 9223372036854775807L")); // LongMaxValue
        Assert.True(await Helpers.CheckBool("maximum Decimal = 99999999999999999999.99999999")); // DecimalMaxValue
        Assert.True(await Helpers.CheckBool("maximum DateTime = @9999-12-31T23:59:59.999Z")); // DateTimeMaxValue
        Assert.True(await Helpers.CheckBool("maximum Date = @9999-12-31")); // DateMaxValue
        Assert.True(await Helpers.CheckBool("maximum Time = @T23:59:59.999")); // TimeMaxValue
        Assert.True(await Helpers.CheckBool("maximum Boolean = ")); // BooleanMaxValue
    }

    [Fact]
    public async Task Modulo()
    {
        Assert.True(await Helpers.CheckBool("(1 mod null) is null")); // ModuloNull
        Assert.True(await Helpers.CheckBool("(0 mod 0) is null")); // Modulo0By0
        Assert.True(await Helpers.CheckBool("4 mod 2 = 0")); // Modulo4By2
        Assert.True(await Helpers.CheckBool("4L mod 2L = 0L")); // Modulo4LBy2L
        Assert.True(await Helpers.CheckBool("4.0 mod 2.0 = 0.0")); // Modulo4DBy2D
        Assert.True(await Helpers.CheckBool("10 mod 3 = 1")); // Modulo10By3
        Assert.True(await Helpers.CheckBool("10.0 mod 3.0 = 1.0")); // Modulo10DBy3D
        Assert.True(await Helpers.CheckBool("10 mod 3.0 = 1.0")); // Modulo10IBy3D
        Assert.True(await Helpers.CheckBool("3.5 mod 3 = 0.5")); // ModuloDResult
        Assert.True(await Helpers.CheckBool("3.5 'cm' mod 3 'cm' = 0.5 'cm'")); // ModuloQuantity
        Assert.True(await Helpers.CheckBool("10.0 'g' mod 3.0 'g' = 1.0 'g'")); // Modulo10By3Quantity
        Assert.True(await Helpers.CheckBool("(10.0 'g' mod 0.0 'g') is null")); // Modulo10By0Quantity
    }

    [Fact]
    public async Task Multiply()
    {
        Assert.True(await Helpers.CheckBool("(1 * null) is null")); // MultiplyNull
        Assert.True(await Helpers.CheckBool("1 * 1 = 1")); // Multiply1By1
        Assert.True(await Helpers.CheckBool("2L * 3L = 6L")); // Multiply2LBy3L
        Assert.True(await Helpers.CheckBool("1.0 * 2.0 = 2.0")); // Multiply1DBy2D
        Assert.True(await Helpers.CheckBool("1 * 1L = 1L")); // Multiply1By1L
        Assert.True(await Helpers.CheckBool("1 * 2.0 = 2.0")); // Multiply1IBy2D
        Assert.True(await Helpers.CheckBool("1.0 'cm' * 2.0 'cm' = 2.0'cm2'")); // Multiply1CMBy2CM
    }

    [Fact]
    public async Task Negate()
    {
        Assert.True(await Helpers.CheckBool("(-(null as Integer)) is null")); // NegateNull
        Assert.True(await Helpers.CheckBool("-0 = 0")); // Negate0
        Assert.True(await Helpers.CheckBool("-(-0) = 0")); // NegateNeg0
        Assert.True(await Helpers.CheckBool("-1 = -1")); // Negate1
        Assert.True(await Helpers.CheckBool("-1L = -1L")); // Negate1L
        Assert.True(await Helpers.CheckBool("-9223372036854775807L = -9223372036854775807L")); // NegateMaxLong
        Assert.True(await Helpers.CheckBool("-(-1) = 1")); // NegateNeg1
        Assert.True(await Helpers.CheckBool("-(-1L) = 1L")); // NegateNeg1L
        Assert.True(await Helpers.CheckBool("-(0.0) = 0.0")); // Negate0D
        Assert.True(await Helpers.CheckBool("-(-0.0) = 0.0")); // NegateNeg0D
        Assert.True(await Helpers.CheckBool("-(1.0) = -1.0")); // Negate1D
        Assert.True(await Helpers.CheckBool("-(-1.0) = 1.0")); // NegateNeg1D
        Assert.True(await Helpers.CheckBool("-(1'cm') = -1.0'cm'")); // Negate1CM
    }

    [Fact]
    public async Task Precision()
    {
        Assert.True(await Helpers.CheckBool("Precision(1.58700) = 5")); // PrecisionDecimal
        Assert.True(await Helpers.CheckBool("Precision(@2014) = 4")); // PrecisionYear
        Assert.True(await Helpers.CheckBool("Precision(@2014-01-05T10:30:00.000) = 17")); // PrecisionDateTimeMilliseconds
        Assert.True(await Helpers.CheckBool("Precision(@T10:30) = 4")); // PrecisionTimeMinutes
        Assert.True(await Helpers.CheckBool("Precision(@T10:30:00.000) = 9")); // PrecisionTimeMilliseconds
    }

    [Fact]
    public async Task Predecessor()
    {
        Assert.True(await Helpers.CheckBool("(predecessor of (null as Integer)) is null")); // PredecessorNull
        Assert.True(await Helpers.CheckBool("predecessor of 0 = -1")); // PredecessorOf0
        Assert.True(await Helpers.CheckBool("predecessor of 1 = 0")); // PredecessorOf1
        Assert.True(await Helpers.CheckBool("predecessor of 1L = 0L")); // PredecessorOf1L
        Assert.True(await Helpers.CheckBool("predecessor of 1.0 = 0.99999999")); // PredecessorOf1D
        Assert.True(await Helpers.CheckBool("predecessor of 1.01 = 1.00999999")); // PredecessorOf101D
        Assert.True(await Helpers.CheckBool("predecessor of 1.0 'cm' = 0.99999999'cm'")); // PredecessorOf1QCM
        Assert.True(await Helpers.CheckBool("predecessor of DateTime(2000,1,1) = @1999-12-31T")); // PredecessorOfJan12000
        Assert.True(await Helpers.CheckBool("predecessor of @T12:00:00.000 = @T11:59:59.999")); // PredecessorOfNoon
        Assert.True(await Helpers.CheckBool("predecessor of DateTime(0001, 1, 1, 0, 0, 0, 0) = ")); // PredecessorUnderflowDt
        Assert.True(await Helpers.CheckBool("predecessor of @T00:00:00.000 = ")); // PredecessorUnderflowT
    }

    [Fact]
    public async Task Power()
    {
        Assert.True(await Helpers.CheckBool("(Power(null as Integer, null as Integer)) is null")); // PowerNullToNull
        Assert.True(await Helpers.CheckBool("Power(0, 0) = 1")); // Power0To0
        Assert.True(await Helpers.CheckBool("Power(2, 2) = 4")); // Power2To2
        Assert.True(await Helpers.CheckBool("Power(-2, 2) = 4")); // PowerNeg2To2
        Assert.True(await Helpers.CheckBool("Power(2, -2) = 0.25")); // Power2ToNeg2
        Assert.True(await Helpers.CheckBool("Power(2L, 2L) = 4L")); // Power2LTo2L
        Assert.True(await Helpers.CheckBool("Power(2.0, 2.0) = 4.0")); // Power2DTo2D
        Assert.True(await Helpers.CheckBool("Power(-2.0, 2.0) = 4.0")); // PowerNeg2DTo2D
        Assert.True(await Helpers.CheckBool("Power(2.0, -2.0) = 0.25")); // Power2DToNeg2D
        Assert.True(await Helpers.CheckBool("Power(2.0, 2) = 4.0")); // Power2DTo2
        Assert.True(await Helpers.CheckBool("Power(2, 2.0) = 4.0")); // Power2To2D
        Assert.True(await Helpers.CheckBool("2^4 = 16")); // Power2To4
        Assert.True(await Helpers.CheckBool("2L^3L = 8L")); // Power2LTo3L
        Assert.True(await Helpers.CheckBool("2.0^4.0 = 16.0")); // Power2DTo4D
        Assert.True(await Helpers.CheckBool("Power(2, -2) ~ 0.25")); // Power2DToNeg2DEquivalence
    }

    [Fact]
    public async Task Round()
    {
        Assert.True(await Helpers.CheckBool("(Round(null as Decimal)) is null")); // RoundNull
        Assert.True(await Helpers.CheckBool("Round(1) = 1.0")); // Round1
        Assert.True(await Helpers.CheckBool("Round(0.5) = 1.0")); // Round0D5
        Assert.True(await Helpers.CheckBool("Round(0.4) = 0.0")); // Round0D4
        Assert.True(await Helpers.CheckBool("Round(3.14159, 2) = 3.14")); // Round3D14159
        Assert.True(await Helpers.CheckBool("Round(-0.5) = 0.0")); // RoundNeg0D5
        Assert.True(await Helpers.CheckBool("Round(-0.4) = 0.0")); // RoundNeg0D4
        Assert.True(await Helpers.CheckBool("Round(-0.6) = -1.0")); // RoundNeg0D6
        Assert.True(await Helpers.CheckBool("Round(-1.1) = -1.0")); // RoundNeg1D1
        Assert.True(await Helpers.CheckBool("Round(-1.5) = -1.0")); // RoundNeg1D5
        Assert.True(await Helpers.CheckBool("Round(-1.6) = -2.0")); // RoundNeg1D6
    }

    [Fact]
    public async Task Subtract()
    {
        Assert.True(await Helpers.CheckBool("(1 - null) is null")); // SubtractNull
        Assert.True(await Helpers.CheckBool("1 - 1 = 0")); // Subtract1And1
        Assert.True(await Helpers.CheckBool("1L - 1L = 0L")); // Subtract1LAnd1L
        Assert.True(await Helpers.CheckBool("1.0 - 2.0 = -1.0")); // Subtract1DAnd2D
        Assert.True(await Helpers.CheckBool("1.0 'cm' - 2.0 'cm' = -1.0'cm'")); // Subtract1CMAnd2CM
        Assert.True(await Helpers.CheckBool("2 - 1.1 = 0.9")); // Subtract2And11D
    }

    [Fact]
    public async Task Successor()
    {
        Assert.True(await Helpers.CheckBool("(successor of (null as Integer)) is null")); // SuccessorNull
        Assert.True(await Helpers.CheckBool("successor of 0 = 1")); // SuccessorOf0
        Assert.True(await Helpers.CheckBool("successor of 1 = 2")); // SuccessorOf1
        Assert.True(await Helpers.CheckBool("successor of 1L = 2L")); // SuccessorOf1L
        Assert.True(await Helpers.CheckBool("successor of 1.0 = 1.00000001")); // SuccessorOf1D
        Assert.True(await Helpers.CheckBool("successor of 1.01 = 1.01000001")); // SuccessorOf101D
        Assert.True(await Helpers.CheckBool("successor of DateTime(2000,1,1) = @2000-01-02T")); // SuccessorOfJan12000
        Assert.True(await Helpers.CheckBool("successor of @T12:00:00.000 = @T12:00:00.001")); // SuccessorOfNoon
        Assert.True(await Helpers.CheckBool("successor of DateTime(9999, 12, 31, 23, 59, 59, 999) = ")); // SuccessorOverflowDt
        Assert.True(await Helpers.CheckBool("successor of @T23:59:59.999 = ")); // SuccessorOverflowT
    }

    [Fact]
    public async Task Truncate()
    {
        Assert.True(await Helpers.CheckBool("(Truncate(null as Decimal)) is null")); // TruncateNull
        Assert.True(await Helpers.CheckBool("Truncate(0) = 0")); // Truncate0
        Assert.True(await Helpers.CheckBool("Truncate(0.0) = 0")); // Truncate0D0
        Assert.True(await Helpers.CheckBool("Truncate(0.1) = 0")); // Truncate0D1
        Assert.True(await Helpers.CheckBool("Truncate(1) = 1")); // Truncate1
        Assert.True(await Helpers.CheckBool("Truncate(1.0) = 1")); // Truncate1D0
        Assert.True(await Helpers.CheckBool("Truncate(1.1) = 1")); // Truncate1D1
        Assert.True(await Helpers.CheckBool("Truncate(1.9) = 1")); // Truncate1D9
        Assert.True(await Helpers.CheckBool("Truncate(-1) = -1")); // TruncateNeg1
        Assert.True(await Helpers.CheckBool("Truncate(-1.0) = -1")); // TruncateNeg1D0
        Assert.True(await Helpers.CheckBool("Truncate(-1.1) = -1")); // TruncateNeg1D1
        Assert.True(await Helpers.CheckBool("Truncate(-1.9) = -1")); // TruncateNeg1D9
    }

    [Fact]
    public async Task TruncatedDivide()
    {
        Assert.True(await Helpers.CheckBool("((null as Integer) div (null as Integer)) is null")); // TruncatedDivideNull
        Assert.True(await Helpers.CheckBool("2 div 1 = 2")); // TruncatedDivide2By1
        Assert.True(await Helpers.CheckBool("(2 div 0) is null")); // TruncatedDivide2By0
        Assert.True(await Helpers.CheckBool("10 div 3 = 3")); // TruncatedDivide10By3
        Assert.True(await Helpers.CheckBool("10L div 3L = 3L")); // TruncatedDivide10LBy3L
        Assert.True(await Helpers.CheckBool("(10L div 0L) is null")); // TruncatedDivide10LBy0L
        Assert.True(await Helpers.CheckBool("10.1 div 3.1 = 3.0")); // TruncatedDivide10d1By3D1
        Assert.True(await Helpers.CheckBool("(10.1 div 0.0) is null")); // TruncatedDivide10D1By0D
        Assert.True(await Helpers.CheckBool("-2 div -1 = 2")); // TruncatedDivideNeg2ByNeg1
        Assert.True(await Helpers.CheckBool("-10 div -3 = 3")); // TruncatedDivideNeg10ByNeg3
        Assert.True(await Helpers.CheckBool("-10.1 div -3.1 = 3.0")); // TruncatedDivideNeg10d1ByNeg3D1
        Assert.True(await Helpers.CheckBool("-2 div 1 = -2")); // TruncatedDivideNeg2By1
        Assert.True(await Helpers.CheckBool("-10 div 3 = -3")); // TruncatedDivideNeg10By3
        Assert.True(await Helpers.CheckBool("-10.1 div 3.1 = -3.0")); // TruncatedDivideNeg10d1By3D1
        Assert.True(await Helpers.CheckBool("2 div -1 = -2")); // TruncatedDivide2ByNeg1
        Assert.True(await Helpers.CheckBool("10 div -3 = -3")); // TruncatedDivide10ByNeg3
        Assert.True(await Helpers.CheckBool("10.1 div -3.1 = -3.0")); // TruncatedDivide10d1ByNeg3D1
        Assert.True(await Helpers.CheckBool("10 div 5.0 = 2.0")); // TruncatedDivide10By5D
        Assert.True(await Helpers.CheckBool("10.1 'cm' div -3.1 'cm' = -3.0 'cm'")); // TruncatedDivide10d1ByNeg3D1Quantity
        Assert.True(await Helpers.CheckBool("10.0 'g' div 5.0 'g' = 2.0 'g'")); // TruncatedDivide10By5DQuantity
        Assert.True(await Helpers.CheckBool("4.14 'm' div 2.06 'm' = 2.0 'm'")); // TruncatedDivide414By206DQuantity
        Assert.True(await Helpers.CheckBool("(10.0 'g' div 0.0 'g') is null")); // TruncatedDivide10By0DQuantity
    }
}

using Xunit;

namespace Fire.Cql.Tests;

public class CqlNullologicalOperatorsTest
{
    [Fact]
    public async Task Coalesce()
    {
        Assert.True(await Helpers.CheckBool("Coalesce('a', null) = 'a'")); // CoalesceANull
        Assert.True(await Helpers.CheckBool("Coalesce(null, 'a') = 'a'")); // CoalesceNullA
        Assert.True(await Helpers.CheckBool("(Coalesce({})) is null")); // CoalesceEmptyList
        Assert.True(await Helpers.CheckBool("Coalesce({'a', null, null}) = 'a'")); // CoalesceListFirstA
        Assert.True(await Helpers.CheckBool("Coalesce({null, null, 'a'}) = 'a'")); // CoalesceListLastA
        Assert.True(await Helpers.CheckBool("Coalesce({'a'},null, null) = {'a'}")); // CoalesceFirstList
        Assert.True(await Helpers.CheckBool("Coalesce(null, null, {'a'}) = {'a'}")); // CoalesceLastList
        Assert.True(await Helpers.CheckBool("Coalesce(null, null, DateTime(2012, 5, 18)) = @2012-05-18T")); // DateTimeCoalesce
        Assert.True(await Helpers.CheckBool("Coalesce({ null, null, DateTime(2012, 5, 18) }) = @2012-05-18T")); // DateTimeListCoalesce
        Assert.True(await Helpers.CheckBool("Coalesce(null, null, @T05:15:33.556) = @T05:15:33.556")); // TimeCoalesce
        Assert.True(await Helpers.CheckBool("Coalesce({ null, null, @T05:15:33.556 }) = @T05:15:33.556")); // TimeListCoalesce
    }

    [Fact]
    public async Task IsNull()
    {
        Assert.True(await Helpers.CheckBool("IsNull(null)")); // IsNullTrue
        Assert.False(await Helpers.CheckBool("IsNull('')")); // IsNullFalseEmptyString
        Assert.False(await Helpers.CheckBool("IsNull('abc')")); // IsNullAlsoFalseAbcString
        Assert.False(await Helpers.CheckBool("IsNull(1)")); // IsNullAlsoFalseNumber1
        Assert.False(await Helpers.CheckBool("IsNull(0)")); // IsNullAlsoFalseNumberZero
    }

    [Fact]
    public async Task IsFalse()
    {
        Assert.True(await Helpers.CheckBool("IsFalse(false)")); // IsFalseFalse
        Assert.False(await Helpers.CheckBool("IsFalse(true)")); // IsFalseTrue
        Assert.False(await Helpers.CheckBool("IsFalse(null)")); // IsFalseNull
    }

    [Fact]
    public async Task IsTrue()
    {
        Assert.True(await Helpers.CheckBool("IsTrue(true)")); // IsTrueTrue
        Assert.False(await Helpers.CheckBool("IsTrue(false)")); // IsTrueFalse
        Assert.False(await Helpers.CheckBool("IsTrue(null)")); // IsTrueNull
    }
}

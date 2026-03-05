using Xunit;

namespace Fire.Cql.Tests;

public class CqlLogicalOperatorsTest
{
    [Fact]
    public async Task And()
    {
        Assert.True(await Helpers.CheckBool("true and true")); // TrueAndTrue
        Assert.False(await Helpers.CheckBool("true and false")); // TrueAndFalse
        Assert.True(await Helpers.CheckBool("(true and null) is null")); // TrueAndNull
        Assert.False(await Helpers.CheckBool("false and true")); // FalseAndTrue
        Assert.False(await Helpers.CheckBool("false and false")); // FalseAndFalse
        Assert.False(await Helpers.CheckBool("false and null")); // FalseAndNull
        Assert.True(await Helpers.CheckBool("(null and true) is null")); // NullAndTrue
        Assert.False(await Helpers.CheckBool("null and false")); // NullAndFalse
        Assert.True(await Helpers.CheckBool("(null and null) is null")); // NullAndNull
    }

    [Fact]
    public async Task Implies()
    {
        Assert.True(await Helpers.CheckBool("true implies true")); // TrueImpliesTrue
        Assert.False(await Helpers.CheckBool("true implies false")); // TrueImpliesFalse
        Assert.True(await Helpers.CheckBool("(true implies null) is null")); // TrueImpliesNull
        Assert.True(await Helpers.CheckBool("false implies true")); // FalseImpliesTrue
        Assert.True(await Helpers.CheckBool("false implies false")); // FalseImpliesFalse
        Assert.True(await Helpers.CheckBool("false implies null")); // FalseImpliesNull
        Assert.True(await Helpers.CheckBool("null implies true")); // NullImpliesTrue
        Assert.True(await Helpers.CheckBool("(null implies false) is null")); // NullImpliesFalse
        Assert.True(await Helpers.CheckBool("(null implies null) is null")); // NullImpliesNull
    }

    [Fact]
    public async Task Not()
    {
        Assert.False(await Helpers.CheckBool("not true")); // NotTrue
        Assert.True(await Helpers.CheckBool("not false")); // NotFalse
        Assert.True(await Helpers.CheckBool("(not null) is null")); // NotNull
    }

    [Fact]
    public async Task Or()
    {
        Assert.True(await Helpers.CheckBool("true or true")); // TrueOrTrue
        Assert.True(await Helpers.CheckBool("true or false")); // TrueOrFalse
        Assert.True(await Helpers.CheckBool("true or null")); // TrueOrNull
        Assert.True(await Helpers.CheckBool("false or true")); // FalseOrTrue
        Assert.False(await Helpers.CheckBool("false or false")); // FalseOrFalse
        Assert.True(await Helpers.CheckBool("(false or null) is null")); // FalseOrNull
        Assert.True(await Helpers.CheckBool("null or true")); // NullOrTrue
        Assert.True(await Helpers.CheckBool("(null or false) is null")); // NullOrFalse
        Assert.True(await Helpers.CheckBool("(null or null) is null")); // NullOrNull
    }

    [Fact]
    public async Task Xor()
    {
        Assert.False(await Helpers.CheckBool("true xor true")); // TrueXorTrue
        Assert.True(await Helpers.CheckBool("true xor false")); // TrueXorFalse
        Assert.True(await Helpers.CheckBool("(true xor null) is null")); // TrueXorNull
        Assert.True(await Helpers.CheckBool("false xor true")); // FalseXorTrue
        Assert.False(await Helpers.CheckBool("false xor false")); // FalseXorFalse
        Assert.True(await Helpers.CheckBool("(false xor null) is null")); // FalseXorNull
        Assert.True(await Helpers.CheckBool("(null xor true) is null")); // NullXorTrue
        Assert.True(await Helpers.CheckBool("(null xor false) is null")); // NullXorFalse
        Assert.True(await Helpers.CheckBool("(null xor null) is null")); // NullXorNull
    }
}

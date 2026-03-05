using Xunit;

namespace Fire.Cql.Tests;

public class CqlStringOperatorsTest
{
    [Fact]
    public async Task Combine()
    {
        Assert.True(await Helpers.CheckBool("(Combine(null)) is null")); // CombineNull
        Assert.True(await Helpers.CheckBool("(Combine({})) is null")); // CombineEmptyList
        Assert.True(await Helpers.CheckBool("Combine({'a', 'b', 'c'}) = 'abc'")); // CombineABC
        Assert.True(await Helpers.CheckBool("Combine({'a', 'b', 'c'}, '-') = 'a-b-c'")); // CombineABCSepDash
    }

    [Fact]
    public async Task Concatenate()
    {
        Assert.True(await Helpers.CheckBool("(Concatenate(null, null)) is null")); // ConcatenateNullNull
        Assert.True(await Helpers.CheckBool("(Concatenate('a', null)) is null")); // ConcatenateANull
        Assert.True(await Helpers.CheckBool("(Concatenate(null, 'b')) is null")); // ConcatenateNullB
        Assert.True(await Helpers.CheckBool("Concatenate('a', 'b') = 'ab'")); // ConcatenateAB
        Assert.True(await Helpers.CheckBool("'a' + 'b' = 'ab'")); // ConcatenateABWithAdd
    }

    [Fact]
    public async Task EndsWith()
    {
        Assert.True(await Helpers.CheckBool("(EndsWith(null, null)) is null")); // EndsWithNull
        Assert.True(await Helpers.CheckBool("EndsWith('Chris Schuler is the man!!', 'n!!')")); // EndsWithTrue
        Assert.False(await Helpers.CheckBool("EndsWith('Chris Schuler is the man!!', 'n!')")); // EndsWithFalse
    }

    [Fact]
    public async Task Indexer()
    {
        Assert.True(await Helpers.CheckBool("(Indexer(null as String, null)) is null")); // IndexerNullNull
        Assert.True(await Helpers.CheckBool("(Indexer('a', null)) is null")); // IndexerANull
        Assert.True(await Helpers.CheckBool("(Indexer(null as String, 1)) is null")); // IndexerNull1String
        Assert.True(await Helpers.CheckBool("Indexer('ab', 0) = 'a'")); // IndexerAB0
        Assert.True(await Helpers.CheckBool("Indexer('ab', 1) = 'b'")); // IndexerAB1
        Assert.True(await Helpers.CheckBool("(Indexer('ab', 2)) is null")); // IndexerAB2
        Assert.True(await Helpers.CheckBool("(Indexer('ab', -1)) is null")); // IndexerABNeg1
    }

    [Fact]
    public async Task LastPositionOf()
    {
        Assert.True(await Helpers.CheckBool("(LastPositionOf(null, null)) is null")); // LastPositionOfNull
        Assert.True(await Helpers.CheckBool("(LastPositionOf(null, 'hi')) is null")); // LastPositionOfNull1
        Assert.True(await Helpers.CheckBool("(LastPositionOf('hi', null)) is null")); // LastPositionOfNull2
        Assert.True(await Helpers.CheckBool("LastPositionOf('hi', 'Ohio is the place to be!') = 1")); // LastPositionOf1
        Assert.True(await Helpers.CheckBool("LastPositionOf('hi', 'Say hi to Ohio!') = 11")); // LastPositionOf2
    }

    [Fact]
    public async Task Length()
    {
        Assert.True(await Helpers.CheckBool("(Length(null as String)) is null")); // LengthNullString
        Assert.True(await Helpers.CheckBool("Length('') = 0")); // LengthEmptyString
        Assert.True(await Helpers.CheckBool("Length('a') = 1")); // LengthA
        Assert.True(await Helpers.CheckBool("Length('ab') = 2")); // LengthAB
    }

    [Fact]
    public async Task Lower()
    {
        Assert.True(await Helpers.CheckBool("(Lower(null)) is null")); // LowerNull
        Assert.True(await Helpers.CheckBool("Lower('') = ''")); // LowerEmpty
        Assert.True(await Helpers.CheckBool("Lower('A') = 'a'")); // LowerA
        Assert.True(await Helpers.CheckBool("Lower('b') = 'b'")); // LowerB
        Assert.True(await Helpers.CheckBool("Lower('Ab') = 'ab'")); // LowerAB
    }

    [Fact]
    public async Task Matches()
    {
        Assert.True(await Helpers.CheckBool("(Matches('Not all who wander are lost', null)) is null")); // MatchesNull
        Assert.False(await Helpers.CheckBool("Matches('Not all who wander are lost', '.*\\\\d+')")); // MatchesNumberFalse
        Assert.True(await Helpers.CheckBool("Matches('Not all who wander are lost - circa 2017', '.*\\\\d+')")); // MatchesNumberTrue
        Assert.True(await Helpers.CheckBool("Matches('Not all who wander are lost', '.*')")); // MatchesAllTrue
        Assert.True(await Helpers.CheckBool("Matches('Not all who wander are lost', '[\\\\w|\\\\s]+')")); // MatchesWordsAndSpacesTrue
        Assert.False(await Helpers.CheckBool("Matches('Not all who wander are lost - circa 2017', '^[\\\\w\\\\s]+$')")); // MatchesWordsAndSpacesFalse
        Assert.True(await Helpers.CheckBool("Matches(' ', '\\\\W+')")); // MatchesNotWords
        Assert.True(await Helpers.CheckBool("Matches(' \\n\\t', '\\\\s+')")); // MatchesWhiteSpace
    }

    [Fact]
    public async Task PositionOf()
    {
        Assert.True(await Helpers.CheckBool("(PositionOf(null, null)) is null")); // PositionOfNullNull
        Assert.True(await Helpers.CheckBool("(PositionOf('a', null)) is null")); // PositionOfANull
        Assert.True(await Helpers.CheckBool("(PositionOf(null, 'a')) is null")); // PositionOfNullA
        Assert.True(await Helpers.CheckBool("PositionOf('a', 'ab') = 0")); // PositionOfAInAB
        Assert.True(await Helpers.CheckBool("PositionOf('b', 'ab') = 1")); // PositionOfBInAB
        Assert.True(await Helpers.CheckBool("PositionOf('c', 'ab') = -1")); // PositionOfCInAB
    }

    [Fact]
    public async Task ReplaceMatches()
    {
        Assert.True(await Helpers.CheckBool("(ReplaceMatches('Not all who wander are lost', null, 'But I am...')) is null")); // ReplaceMatchesNull
        Assert.True(await Helpers.CheckBool("ReplaceMatches('Not all who wander are lost', 'Not all who wander are lost', 'But still waters run deep') = 'But still waters run deep'")); // ReplaceMatchesAll
        Assert.True(await Helpers.CheckBool("ReplaceMatches('Who put the bop in the bop she bop she bop?', 'bop', 'bang') = 'Who put the bang in the bang she bang she bang?'")); // ReplaceMatchesMany
        Assert.True(await Helpers.CheckBool("ReplaceMatches('All that glitters is not gold', '\\\\s', '\\\\$') = 'All$that$glitters$is$not$gold'")); // ReplaceMatchesSpaces
    }

    [Fact]
    public async Task Split()
    {
        Assert.True(await Helpers.CheckBool("(Split(null, null)) is null")); // SplitNullNull
        Assert.True(await Helpers.CheckBool("(Split(null, ',')) is null")); // SplitNullComma
        Assert.True(await Helpers.CheckBool("Split('a,b', null) = {'a,b'}")); // SplitABNull
        Assert.True(await Helpers.CheckBool("Split('a,b', '-') = {'a,b'}")); // SplitABDash
        Assert.True(await Helpers.CheckBool("Split('a,b', ',') = {'a','b'}")); // SplitABComma
    }

    [Fact]
    public async Task StartsWith()
    {
        Assert.True(await Helpers.CheckBool("(StartsWith(null, null)) is null")); // StartsWithNull
        Assert.True(await Helpers.CheckBool("(StartsWith('hi', null)) is null")); // StartsWithNull1
        Assert.True(await Helpers.CheckBool("(StartsWith(null, 'hi')) is null")); // StartsWithNull2
        Assert.True(await Helpers.CheckBool("StartsWith('Breathe deep the gathering gloom', 'Bre')")); // StartsWithTrue1
        Assert.False(await Helpers.CheckBool("StartsWith('Breathe deep the gathering gloom', 'bre')")); // StartsWithFalse1
    }

    [Fact]
    public async Task Substring()
    {
        Assert.True(await Helpers.CheckBool("(Substring(null, null)) is null")); // SubstringNullNull
        Assert.True(await Helpers.CheckBool("(Substring('a', null)) is null")); // SubstringANull
        Assert.True(await Helpers.CheckBool("(Substring(null, 1)) is null")); // SubstringNull1
        Assert.True(await Helpers.CheckBool("Substring('ab', 0) = 'ab'")); // SubstringAB0
        Assert.True(await Helpers.CheckBool("Substring('ab', 1) = 'b'")); // SubstringAB1
        Assert.True(await Helpers.CheckBool("(Substring('ab', 2)) is null")); // SubstringAB2
        Assert.True(await Helpers.CheckBool("(Substring('ab', -1)) is null")); // SubstringABNeg1
        Assert.True(await Helpers.CheckBool("Substring('ab', 0, 1) = 'a'")); // SubstringAB0To1
        Assert.True(await Helpers.CheckBool("Substring('abc', 1, 1) = 'b'")); // SubstringABC1To1
        Assert.True(await Helpers.CheckBool("Substring('ab', 0, 3) = 'ab'")); // SubstringAB0To3
    }

    [Fact]
    public async Task Upper()
    {
        Assert.True(await Helpers.CheckBool("(Upper(null)) is null")); // UpperNull
        Assert.True(await Helpers.CheckBool("Upper('') = ''")); // UpperEmpty
        Assert.True(await Helpers.CheckBool("Upper('a') = 'A'")); // UpperA
        Assert.True(await Helpers.CheckBool("Upper('B') = 'B'")); // UpperB
        Assert.True(await Helpers.CheckBool("Upper('aB') = 'AB'")); // UpperAB
    }

    [Fact]
    public async Task toStringtests()
    {
        Assert.True(await Helpers.CheckBool("ToString(125 'cm') = '125 \\'cm\\''")); // QuantityToString
        Assert.True(await Helpers.CheckBool("ToString(DateTime(2000, 1, 1)) = '2000-01-01'")); // DateTimeToString1
        Assert.True(await Helpers.CheckBool("ToString(DateTime(2000, 1, 1, 15, 25, 25, 300)) = '2000-01-01T15:25:25.300'")); // DateTimeToString2
        Assert.True(await Helpers.CheckBool("ToString(DateTime(2000, 1, 1, 8, 25, 25, 300, -7)) = '2000-01-01T08:25:25.300-07:00'")); // DateTimeToString3
        Assert.True(await Helpers.CheckBool("ToString(@T09:30:01.003) = '09:30:01.003'")); // TimeToString1
    }
}

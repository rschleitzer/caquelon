using Xunit;

namespace Fire.Cql.Tests;

public class CqlListOperatorsTest
{
    [Fact]
    public async Task Sort()
    {
        Assert.True(await Helpers.CheckBool("({4, 5, 1, 6, 2, 1}) sL sort asc = {1, 1, 2, 4, 5, 6}")); // simpleSortAsc
        Assert.True(await Helpers.CheckBool("({4, 5, 1, 6, 2, 1}) sL sort desc = {6, 5, 4, 2, 1, 1}")); // simpleSortDesc
        Assert.True(await Helpers.CheckBool("({'back', 'aardvark', 'alligator', 'zebra', 'iguana', 'Wolf', 'Armadillo'}) sls sort asc = {'Armadillo', 'Wolf', 'aardvark', 'alligator', 'back', 'iguana', 'zebra'}")); // simpleSortStringAsc
        Assert.True(await Helpers.CheckBool("({'back', 'aardvark', 'alligator', 'zebra', 'iguana', 'Wolf', 'Armadillo'}) sls sort desc = {'zebra', 'iguana', 'back', 'alligator', 'aardvark', 'Wolf', 'Armadillo'}")); // simpleSortStringDesc
        Assert.True(await Helpers.CheckBool("({ DateTime(2012, 10, 5, 10), DateTime(2012, 1, 1), DateTime(2012, 1, 1, 12), DateTime(2012, 10, 5) }) S sort asc = { @2012-01-01T, @2012-01-01T12, @2012-10-05T, @2012-10-05T10 }")); // SortDatesAsc
        Assert.True(await Helpers.CheckBool("({ DateTime(2012, 10, 5, 10), DateTime(2012, 1, 1), DateTime(2012, 1, 1, 12), DateTime(2012, 10, 5) }) S sort desc = { @2012-10-05T10, @2012-10-05T, @2012-01-01T12, @2012-01-01T }")); // SortDatesDesc
        Assert.True(await Helpers.CheckBool("{ 3, 2, 1 } = {3, 2, 1}")); // intList
        Assert.True(await Helpers.CheckBool("{ 3.8, 2.4, 1.9 } = {3.8, 2.4, 1.9}")); // decimalList
        Assert.True(await Helpers.CheckBool("{ 19.99 '[lb_av]', 17.33 '[lb_av]', 10.66 '[lb_av]' } = {19.99 '[lb_av]', 17.33 '[lb_av]', 10.66 '[lb_av]'}")); // quantityList
        Assert.True(await Helpers.CheckBool("{ DateTime(2016), DateTime(2015), DateTime(2010) } = {@2016T, @2015T, @2010T}")); // dateTimeList
        Assert.True(await Helpers.CheckBool("{ @T15:59:59.999, @T15:12:59.999, @T15:12:13.999 } = {@T15:59:59.999, @T15:12:59.999, @T15:12:13.999}")); // timeList
    }

    [Fact]
    public async Task Contains()
    {
        Assert.True(await Helpers.CheckBool("{ 'a', 'b', null } contains null")); // ContainsABNullHasNull
        Assert.False(await Helpers.CheckBool("{ null, 'b', 'c' } contains 'a'")); // ContainsNullFirst
        Assert.True(await Helpers.CheckBool("{ 'a', 'b', 'c' } contains 'a'")); // ContainsABCHasA
        Assert.True(await Helpers.CheckBool("{ DateTime(2012, 10, 5), DateTime(2012, 9, 5), DateTime(2012, 1, 1) } contains DateTime(2012, 1, 1)")); // ContainsJan2012True
        Assert.False(await Helpers.CheckBool("{ DateTime(2012, 10, 5), DateTime(2012, 9, 5), DateTime(2012, 10, 1) } contains DateTime(2012, 1, 1)")); // ContainsJan2012False
        Assert.True(await Helpers.CheckBool("{ @T15:59:59.999, @T05:59:59.999, @T20:59:59.999 } contains @T05:59:59.999")); // ContainsTimeTrue
        Assert.False(await Helpers.CheckBool("{ @T15:59:59.999, @T05:59:59.999, @T20:59:59.999 } contains @T08:59:59.999")); // ContainsTimeFalse
        Assert.False(await Helpers.CheckBool("null contains 'a'")); // ContainsNullLeft
    }

    [Fact]
    public async Task Descendents()
    {
        Assert.True(await Helpers.CheckBool("((null).descendents()) is null")); // DescendentsEmptyList
    }

    [Fact]
    public async Task Distinct()
    {
        Assert.True(await Helpers.CheckBool("distinct {} = {}")); // DistinctEmptyList
        Assert.True(await Helpers.CheckBool("distinct { null, null, null} = { null }")); // DistinctNullNullNull
        Assert.True(await Helpers.CheckBool("distinct { 'a', null, 'a', null} = {'a', null}")); // DistinctANullANull
        Assert.True(await Helpers.CheckBool("distinct { 1, 1, 2, 2, 3, 3} = {1,2,3}")); // Distinct112233
        Assert.True(await Helpers.CheckBool("distinct { 1, 2, 3, 1, 2, 3} = {1,2,3}")); // Distinct123123
        Assert.True(await Helpers.CheckBool("distinct { 'a', 'a', 'b', 'b', 'c', 'c'} = {'a','b','c'}")); // DistinctAABBCC
        Assert.True(await Helpers.CheckBool("distinct { 'a', 'b', 'c', 'a', 'b', 'c'} = {'a','b','c'}")); // DistinctABCABC
        Assert.True(await Helpers.CheckBool("distinct { DateTime(2012, 10, 5), DateTime(2012, 1, 1), DateTime(2012, 1, 1)} = { @2012-10-05T, @2012-01-01T }")); // DistinctDateTime
        Assert.True(await Helpers.CheckBool("distinct { @T15:59:59.999, @T20:59:59.999 } = { @T15:59:59.999, @T20:59:59.999 }")); // DistinctTime
    }

    [Fact]
    public async Task Equal()
    {
        Assert.True(await Helpers.CheckBool("{null} = {null}")); // EqualNullNull
        Assert.True(await Helpers.CheckBool("({} as List<String> = null) is null")); // EqualEmptyListNull
        Assert.True(await Helpers.CheckBool("(null = {} as List<String>) is null")); // EqualNullEmptyList
        Assert.True(await Helpers.CheckBool("{} = {}")); // EqualEmptyListAndEmptyList
        Assert.True(await Helpers.CheckBool("{ 'a', 'b', 'c' } = { 'a', 'b', 'c' }")); // EqualABCAndABC
        Assert.False(await Helpers.CheckBool("{ 'a', 'b', 'c' } = { 'a', 'b' }")); // EqualABCAndAB
        Assert.False(await Helpers.CheckBool("{ 'a', 'b', 'c' } = { 1, 2, 3 }")); // EqualABCAnd123
        Assert.False(await Helpers.CheckBool("{ 1, 2, 3 } = { 'a', 'b', 'c' }")); // Equal123AndABC
        Assert.False(await Helpers.CheckBool("{ 1, 2, 3 } = { '1', '2', '3' }")); // Equal123AndString123
        Assert.False(await Helpers.CheckBool("{ 1, 2 } = { 1, 2, 3 }")); // Equal12And123
        Assert.False(await Helpers.CheckBool("{ 1, 2, 3 } = { 1, 2 }")); // Equal123And12
        Assert.True(await Helpers.CheckBool("{ 1, 2, 3 } = { 1, 2, 3 }")); // Equal123And123
        Assert.True(await Helpers.CheckBool("{DateTime(2012, 5, 10, 0, 0, 0, 0), DateTime(2014, 12, 10, 0, 0, 0, 0)} = {DateTime(2012, 5, 10, 0, 0, 0, 0), DateTime(2014, 12, 10, 0, 0, 0, 0)}")); // EqualDateTimeTrue
        Assert.False(await Helpers.CheckBool("{DateTime(2012, 5, 10, 0, 0, 0, 0), DateTime(2014, 12, 10, 0, 0, 0, 0)} = {DateTime(2012, 1, 10, 0, 0, 0, 0), DateTime(2014, 12, 10, 0, 0, 0, 0)}")); // EqualDateTimeFalse
        Assert.True(await Helpers.CheckBool("{ @T15:59:59.999, @T20:59:59.999, @T20:59:59.999 } = { @T15:59:59.999, @T20:59:59.999, @T20:59:59.999 }")); // EqualTimeTrue
        Assert.False(await Helpers.CheckBool("{ @T15:59:59.999, @T20:59:59.999, @T20:59:59.999 } = { @T10:59:59.999, @T20:59:59.999, @T20:59:59.999 }")); // EqualTimeFalse
    }

    [Fact]
    public async Task Except()
    {
        Assert.True(await Helpers.CheckBool("{} except {} = {}")); // ExceptEmptyListAndEmptyList
        Assert.True(await Helpers.CheckBool("{ 1, 2, 3, 4 } except { 2, 3 } = { 1, 4 }")); // Except1234And23
        Assert.True(await Helpers.CheckBool("{ 2, 3 } except { 1, 2, 3, 4 } = {}")); // Except23And1234
        Assert.True(await Helpers.CheckBool("{ DateTime(2012, 5, 10), DateTime(2014, 12, 10), DateTime(2010, 1, 1)} except {DateTime(2014, 12, 10), DateTime(2010, 1, 1) } = {@2012-05-10T}")); // ExceptDateTimeList
        Assert.True(await Helpers.CheckBool("{ @T15:59:59.999, @T20:59:59.999, @T12:59:59.999 } except { @T20:59:59.999, @T12:59:59.999 } = {@T15:59:59.999}")); // ExceptTimeList
        Assert.True(await Helpers.CheckBool("{ 1, 4 } except null = {1, 4}")); // ExceptNullRight
    }

    [Fact]
    public async Task Exists()
    {
        Assert.False(await Helpers.CheckBool("Exists({})")); // ExistsEmpty
        Assert.False(await Helpers.CheckBool("Exists({ null })")); // ExistsListNull
        Assert.True(await Helpers.CheckBool("Exists({ 1 })")); // Exists1
        Assert.True(await Helpers.CheckBool("Exists({ 1, 2 })")); // Exists12
        Assert.True(await Helpers.CheckBool("Exists({ DateTime(2012, 5, 10), DateTime(2014, 12, 10) })")); // ExistsDateTime
        Assert.True(await Helpers.CheckBool("Exists({ @T15:59:59.999, @T20:59:59.999 })")); // ExistsTime
        Assert.False(await Helpers.CheckBool("Exists(null)")); // ExistsNull
    }

    [Fact]
    public async Task Flatten()
    {
        Assert.True(await Helpers.CheckBool("Flatten({{},{}}) = {}")); // FlattenEmpty
        Assert.True(await Helpers.CheckBool("Flatten({{null}, {null}}) = {null, null}")); // FlattenListNullAndNull
        Assert.True(await Helpers.CheckBool("Flatten({{1,2}, {3,4}}) = {1,2,3,4}")); // FlattenList12And34
        Assert.True(await Helpers.CheckBool("Flatten({ {DateTime(2012, 5, 10)}, {DateTime(2014, 12, 10)} }) = { @2012-05-10T, @2014-12-10T }")); // FlattenDateTime
        Assert.True(await Helpers.CheckBool("Flatten({ {@T15:59:59.999}, {@T20:59:59.999} }) = { @T15:59:59.999, @T20:59:59.999 }")); // FlattenTime
    }

    [Fact]
    public async Task First()
    {
        Assert.True(await Helpers.CheckBool("(First({})) is null")); // FirstEmpty
        Assert.True(await Helpers.CheckBool("(First({ null, 1 })) is null")); // FirstNull1
        Assert.True(await Helpers.CheckBool("First({ 1, null }) = 1")); // First1Null
        Assert.True(await Helpers.CheckBool("First({ 1, 2 }) = 1")); // First12
        Assert.True(await Helpers.CheckBool("First({ DateTime(2012, 5, 10), DateTime(2014, 12, 10) }) = @2012-05-10T")); // FirstDateTime
        Assert.True(await Helpers.CheckBool("First({ @T15:59:59.999, @T20:59:59.999 }) = @T15:59:59.999")); // FirstTime
    }

    [Fact]
    public async Task In()
    {
        Assert.False(await Helpers.CheckBool("null in {}")); // InNullEmpty
        Assert.True(await Helpers.CheckBool("null in { 1, null }")); // InNullAnd1Null
        Assert.False(await Helpers.CheckBool("1 in null")); // In1Null
        Assert.True(await Helpers.CheckBool("1 in { 1, 2 }")); // In1And12
        Assert.False(await Helpers.CheckBool("3 in { 1, 2 }")); // In3And12
        Assert.True(await Helpers.CheckBool("DateTime(2012, 5, 10) in { DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10) }")); // InDateTimeTrue
        Assert.False(await Helpers.CheckBool("DateTime(2012, 6, 10) in { DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10) }")); // InDateTimeFalse
        Assert.True(await Helpers.CheckBool("@T15:59:59.999 in { @T02:29:15.156, @T15:59:59.999, @T20:59:59.999 }")); // InTimeTrue
        Assert.False(await Helpers.CheckBool("@T16:59:59.999 in { @T02:29:15.156, @T15:59:59.999, @T20:59:59.999 }")); // InTimeFalse
    }

    [Fact]
    public async Task Includes()
    {
        Assert.True(await Helpers.CheckBool("{} includes {}")); // IncludesEmptyAndEmpty
        Assert.True(await Helpers.CheckBool("{null} includes {null}")); // IncludesListNullAndListNull
        Assert.True(await Helpers.CheckBool("{1, 2, 3} includes {}")); // Includes123AndEmpty
        Assert.True(await Helpers.CheckBool("{1, 2, 3} includes {2}")); // Includes123And2
        Assert.False(await Helpers.CheckBool("{1, 2, 3} includes {4}")); // Includes123And4
        Assert.True(await Helpers.CheckBool("{DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10)} includes {DateTime(2012, 5, 10)}")); // IncludesDateTimeTrue
        Assert.False(await Helpers.CheckBool("{DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10)} includes {DateTime(2012, 5, 11)}")); // IncludesDateTimeFalse
        Assert.True(await Helpers.CheckBool("{ @T02:29:15.156, @T15:59:59.999, @T20:59:59.999 } includes @T15:59:59.999")); // IncludesTimeTrue
        Assert.False(await Helpers.CheckBool("{ @T02:29:15.156, @T15:59:59.999, @T20:59:59.999 } includes @T16:59:59.999")); // IncludesTimeFalse
        Assert.True(await Helpers.CheckBool("(null includes {2}) is null")); // IncludesNullLeft
        Assert.True(await Helpers.CheckBool("({'s', 'a', 'm'} includes null) is null")); // IncludesNullRight
    }

    [Fact]
    public async Task IncludedIn()
    {
        Assert.True(await Helpers.CheckBool("{} included in {}")); // IncludedInEmptyAndEmpty
        Assert.True(await Helpers.CheckBool("{ null } included in { null }")); // IncludedInListNullAndListNull
        Assert.True(await Helpers.CheckBool("{} included in { 1, 2, 3 }")); // IncludedInEmptyAnd123
        Assert.True(await Helpers.CheckBool("{ 2 } included in { 1, 2, 3 }")); // IncludedIn2And123
        Assert.False(await Helpers.CheckBool("{ 4 } included in { 1, 2, 3 }")); // IncludedIn4And123
        Assert.True(await Helpers.CheckBool("{ DateTime(2012, 5, 10)} included in {DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10)}")); // IncludedInDateTimeTrue
        Assert.False(await Helpers.CheckBool("{DateTime(2012, 5, 11)} included in {DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10)}")); // IncludedInDateTimeFalse
        Assert.True(await Helpers.CheckBool("@T15:59:59.999 included in { @T02:29:15.156, @T15:59:59.999, @T20:59:59.999 }")); // IncludedInTimeTrue
        Assert.False(await Helpers.CheckBool("@T16:59:59.999 included in { @T02:29:15.156, @T15:59:59.999, @T20:59:59.999 }")); // IncludedInTimeFalse
        Assert.True(await Helpers.CheckBool("(null included in {2}) is null")); // IncludedInNullLeft
        Assert.True(await Helpers.CheckBool("({'s', 'a', 'm'} included in null) is null")); // IncludedInNullRight
    }

    [Fact]
    public async Task Indexer()
    {
        Assert.True(await Helpers.CheckBool("((null as List<System.Any>)[1]) is null")); // IndexerNull1List
        Assert.True(await Helpers.CheckBool("{ 1, 2 }[0] = 1")); // Indexer0Of12
        Assert.True(await Helpers.CheckBool("{ 1, 2 }[1] = 2")); // Indexer1Of12
        Assert.True(await Helpers.CheckBool("({ 1, 2 }[2]) is null")); // Indexer2Of12
        Assert.True(await Helpers.CheckBool("({ 1, 2 }[-1]) is null")); // IndexerNeg1Of12
        Assert.True(await Helpers.CheckBool("{ DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10) }[1] = @2012-05-10T")); // IndexerDateTime
        Assert.True(await Helpers.CheckBool("{ @T02:29:15.156, @T15:59:59.999, @T20:59:59.999 }[1] = @T15:59:59.999")); // IndexerTime
    }

    [Fact]
    public async Task IndexOf()
    {
        Assert.True(await Helpers.CheckBool("(IndexOf({}, null)) is null")); // IndexOfEmptyNull
        Assert.True(await Helpers.CheckBool("(IndexOf(null, {})) is null")); // IndexOfNullEmpty
        Assert.True(await Helpers.CheckBool("(IndexOf({ 1, null }, null)) is null")); // IndexOfNullIn1Null
        Assert.True(await Helpers.CheckBool("IndexOf({ 1, 2 }, 1) = 0")); // IndexOf1In12
        Assert.True(await Helpers.CheckBool("IndexOf({ 1, 2 }, 2) = 1")); // IndexOf2In12
        Assert.True(await Helpers.CheckBool("IndexOf({ 1, 2 }, 3) = -1")); // IndexOf3In12
        Assert.True(await Helpers.CheckBool("IndexOf({ DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10) }, DateTime(2014, 12, 10)) = 2")); // IndexOfDateTime
        Assert.True(await Helpers.CheckBool("IndexOf({ @T02:29:15.156, @T15:59:59.999, @T20:59:59.999 }, @T15:59:59.999) = 1")); // IndexOfTime
    }

    [Fact]
    public async Task Intersect()
    {
        Assert.True(await Helpers.CheckBool("{} intersect {} = {}")); // IntersectEmptyListAndEmptyList
        Assert.True(await Helpers.CheckBool("{ 1, 2, 3, 4 } intersect { 2, 3 } = { 2, 3 }")); // Intersect1234And23
        Assert.True(await Helpers.CheckBool("{2, 3} intersect { 1, 2, 3, 4 } = { 2, 3 }")); // Intersect23And1234
        Assert.True(await Helpers.CheckBool("{ DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10) } intersect { DateTime(2012, 5, 10), DateTime(2014, 12, 10), DateTime(2000, 5, 5) } = {@2012-05-10T, @2014-12-10T}")); // IntersectDateTime
        Assert.True(await Helpers.CheckBool("{ @T02:29:15.156, @T15:59:59.999, @T20:59:59.999 } intersect { @T01:29:15.156, @T15:59:59.999, @T20:59:59.999 } = {@T15:59:59.999, @T20:59:59.999}")); // IntersectTime
    }

    [Fact]
    public async Task Last()
    {
        Assert.True(await Helpers.CheckBool("(Last({})) is null")); // LastEmpty
        Assert.True(await Helpers.CheckBool("Last({null, 1}) = 1")); // LastNull1
        Assert.True(await Helpers.CheckBool("(Last({1, null})) is null")); // Last1Null
        Assert.True(await Helpers.CheckBool("Last({1, 2}) = 2")); // Last12
        Assert.True(await Helpers.CheckBool("Last({DateTime(2012, 5, 10), DateTime(2014, 12, 10)}) = @2014-12-10T")); // LastDateTime
        Assert.True(await Helpers.CheckBool("Last({ @T15:59:59.999, @T20:59:59.999 }) = @T20:59:59.999")); // LastTime
    }

    [Fact]
    public async Task Length()
    {
        Assert.True(await Helpers.CheckBool("Length({}) = 0")); // LengthEmptyList
        Assert.True(await Helpers.CheckBool("Length({null, 1}) = 2")); // LengthNull1
        Assert.True(await Helpers.CheckBool("Length({1, null}) = 2")); // Length1Null
        Assert.True(await Helpers.CheckBool("Length({1, 2}) = 2")); // Length12
        Assert.True(await Helpers.CheckBool("Length({DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10)}) = 3")); // LengthDateTime
        Assert.True(await Helpers.CheckBool("Length({ @T15:59:59.999, @T20:59:59.999, @T15:59:59.999, @T20:59:59.999, @T15:59:59.999, @T20:59:59.999 }) = 6")); // LengthTime
        Assert.True(await Helpers.CheckBool("Length(null as List<Any>) = 0")); // LengthNullList
    }

    [Fact]
    public async Task Equivalent()
    {
        Assert.True(await Helpers.CheckBool("{} ~ {}")); // EquivalentEmptyAndEmpty
        Assert.True(await Helpers.CheckBool("{ 'a', 'b', 'c' } ~ { 'a', 'b', 'c' }")); // EquivalentABCAndABC
        Assert.False(await Helpers.CheckBool("{ 'a', 'b', 'c' } ~ { 'a', 'b' }")); // EquivalentABCAndAB
        Assert.False(await Helpers.CheckBool("{ 'a', 'b', 'c' } ~ { 1, 2, 3 }")); // EquivalentABCAnd123
        Assert.False(await Helpers.CheckBool("{ 1, 2, 3 } ~ { 'a', 'b', 'c' }")); // Equivalent123AndABC
        Assert.False(await Helpers.CheckBool("{ 1, 2, 3 } ~ { '1', '2', '3' }")); // Equivalent123AndString123
        Assert.True(await Helpers.CheckBool("{DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10), null} ~ {DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10), null}")); // EquivalentDateTimeTrue
        Assert.False(await Helpers.CheckBool("{DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10)} ~ {DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10), null}")); // EquivalentDateTimeNull
        Assert.False(await Helpers.CheckBool("{DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10)} ~ {DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 1)}")); // EquivalentDateTimeFalse
        Assert.True(await Helpers.CheckBool("{ @T15:59:59.999, @T20:59:59.999 } ~ { @T15:59:59.999, @T20:59:59.999 }")); // EquivalentTimeTrue
        Assert.False(await Helpers.CheckBool("{ @T15:59:59.999, @T20:59:59.999 } ~ { @T15:59:59.999, @T20:59:59.999, null }")); // EquivalentTimeNull
        Assert.False(await Helpers.CheckBool("{ @T15:59:59.999, @T20:59:59.999 } ~ { @T15:59:59.999, @T20:59:59.995 }")); // EquivalentTimeFalse
    }

    [Fact]
    public async Task NotEqual()
    {
        Assert.False(await Helpers.CheckBool("{} != {}")); // NotEqualEmptyAndEmpty
        Assert.False(await Helpers.CheckBool("{ 'a', 'b', 'c' } != { 'a', 'b', 'c' }")); // NotEqualABCAndABC
        Assert.True(await Helpers.CheckBool("{ 'a', 'b', 'c' } != { 'a', 'b' }")); // NotEqualABCAndAB
        Assert.True(await Helpers.CheckBool("{ 'a', 'b', 'c' } != { 1, 2, 3 }")); // NotEqualABCAnd123
        Assert.True(await Helpers.CheckBool("{ 1, 2, 3 } != { 'a', 'b', 'c' }")); // NotEqual123AndABC
        Assert.True(await Helpers.CheckBool("{ 1, 2, 3 } != { '1', '2', '3' }")); // NotEqual123AndString123
        Assert.True(await Helpers.CheckBool("{DateTime(2001, 9, 11, 0, 0, 0, 0), DateTime(2012, 5, 10, 0, 0, 0, 0), DateTime(2014, 12, 10, 0, 0, 0, 0)} != {DateTime(2001, 9, 11, 0, 0, 0, 0), DateTime(2012, 5, 10, 0, 0, 0, 0), DateTime(2014, 12, 1, 0, 0, 0, 0)}")); // NotEqualDateTimeTrue
        Assert.False(await Helpers.CheckBool("{DateTime(2001, 9, 11, 0, 0, 0, 0), DateTime(2012, 5, 10, 0, 0, 0, 0), DateTime(2014, 12, 10, 0, 0, 0, 0)} != {DateTime(2001, 9, 11, 0, 0, 0, 0), DateTime(2012, 5, 10, 0, 0, 0, 0), DateTime(2014, 12, 10, 0, 0, 0, 0)}")); // NotEqualDateTimeFalse
        Assert.True(await Helpers.CheckBool("{ @T15:59:59.999, @T20:59:59.999 } = { @T15:59:59.999, @T20:59:59.999 }")); // NotEqualTimeTrue
        Assert.False(await Helpers.CheckBool("{ @T15:59:59.999, @T20:59:59.999 } = { @T15:59:59.999, @T20:59:49.999 }")); // NotEqualTimeFalse
    }

    [Fact]
    public async Task ProperContains()
    {
        Assert.False(await Helpers.CheckBool("{'s', 'u', 'n'} properly includes null")); // ProperContainsNullRightFalse
        Assert.True(await Helpers.CheckBool("{'s', 'u', 'n', null} properly includes null")); // ProperContainsNullRightTrue
        Assert.True(await Helpers.CheckBool("{ @T15:59:59, @T20:59:59.999, @T20:59:49.999 } properly includes @T15:59:59")); // ProperContainsTimeTrue
        Assert.True(await Helpers.CheckBool("({ @T15:59:59.999, @T20:59:59.999, @T20:59:49.999 } properly includes @T15:59:59) is null")); // ProperContainsTimeNull
    }

    [Fact]
    public async Task ProperIn()
    {
        Assert.False(await Helpers.CheckBool("null properly included in {'s', 'u', 'n'}")); // ProperInNullRightFalse
        Assert.True(await Helpers.CheckBool("null properly included in {'s', 'u', 'n', null}")); // ProperInNullRightTrue
        Assert.True(await Helpers.CheckBool("@T15:59:59 properly included in { @T15:59:59, @T20:59:59.999, @T20:59:49.999 }")); // ProperInTimeTrue
        Assert.True(await Helpers.CheckBool("(@T15:59:59 properly included in { @T15:59:59.999, @T20:59:59.999, @T20:59:49.999 }) is null")); // ProperInTimeNull
    }

    [Fact]
    public async Task ProperlyIncludes()
    {
        Assert.False(await Helpers.CheckBool("{} properly includes {}")); // ProperIncludesEmptyAndEmpty
        Assert.False(await Helpers.CheckBool("{null} properly includes {null}")); // ProperIncludesListNullAndListNull
        Assert.True(await Helpers.CheckBool("{1, 2, 3} properly includes {}")); // ProperIncludes123AndEmpty
        Assert.True(await Helpers.CheckBool("{1, 2, 3} properly includes {2}")); // ProperIncludes123And2
        Assert.False(await Helpers.CheckBool("{1, 2, 3} properly includes {4}")); // ProperIncludes123And4
        Assert.True(await Helpers.CheckBool("{DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10)} properly includes {DateTime(2012, 5, 10), DateTime(2014, 12, 10)}")); // ProperIncludesDateTimeTrue
        Assert.False(await Helpers.CheckBool("{DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10)} properly includes {DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10)}")); // ProperIncludesDateTimeFalse
        Assert.True(await Helpers.CheckBool("{ @T15:59:59.999, @T20:59:59.999, @T20:59:49.999 } properly includes { @T15:59:59.999, @T20:59:59.999 }")); // ProperIncludesTimeTrue
        Assert.False(await Helpers.CheckBool("{ @T15:59:59.999, @T20:59:59.999, @T20:59:49.999 } properly includes { @T15:59:59.999, @T20:59:59.999, @T14:59:22.999 }")); // ProperIncludesTimeFalse
        Assert.True(await Helpers.CheckBool("(null properly includes {2}) is null")); // ProperlyIncludesNullLeft
    }

    [Fact]
    public async Task ProperlyIncludedIn()
    {
        Assert.False(await Helpers.CheckBool("{} properly included in {}")); // ProperIncludedInEmptyAndEmpty
        Assert.False(await Helpers.CheckBool("{null} properly included in {null}")); // ProperIncludedInListNullAndListNull
        Assert.True(await Helpers.CheckBool("{} properly included in {1, 2, 3}")); // ProperIncludedInEmptyAnd123
        Assert.True(await Helpers.CheckBool("{2} properly included in {1, 2, 3}")); // ProperIncludedIn2And123
        Assert.False(await Helpers.CheckBool("{4} properly included in {1, 2, 3}")); // ProperIncludedIn4And123
        Assert.True(await Helpers.CheckBool("{DateTime(2012, 5, 10), DateTime(2014, 12, 10)} properly included in {DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10)}")); // ProperIncludedInDateTimeTrue
        Assert.False(await Helpers.CheckBool("{DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10)} properly included in {DateTime(2001, 9, 11), DateTime(2012, 5, 10), DateTime(2014, 12, 10)}")); // ProperIncludedInDateTimeFalse
        Assert.True(await Helpers.CheckBool("{ @T15:59:59.999, @T20:59:59.999 } properly included in { @T15:59:59.999, @T20:59:59.999, @T20:59:49.999 }")); // ProperIncludedInTimeTrue
        Assert.False(await Helpers.CheckBool("{ @T15:59:59.999, @T20:59:59.999, @T14:59:22.999 } properly included in { @T15:59:59.999, @T20:59:59.999, @T20:59:49.999 }")); // ProperIncludedInTimeFalse
        Assert.True(await Helpers.CheckBool("({'s', 'u', 'n'} properly included in null) is null")); // ProperlyIncludedInNulRight
    }

    [Fact]
    public async Task SingletonFrom()
    {
        Assert.True(await Helpers.CheckBool("(singleton from {}) is null")); // SingletonFromEmpty
        Assert.True(await Helpers.CheckBool("(singleton from {null}) is null")); // SingletonFromListNull
        Assert.True(await Helpers.CheckBool("singleton from { 1 } = 1")); // SingletonFrom1
        Assert.True(await Helpers.CheckBool("singleton from { 1, 2 } = ")); // SingletonFrom12
        Assert.True(await Helpers.CheckBool("singleton from { DateTime(2012, 5, 10) } = @2012-05-10T")); // SingletonFromDateTime
        Assert.True(await Helpers.CheckBool("singleton from { @T15:59:59.999 } = @T15:59:59.999")); // SingletonFromTime
    }

    [Fact]
    public async Task Skip()
    {
        Assert.True(await Helpers.CheckBool("(Skip(null, 3)) is null")); // SkipNull
        Assert.True(await Helpers.CheckBool("Skip({1,2,3,4,5}, 2) = {3, 4, 5}")); // SkipEven
        Assert.True(await Helpers.CheckBool("Skip({1,2,3,4,5}, 3) = {4, 5}")); // SkipOdd
        Assert.True(await Helpers.CheckBool("Skip({1,2,3,4,5}, 0) = {1,2,3,4,5}")); // SkipNone
        Assert.True(await Helpers.CheckBool("Skip({1,2,3,4,5}, 5) = {}")); // SkipAll
    }

    [Fact]
    public async Task Tail()
    {
        Assert.True(await Helpers.CheckBool("(Tail(null)) is null")); // TailNull
        Assert.True(await Helpers.CheckBool("Tail({1,2,3,4}) = {2,3,4}")); // TailEven
        Assert.True(await Helpers.CheckBool("Tail({1,2,3,4,5}) = {2,3,4,5}")); // TailOdd
        Assert.True(await Helpers.CheckBool("Tail({}) = {}")); // TailEmpty
        Assert.True(await Helpers.CheckBool("Tail({1}) = {}")); // TailOneElement
    }

    [Fact]
    public async Task Take()
    {
        Assert.True(await Helpers.CheckBool("(Take(null, 3)) is null")); // TakeNull
        Assert.True(await Helpers.CheckBool("Take({1,2,3}, null as Integer) = {}")); // TakeNullEmpty
        Assert.True(await Helpers.CheckBool("Take({1,2,3}, 0) = {}")); // TakeEmpty
        Assert.True(await Helpers.CheckBool("Take({1,2,3,4}, 2) = {1, 2}")); // TakeEven
        Assert.True(await Helpers.CheckBool("Take({1,2,3,4}, 3) = {1, 2, 3}")); // TakeOdd
        Assert.True(await Helpers.CheckBool("Take({1,2,3,4}, 4) = {1, 2, 3, 4}")); // TakeAll
    }

    [Fact]
    public async Task Union()
    {
        Assert.True(await Helpers.CheckBool("{} union {} = {}")); // UnionEmptyAndEmpty
        Assert.True(await Helpers.CheckBool("{ null } union { null } = {null}")); // UnionListNullAndListNull
        Assert.True(await Helpers.CheckBool("{ 1, 2, 3 } union {} = {1, 2, 3}")); // Union123AndEmpty
        Assert.True(await Helpers.CheckBool("{ 1, 2, 3 } union { 2 } = {1, 2, 3}")); // Union123And2
        Assert.True(await Helpers.CheckBool("{ 1, 2, 3 } union { 4 } = {1, 2, 3, 4}")); // Union123And4
        Assert.True(await Helpers.CheckBool("{ DateTime(2001, 9, 11)} union {DateTime(2012, 5, 10), DateTime(2014, 12, 10) } = {@2001-09-11T, @2012-05-10T, @2014-12-10T}")); // UnionDateTime
        Assert.True(await Helpers.CheckBool("{ @T15:59:59.999, @T20:59:59.999, @T12:59:59.999 } union { @T10:59:59.999 } = {@T15:59:59.999, @T20:59:59.999, @T12:59:59.999, @T10:59:59.999}")); // UnionTime
    }
}

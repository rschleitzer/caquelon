using Xunit;

namespace Fire.Cql.Tests;

public class CqlAggregateTest
{
    [Fact]
    public async Task AggregateTests()
    {
        Assert.True(await Helpers.CheckBool("(({ 1, 2, 3, 4, 5 }) Num aggregate Result starting 1: Result * Num) = 120")); // FactorialOfFive
        Assert.True(await Helpers.CheckBool("(  ({  Interval[@2012-01-01, @2012-02-28],  Interval[@2012-02-01, @2012-03-31],  Interval[@2012-03-01, @2012-04-30]  }) M  aggregate R starting (null as List<Interval<DateTime>>): R union ({  M X  let S: Max({ end of Last(R) + 1 day, start of X }),  E: S + Quantity{ value: duration in days of X, unit: 'days' }  return Interval[S, E]  })  ) =   {  Interval[@2012-01-01, @2012-02-28],  Interval[@2012-02-29, @2012-04-28],  Interval[@2012-04-29, @2012-06-28]  }  ")); // RolledOutIntervals
        Assert.True(await Helpers.CheckBool("(  ({ 1, 2, 3, 4, 5 }) Num  aggregate Result starting 1: Result + Num  ) = 16")); // AggregateSumWithStart
        Assert.True(await Helpers.CheckBool("(  ({ 1, 2, 3, 4, 5 }) Num  aggregate Result: Coalesce(Result, 0) + Num  ) = 15")); // AggregateSumWithNull
        Assert.True(await Helpers.CheckBool("(  ({ 1, 1, 2, 2, 2, 3, 4, 4, 5 }) Num  aggregate all Result: Coalesce(Result, 0) + Num  ) = 24")); // AggregateSumAll
        Assert.True(await Helpers.CheckBool("(  ({ 1, 1, 2, 2, 2, 3, 4, 4, 5 }) Num  aggregate distinct Result: Coalesce(Result, 0) + Num  ) = 15")); // AggregateSumDistinct
        Assert.True(await Helpers.CheckBool("(  from ({1}) X, ({2}) Y, ({3}) Z  aggregate Agg: Coalesce(Agg, 0) + X + Y + Z  ) = 6")); // Multi
        Assert.True(await Helpers.CheckBool("(  from ({1, 2}) X, ({1, 2}) Y, ({1, 2}) Z  aggregate Agg starting 0: Agg + X + Y + Z  ) = 36")); // MegaMulti
        Assert.True(await Helpers.CheckBool("(  from ({1, 2, 2, 1}) X, ({1, 2, 1, 2}) Y, ({2, 1, 2, 1}) Z  aggregate distinct Agg starting 1: Agg + X + Y + Z  ) = 37")); // MegaMultiDistinct
    }
}

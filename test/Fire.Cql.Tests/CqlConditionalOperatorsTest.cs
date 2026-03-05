using Xunit;

namespace Fire.Cql.Tests;

public class CqlConditionalOperatorsTest
{
    [Fact]
    public async Task ifthenelse()
    {
        Assert.True(await Helpers.CheckBool("if 10 > 5 then 5 else 10 = 5")); // IfTrue1
        Assert.True(await Helpers.CheckBool("if 10 = 5 then 10 + 5 else 10 - 5 = 5")); // IfFalse1
        Assert.True(await Helpers.CheckBool("if 10 = null then 5 else 10 = 10")); // IfNull1
    }

    [Fact]
    public async Task standardcase()
    {
        Assert.True(await Helpers.CheckBool("  case  when 10 > 5 then 5  when 5 > 10 then 10  else null  end   = 5")); // StandardCase1
        Assert.True(await Helpers.CheckBool("  case  when 5 > 10 then 5 + 10  when 5 = 10 then 10  else 10 - 5  end   = 5")); // StandardCase2
        Assert.True(await Helpers.CheckBool("  case  when null ~ 10 then null + 10  when null ~ 5 then 5  else 5 + 10  end   = 15")); // StandardCase3
    }

    [Fact]
    public async Task selectedcase()
    {
        Assert.True(await Helpers.CheckBool("  case 5  when 5 then 12  when 10 then 10 + 5  else 10 - 5  end   = 12")); // SelectedCase1
        Assert.True(await Helpers.CheckBool("  case 10  when 5 then 12  when 10 then 10 + 5  else 10 - 5  end   = 15")); // SelectedCase2
        Assert.True(await Helpers.CheckBool("  case 10 + 5  when 5 then 12  when 10 then 10 + 5  else 10 - 5  end   = 5")); // SelectedCase3
    }
}

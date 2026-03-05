using Xunit;

namespace Fire.Cql.Tests;

public class CqlErrorsAndMessagingOperatorsTest
{
    [Fact]
    public async Task Messaging()
    {
        Assert.True(await Helpers.CheckBool("Message(1, true, '100', 'Message', 'Test Message') = 1")); // TestMessageInfo
        Assert.True(await Helpers.CheckBool("Message(2, true, '200', 'Warning', 'You have been warned!') = 2")); // TestMessageWarn
        Assert.True(await Helpers.CheckBool("Message({3, 4, 5}, true, '300', 'Trace', 'This is a trace') = {3, 4, 5}")); // TestMessageTrace
        Assert.True(await Helpers.CheckBool("Message(3 + 1, true, '400', 'Error', 'This is an error!') = ")); // TestMessageError
    }
}

namespace Fire.Cql.Tests;

public static class Helpers
{
    public static Task<bool> CheckBool(string expression)
    {
        var result = ElmInterpreter.Evaluate(expression);
        return Task.FromResult(result is true);
    }
}

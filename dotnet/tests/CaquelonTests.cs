using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fondue.Caquelon;

namespace tests
{
    [TestClass]
    public class CaquelonTests
    {
        [TestMethod]
        public void ParserTest()
        {
            var operation = new Parser("false").parse_operation();
        }
    }
}

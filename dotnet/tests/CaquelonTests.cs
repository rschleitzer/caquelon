using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fondue.Caquelon;

namespace tests
{
    [TestClass]
    public class CaquelonTests
    {
        [TestMethod]
        public void Parser()
        {
            new Parser("false").parse_operation();
        }

        [TestMethod]
        public void BuildLibrary()
        {
            Transpiler.BuildLibrary("LibraryTest.cql");
        }
    }
}

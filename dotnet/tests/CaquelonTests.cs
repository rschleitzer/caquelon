using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fondue.Caquelon;

namespace tests
{
    [TestClass]
    public class CaquelonTests
    {
        [TestMethod]
        public void ExecuteExpression()
        {
            var empty = new string[] { };
            Assert.AreEqual(Transpiler.ExecuteIntegerExpression("false", empty), 0);
            Assert.AreEqual(Transpiler.ExecuteIntegerExpression("true", empty), 1);
        }

        [TestMethod]
        public void ParserTest()
        {
            Transpiler.BuildLibrary("ParserTest.cql");
        }
    }
}

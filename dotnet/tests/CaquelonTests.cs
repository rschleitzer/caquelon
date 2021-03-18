using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fondue.Caquelon;

namespace tests
{
    [TestClass]
    public class CaquelonTests
    {
        //[TestMethod]
        //public void Boolean()
        //{
        //    var empty = new string[] { };
        //    Assert.AreEqual(Transpiler.ExecuteIntegerExpression("false", empty), 0);
        //    Assert.AreEqual(Transpiler.ExecuteIntegerExpression("true", empty), 1);
        //}

        [TestMethod]
        public void Integer()
        {
            Assert.AreEqual(Transpiler.ExecuteIntegerExpression("42", new string[] { }), 42);
        }

        [TestMethod]
        public void ParserTest()
        {
            Transpiler.BuildLibrary("ParserTest.cql");
        }

        [TestMethod]
        public void LogicalOperatorsTest()
        {
            var fileSyntax = Transpiler.ParseFile("CqlLogicalOperatorsTest.cql");
        }
    }
}

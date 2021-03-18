using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fondue.Caquelon;
using System.Linq;
using System;

namespace tests
{
    [TestClass]
    public class SimpleTests
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
            Assert.AreEqual(Transpiler.ExecuteIntegerExpression("if (true and true) = (true) then 1 else 0", new string[] { }), 1, "TrueAndTrue");
        }
    }
}

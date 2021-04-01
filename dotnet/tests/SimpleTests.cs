using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fondue.Caquelon;
using System.Linq;
using System;

namespace SimpleTest
{
    [TestClass]
    public class Elementary
    {
        [TestMethod]
        public void Parser()
        {
            Transpiler.BuildLibrary("ParserTest.cql");
        }

        [TestMethod]
        public void Integer()
        {
            Assert.AreEqual(Transpiler.ExecuteIntegerExpression("42", new string[] { }), 42);
        }

        [TestMethod]
        public void If()
        {
            Assert.AreEqual(Transpiler.ExecuteIntegerExpression("if true then 1 else 0", new string[] { }), 1);
        }

        [TestMethod]
        public void Parenthesized()
        {
            Assert.AreEqual(Transpiler.ExecuteIntegerExpression("(42)", new string[] { }), 42);
        }

        [TestMethod]
        public void FirstNot()
        {
            Assert.AreEqual(Transpiler.ExecuteIntegerExpression(@"if (not true) = (false) then 1 else 0", new string[] { }), 1);
        }
    }
}

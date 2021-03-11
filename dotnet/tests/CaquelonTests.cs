using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scaly.Compiler;

namespace tests
{
    [TestClass]
    public class CaquelonTests
    {
        [TestMethod]
        public void TestShortest()
        {
            Assert.AreEqual(Compiler.compileAndRunProgram("42", new string[] { }), 42);
        }
    }
}

(define (test-module-file) (file ($ "Standard/"(name)".cs") ($
"using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fondue.Caquelon;

namespace CqlTest
{
    [TestClass]
    public class "(name)"
    {"
    (for-selected-children "group" (lambda (group) ($
"
        [TestMethod]
        public void "(string-replace (string-replace (string-replace (if (name-of group) (if (string=? "ToString" (name-of group)) "ConvertToString" (name-of group)) "TestMessage") " " "") "#" "") "-" "")"()
        {
"       (for-selected-children-of group "test" (lambda (test) (let ((test-output (data (select-children "output" test)))) ($
"            Assert.AreEqual(Transpiler.ExecuteIntegerExpression(@\"if ("(data (select-children "expression" test))") = ("(data (select-children "output" test))") then 1 else 0\", new string[] { }), 1, \""(name-of test)"\");
"       ))))
"        }
"   )))
"    }
}
")))

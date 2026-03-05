(define (test-module-file) (file ($ (name)".cs") ($
"using Xunit;

namespace Fire.Cql.Tests;

public class "(name)"
{"
    (for-selected-children "group" (lambda (group) ($
"
    [Fact]
    public async Task "(let ((method-name (string-replace (string-replace (string-replace (name-of group) " " "") "#" "") "-" "")))
        (if (string=? method-name "ToString") "CqlToString" method-name))"()
    {
"       (for-selected-children-of group "test" (lambda (test) (let ((test-output (data (select-children "output" test)))) ($
"        Assert."(case test-output (("true") "True") (("false") "False") (else "True"))"(await Helpers.CheckBool(\""(case test-output (("null") "(") (else ""))(escape-csharp-string (normalize-whitespace (data (select-children "expression" test)))) (case test-output (("true" "false") "") (("null") ") is null") (else ($ " = " (escape-csharp-string (normalize-whitespace test-output)))))"\")); // "(name-of test)"
"       ))))
"    }
"   )))
"}
")))

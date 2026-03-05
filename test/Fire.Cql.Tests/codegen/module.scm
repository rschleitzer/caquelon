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
"       (for-selected-children-of group "test" (lambda (test)
          (let* ((expr-node (node-list-first (select-children "expression" test)))
                 (inv-attr (attribute-string "invalid" expr-node))
                 (is-invalid (and inv-attr (string=? inv-attr "true")))
                 (test-output (data (select-children "output" test)))
                 (expr-text (escape-csharp-string (normalize-whitespace (data (select-children "expression" test)))))
                 (has-output (not (or (string=? test-output "true") (string=? test-output "false") (string=? test-output "null") (string=? test-output ""))))
                 (needs-parens (and has-output (or (starts-with? expr-text "collapse ") (starts-with? expr-text "expand ") (starts-with? expr-text "distinct ") (starts-with? expr-text "flatten ") (starts-with? expr-text "if ") (string-contains? expr-text " aggregate ")))))
          (if is-invalid ""
          ($
"        Assert."(case test-output (("true") "True") (("false") "False") (else "True"))"(await Helpers.CheckBool(\""(case test-output (("null" "") "(") (else ""))(if needs-parens "(" "") expr-text (if needs-parens ")" "")(case test-output (("true" "false") "") (("null" "") ") is null") (else ($ " = " (escape-csharp-string (normalize-whitespace test-output)))))"\")); // "(name-of test)"
"       )))))
"    }
"   )))
"}
")))

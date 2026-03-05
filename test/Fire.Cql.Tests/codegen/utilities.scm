(declare-flow-object-class element                  "UNREGISTERED::James Clark//Flow Object Class::element")
(declare-flow-object-class empty-element            "UNREGISTERED::James Clark//Flow Object Class::empty-element")
(declare-flow-object-class document-type            "UNREGISTERED::James Clark//Flow Object Class::document-type")
(declare-flow-object-class processing-instruction   "UNREGISTERED::James Clark//Flow Object Class::processing-instruction")
(declare-flow-object-class entity                   "UNREGISTERED::James Clark//Flow Object Class::entity")
(declare-flow-object-class entity-ref               "UNREGISTERED::James Clark//Flow Object Class::entity-ref")
(declare-flow-object-class formatting-instruction   "UNREGISTERED::James Clark//Flow Object Class::formatting-instruction")
(declare-characteristic preserve-sdata?             "UNREGISTERED::James Clark//Characteristic::preserve-sdata?" #f)

(define $ string-append)

(define % attribute-string)

(define (file filename contents)
    (make entity system-id: filename (make formatting-instruction data: contents)))

(define (name-of node) (% "name" node))

(define (name) (name-of (current-node)))

(define (select-children name node)
    (select-elements (children node) name))

(define (for-selected-children element func)
    (for-selected-children-of (current-node) element func))

(define (for-selected-children-of node element func)
    (for (select-children element node) func))

(define (for nl func)
    (apply $ (map func (node-list->list nl))))

(define (string-replace string target repl)
  (let loop ((str string) (pos 0))
    (if (>= pos (string-length str))
    str
    (loop (repl-substring str target repl pos)
        (if (repl-substring? str target pos)
            (+ (string-length repl) pos)
            (+ 1 pos))))))

(define (whitespace? c)
  (or (char=? c #\space)
      (char=? c #\
)
      (char=? c #\	)))

(define (normalize-whitespace str)
  (let loop ((chars (string->list str)) (result '()) (in-space #f))
    (cond
      ((null? chars)
       (list->string (reverse result)))
      ((whitespace? (car chars))
       (if in-space
           (loop (cdr chars) result #t)
           (loop (cdr chars) (cons #\space result) #t)))
      (else
       (loop (cdr chars) (cons (car chars) result) #f)))))

(define (escape-csharp-string str)
  (string-replace (string-replace str "\\" "\\\\") "\"" "\\\""))

(define (repl-substring? string target pos)
    (let* (
        (could-match (<= (+ pos (string-length target)) (string-length string)))
        (match (if could-match (substring string pos (+ pos (string-length target))) ""))
        )
        (and could-match (string=? match target))))

(define (repl-substring string target repl pos)
    (let ((matches (repl-substring? string target pos)))
        (if matches
            ($
                (substring string 0 pos)
                repl
                (substring string (+ pos (string-length target)) (string-length string))
            )
            string
        )))

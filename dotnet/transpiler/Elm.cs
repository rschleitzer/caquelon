using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scaly.Compiler;

namespace Fondue.Caquelon.Elm
{
    public abstract class Expression
    {
        public Span Span;
    }

    public class Literal : Expression
    {
        public string Value;
        public string ValueType;
    }

    public class If : Expression
    {
        public Expression Condition;
        public Expression Then;
        public Expression Else;
    }
}

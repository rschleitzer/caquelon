using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fondue.Caquelon.Elm
{
    public class Builder
    {
        internal static Expression BuildExpression(ExpressionSyntax expressionSyntax)
        {
            var operand = expressionSyntax.operands[0];
            switch (operand)
            {
                case LiteralSyntax literalSyntax:
                    return BuildLiteral(literalSyntax);
                case IfSyntax ifSyntax:
                    return BuildIf(ifSyntax);
                case TrueSyntax trueSyntax:
                    return new Literal { Value = "true", ValueType = "Boolean", Span = trueSyntax.span };
                case FalseSyntax falseSyntax:
                    return new Literal { Value = "false", ValueType = "Boolean", Span = falseSyntax.span };
                case OpenIntervalSyntax openIntervalSyntax:
                    return BuildOpenInterval(openIntervalSyntax);
                default:
                    throw new NotImplementedException($"The {expressionSyntax.GetType()} is not yet implemented.");
            }
        }

        static Expression BuildLiteral(LiteralSyntax literalSyntax)
        {
            switch (literalSyntax.literal)
            {
                case Integer integer:
                    return new Literal { Value = integer.value, ValueType = "Integer", Span = literalSyntax.span };
                default:
                    throw new NotImplementedException($"The {literalSyntax.GetType()} is not yet implemented.");
            }
        }

        static Expression BuildIf(IfSyntax ifSyntax)
        {
            return new If
            {
                Condition = BuildExpression(ifSyntax.condition),
                Then = BuildExpression(ifSyntax.consequent),
                Else = BuildExpression(ifSyntax.alternative),
                Span = ifSyntax.span,
            };
        }

        static Expression BuildOpenInterval(OpenIntervalSyntax openIntervalSyntax)
        {
            switch (openIntervalSyntax.end)
            {
                case OpenEndSyntax _:
                    if (openIntervalSyntax.components.Length > 1)
                        throw new NotImplementedException($"Open Intervals are not yet implemented.");
                    return BuildExpression(openIntervalSyntax.components[0].expression);
                default:
                    throw new NotImplementedException($"Half closed Intervals are not yet implemented.");
            }
        }
    }
}

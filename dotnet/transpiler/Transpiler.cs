using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scaly.Compiler;
using Scaly.Compiler.Model;

namespace Fondue.Caquelon
{
    public class Transpiler
    {
        public static FileSyntax BuildLibrary(string library)
        {
            var fileSyntax = ParseFile(library);
            return fileSyntax;
        }

        public static int ExecuteIntegerExpression(string expression, string[] arguments)
        {
            var expressionSyntax = ParseOperands(expression);
            var source = BuildExpression(expressionSyntax);
            return Compiler.ExecuteModel(source, arguments);
        }


        static Source BuildExpression(ExpressionSyntax expressionSyntax)
        {
            var source = new Source
            {
                FileName = "",
                Usings = new List<Namespace> { new Namespace { Path = "Boolean" } },
            };

            if (source.Functions == null)
                source.Functions = new List<Function>();
            var main = Modeler.BuildSource("main.scaly").Functions[0];
            main.Routine.Operation = new Operation { SourceOperands = new List<Operand> { new Operand { Expression = new Scope { Operations = new List<Operation> { BuildOperation(expressionSyntax) } } } } };
            main.Source = source;
            source.Functions.Add(main);
            return source;
        }

        static Operation BuildOperation(ExpressionSyntax expressionSyntax)
        {
            return new Operation { SourceOperands = expressionSyntax.operands.ToList().ConvertAll(it => BuildOperand(it)) };
        }

        static Operand BuildOperand(object operandSyntax)
        {
            switch (operandSyntax)
            {
                case FalseSyntax trueSyntax:
                    return new Operand { Expression = new Name { Path = "false", Span = trueSyntax.span } };
                case TrueSyntax trueSyntax:
                    return new Operand { Expression = new Name { Path = "true", Span = trueSyntax.span } };
                case PrimitiveSyntax primitiveSyntax:
                    return BuildPrimitive(primitiveSyntax);
                case IfSyntax ifSyntax:
                    return new Operand { Expression = new If
                    {
                        Condition = BuildOperation(ifSyntax.condition).SourceOperands,
                        Consequent = BuildOperation(ifSyntax.consequent),
                        Alternative = BuildOperation(ifSyntax.alternative),
                        Span = ifSyntax.span
                    } };
                default:
                    throw new NotImplementedException($"The {operandSyntax.GetType()} is not yet implemented.");
            }
        }

        static Operand BuildPrimitive(PrimitiveSyntax primitiveSyntax)
        {
            switch (primitiveSyntax.literal)
            {
                case Integer integer:
                    return new Operand { Expression = new IntegerConstant { Value = int.Parse(integer.value) } };
                default:
                    throw new NotImplementedException($"The {primitiveSyntax.GetType()} is not yet implemented.");
            }
        }

        static ExpressionSyntax ParseOperands(string text)
        {
            var parser = new Parser(text);
            var expressionSyntax = parser.parse_expression();
            if (!parser.is_at_end())
                throw new CompilerException
                ("Unexpected content at end of file.",
                    new Span
                    {
                        file = "",
                        start = new Position { line = parser.get_current_line(), column = parser.get_current_column() },
                        end = new Position { line = parser.get_current_line(), column = parser.get_current_column() }
                    }
                );
            return expressionSyntax;
        }

        public static FileSyntax ParseFile(string file)
        {
            var text = System.IO.File.ReadAllText(file);
            var parser = new Parser(text);

            var fileSyntax = parser.parse_file(file);
            if (!parser.is_at_end())
                throw new CompilerException
                ("Unexpected content at end of file.",
                    new Span
                    {
                        file = file,
                        start = new Position { line = parser.get_current_line(), column = parser.get_current_column() },
                        end = new Position { line = parser.get_current_line(), column = parser.get_current_column() }
                    }
                );
            return fileSyntax;
        }
    }
}

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
            var expressionSyntax = ParseExpression(expression);
            var elmExpression = Elm.Builder.BuildExpression(expressionSyntax);
            var source = TranslateExpression(elmExpression);
            return Compiler.ExecuteModel(source, arguments);
        }

        static Source TranslateExpression(Elm.Expression expression)
        {
            var cql = Modeler.BuildSource("cql.scaly");
            var source = new Source
            {
                FileName = "",
                Usings = new List<Namespace> { new Namespace { Path = "CQL" } },
                Sources = new List<Source> { cql },
            };

            if (source.Functions == null)
                source.Functions = new List<Function>();
            var main = Modeler.BuildSource("main.scaly").Functions[0];
            main.Routine.Operation = CompileExpression(expression);
            main.Source = source;
            source.Functions.Add(main);
            return source;
        }

        static Operation CompileExpression(Elm.Expression expression)
        {
            List<Operand> operands;
            switch (expression)
            {
                case Elm.Literal literal:
                    operands = CompileLiteral(literal);
                    break;
                case Elm.If @if:
                    operands = CompileIf(@if);
                    break;
                case Elm.Not not:
                    operands = CompileNot(not);
                    break;
                default:
                    throw new NotImplementedException($"The {expression.GetType()} is not yet implemented.");
            }
            return new Operation { SourceOperands = operands };
        }

        static List<Operand> CompileIf(Elm.If @if)
        {
            return new List<Operand> { new Operand
            {
                Expression = new If
                {
                    Condition = CompileExpression(@if.Condition).SourceOperands,
                    Consequent = CompileExpression(@if.Then),
                    Alternative = CompileExpression(@if.Else),
                    Span = @if.Span
                }
            } };
        }

        static List<Operand> CompileNot(Elm.Not not)
        {
            return new List<Operand>
            {
                new Operand { Expression = new Name { Path = "not", Span = not.Span } },
                new Operand { Expression = new Scaly.Compiler.Model.Tuple { Components = new List<Component> { new Component { Value = CompileExpression(not.Expression).SourceOperands, Span = not.Expression.Span } } } }
            };
        }

        static List<Operand> CompileLiteral(Elm.Literal literal)
        {
            switch (literal.ValueType)
            {
                case "Integer":
                    return new List<Operand> { new Operand { Expression = new IntegerConstant { Value = int.Parse(literal.Value), Span = literal.Span } } };
                case "Boolean":
                    return new List<Operand> { new Operand { Expression = new Name { Path = "true", Span = literal.Span } } };
                default:
                    throw new NotImplementedException($"The {literal.ValueType} value type is not yet implemented.");
            }
        }

        static ExpressionSyntax ParseExpression(string text)
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

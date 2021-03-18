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
            var fileSyntax = parseFile(library);
            return fileSyntax;
        }

        public static int ExecuteIntegerExpression(string expression, string[] arguments)
        {
            var expressionSyntax = parseExpression(expression);
            var source = BuildIntegerExpression(expressionSyntax);
            return Compiler.ExecuteModel(source, arguments);
        }


        static Source BuildIntegerExpression(object expressionSyntax)
        {
            var source = new Source
            {
                FileName = "",
                Usings = new List<Namespace> { new Namespace { Path = "Boolean" } },
            };

            if (source.Functions == null)
                source.Functions = new List<Function>();
            var main = Modeler.BuildSource("main.scaly").Functions[0];
            main.Routine.Operation = new Operation { Operands = new List<Operand> { new Operand { Expression = new Scope { Operations = new List<Operation> { BuildOperation(expressionSyntax) } } } } };
            main.Source = source;
            source.Functions.Add(main);
            return source;
        }

        static Operation BuildOperation(object expressionSyntax)
        {
            var operation = new Operation { Operands = new List<Operand>() };
            switch (expressionSyntax)
            {
                case FalseSyntax trueSyntax:
                    operation.Operands.Add(new Operand { Expression = new Name { Path = "false", Span = trueSyntax.span } });
                    operation.Operands.Add(new Operand { Expression = new Name { Path = "as Integer", Span = trueSyntax.span } });
                    return operation;
                case TrueSyntax trueSyntax:
                    operation.Operands.Add(new Operand { Expression = new Name { Path = "true", Span = trueSyntax.span } });
                    operation.Operands.Add(new Operand { Expression = new Name { Path = "as Integer", Span = trueSyntax.span } });
                    return operation;
                case PrimitiveSyntax primitiveSyntax:
                    operation.Operands.Add( BuildPrimitive(primitiveSyntax));
                    return operation;
                default:
                    throw new NotImplementedException($"The {expressionSyntax.GetType()} is not yet implemented.");
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

        static object parseExpression(string text)
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

        static FileSyntax parseFile(string file)
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

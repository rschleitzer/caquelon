using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scaly.Compiler;

namespace Fondue.Caquelon
{
    public class Transpiler
    {
        public static Definition BuildLibrary(string library)
        {
            var fileSyntax = parseFile(library);
            return new Definition { };
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

using System;
using System.Collections.Generic;
using System.Text;
using Scaly.Compiler;

namespace Fondue.Caquelon
{
    public class Parser
    {
        Lexer lexer;
        string file_name = "";
        HashSet<string> keywords = new HashSet<string>(new string[] {
            "called",
            "code",
            "codesystem",
            "codesystems",
            "concept",
            "context",
            "default",
            "define",
            "display",
            "external",
            "from",
            "function",
            "include",
            "library",
            "parameter",
            "private",
            "public",
            "returns",
            "using",
            "valueset",
            "version",
        });

        public Parser(string deck)
        {
            lexer = new Lexer(deck);
        }

        public FileSyntax parse_file(string file_name)
        {
            this.file_name = file_name;
            var library = parse_library();
            var usings = parse_using_list();
            var includes = parse_include_list();
            var definitions = parse_definition_list();
            if (definitions != null)
            {
                if (!is_at_end())
                {
                    throw new ParserException("Unable to parse definition list.", new Span { file = file_name, start = new Position { line = lexer.previous_line, column = lexer.previous_column }, end = new Position { line = lexer.line, column = lexer.column } } );
                }
            }

            var ret = new FileSyntax
            {
                span = new Span
                {
                    file = file_name                },
                library = library,
                usings = usings,
                includes = includes,
                definitions = definitions,
            };

            return ret;
        }

        public LibrarySyntax parse_library()
        {
            var start = lexer.get_previous_position();

            var success_library_1 = lexer.parse_keyword("library");
            if (!success_library_1)
                    return null;
            var name = parse_name();
            if (name == null)
                throw new ParserException("Unable to parse name.", new Span { file = file_name, start = new Position { line = lexer.previous_line, column = lexer.previous_column }, end = new Position { line = lexer.line, column = lexer.column } } );
            var version = parse_version();

            var end = lexer.get_position();

            var ret = new LibrarySyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                name = name,
                version = version,
            };

            return ret;
        }

        public NameSyntax parse_name()
        {
            var start = lexer.get_previous_position();

            var name = lexer.parse_identifier(keywords);
            if (name != null)
            {
                if (!is_identifier(name))
                {
                    return null;
                }
            }
            else
            {
                    return null;
            }
            var extensions = parse_extension_list();

            var end = lexer.get_position();

            var ret = new NameSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                name = name,
                extensions = extensions,
            };

            return ret;
        }

        public ExtensionSyntax[] parse_extension_list()
        {
            List<ExtensionSyntax> list = null;
            while (true)
            {
                var node = parse_extension();
                if (node == null)
                    break;

                if (list == null)
                    list = new List<ExtensionSyntax>();

                list.Add(node);
            }

            if (list != null)
                return list.ToArray();
            else
                return null;
        }

        public ExtensionSyntax parse_extension()
        {
            var start = lexer.get_previous_position();

            var success_dot_1 = lexer.parse_punctuation(".");
            if (!success_dot_1)
                    return null;

            var name = lexer.parse_identifier(keywords);
            if (name != null)
            {
                if (!is_identifier(name))
                {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
                }
            }
            else
            {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            }

            var end = lexer.get_position();

            var ret = new ExtensionSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                name = name,
            };

            return ret;
        }

        public VersionSyntax parse_version()
        {
            var start = lexer.get_previous_position();

            var success_version_1 = lexer.parse_keyword("version");
            if (!success_version_1)
                    return null;

            var name = lexer.parse_literal();
            if (name == null)
            {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            }

            var end = lexer.get_position();

            var ret = new VersionSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                name = name,
            };

            return ret;
        }

        public UsingSyntax[] parse_using_list()
        {
            List<UsingSyntax> list = null;
            while (true)
            {
                var node = parse_using();
                if (node == null)
                    break;

                if (list == null)
                    list = new List<UsingSyntax>();

                list.Add(node);
            }

            if (list != null)
                return list.ToArray();
            else
                return null;
        }

        public UsingSyntax parse_using()
        {
            var start = lexer.get_previous_position();

            var success_using_1 = lexer.parse_keyword("using");
            if (!success_using_1)
                    return null;

            var model = lexer.parse_identifier(keywords);
            if (model != null)
            {
                if (!is_identifier(model))
                {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
                }
            }
            else
            {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            }
            var version = parse_version();

            var end = lexer.get_position();

            var ret = new UsingSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                model = model,
                version = version,
            };

            return ret;
        }

        public IncludeSyntax[] parse_include_list()
        {
            List<IncludeSyntax> list = null;
            while (true)
            {
                var node = parse_include();
                if (node == null)
                    break;

                if (list == null)
                    list = new List<IncludeSyntax>();

                list.Add(node);
            }

            if (list != null)
                return list.ToArray();
            else
                return null;
        }

        public IncludeSyntax parse_include()
        {
            var start = lexer.get_previous_position();

            var success_include_1 = lexer.parse_keyword("include");
            if (!success_include_1)
                    return null;
            var name = parse_name();
            if (name == null)
                throw new ParserException("Unable to parse name.", new Span { file = file_name, start = new Position { line = lexer.previous_line, column = lexer.previous_column }, end = new Position { line = lexer.line, column = lexer.column } } );
            var version = parse_version();
            var called = parse_called();

            var end = lexer.get_position();

            var ret = new IncludeSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                name = name,
                version = version,
                called = called,
            };

            return ret;
        }

        public CalledSyntax parse_called()
        {
            var start = lexer.get_previous_position();

            var success_called_1 = lexer.parse_keyword("called");
            if (!success_called_1)
                    return null;

            var name = lexer.parse_literal();
            if (name == null)
            {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            }

            var end = lexer.get_position();

            var ret = new CalledSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                name = name,
            };

            return ret;
        }

        public object[] parse_declaration_list()
        {
            List<object> list = null;
            while (true)
            {
                var node = parse_declaration();
                if (node == null)
                    break;

                if (list == null)
                    list = new List<object>();

                list.Add(node);
            }

            if (list != null)
                return list.ToArray();
            else
                return null;
        }

        public object parse_declaration()
        {
            {
                var node = parse_publicdeclaration();
                if (node != null)
                    return node;
            }
            {
                var node = parse_privatedeclaration();
                if (node != null)
                    return node;
            }
            {
                var node = parse_codesystem();
                if (node != null)
                    return node;
            }
            {
                var node = parse_valueset();
                if (node != null)
                    return node;
            }
            {
                var node = parse_code();
                if (node != null)
                    return node;
            }
            {
                var node = parse_concept();
                if (node != null)
                    return node;
            }
            {
                var node = parse_parameter();
                if (node != null)
                    return node;
            }

            return null;
        }

        public PrivateDeclarationSyntax parse_privatedeclaration()
        {
            var start = lexer.get_previous_position();

            var success_private_1 = lexer.parse_keyword("private");
            if (!success_private_1)
                    return null;
            var declaration = parse_declaration();
            if (declaration == null)
                throw new ParserException("Unable to parse declaration.", new Span { file = file_name, start = new Position { line = lexer.previous_line, column = lexer.previous_column }, end = new Position { line = lexer.line, column = lexer.column } } );

            var end = lexer.get_position();

            var ret = new PrivateDeclarationSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                declaration = declaration,
            };

            return ret;
        }

        public PublicDeclarationSyntax parse_publicdeclaration()
        {
            var start = lexer.get_previous_position();

            var success_private_1 = lexer.parse_keyword("private");
            if (!success_private_1)
                    return null;
            var export = parse_declaration();
            if (export == null)
                throw new ParserException("Unable to parse declaration.", new Span { file = file_name, start = new Position { line = lexer.previous_line, column = lexer.previous_column }, end = new Position { line = lexer.line, column = lexer.column } } );

            var end = lexer.get_position();

            var ret = new PublicDeclarationSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                export = export,
            };

            return ret;
        }

        public CodeSystemSyntax parse_codesystem()
        {
            var start = lexer.get_previous_position();

            var success_codesystem_1 = lexer.parse_keyword("codesystem");
            if (!success_codesystem_1)
                    return null;

            var name = lexer.parse_identifier(keywords);
            if (name != null)
            {
                if (!is_identifier(name))
                {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
                }
            }
            else
            {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            }

            var success_colon_3 = lexer.parse_punctuation(":");
            if (!success_colon_3)
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );

            var id = lexer.parse_literal();
            if (id == null)
            {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            }
            var version = parse_version();

            var end = lexer.get_position();

            var ret = new CodeSystemSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                name = name,
                id = id,
                version = version,
            };

            return ret;
        }

        public ValueSetSyntax parse_valueset()
        {
            var start = lexer.get_previous_position();

            var success_valueset_1 = lexer.parse_keyword("valueset");
            if (!success_valueset_1)
                    return null;

            var name = lexer.parse_identifier(keywords);
            if (name != null)
            {
                if (!is_identifier(name))
                {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
                }
            }
            else
            {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            }

            var success_colon_3 = lexer.parse_punctuation(":");
            if (!success_colon_3)
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );

            var id = lexer.parse_literal();
            if (id == null)
            {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            }
            var version = parse_version();
            var codeSystems = parse_codesystem();

            var end = lexer.get_position();

            var ret = new ValueSetSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                name = name,
                id = id,
                version = version,
                codeSystems = codeSystems,
            };

            return ret;
        }

        public CodeSystemsSyntax parse_codesystems()
        {
            var start = lexer.get_previous_position();

            var success_codesystems_1 = lexer.parse_keyword("codesystems");
            if (!success_codesystems_1)
                    return null;

            var success_left_curly_2 = lexer.parse_punctuation("{");
            if (!success_left_curly_2)
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            var identifiers = parse_codeidentifier_list();
            if (identifiers == null)
                throw new ParserException("Unable to parse codeidentifier list.", new Span { file = file_name, start = new Position { line = lexer.previous_line, column = lexer.previous_column }, end = new Position { line = lexer.line, column = lexer.column } } );

            var success_right_curly_4 = lexer.parse_punctuation("}");
            if (!success_right_curly_4)
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );

            var end = lexer.get_position();

            var ret = new CodeSystemsSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                identifiers = identifiers,
            };

            return ret;
        }

        public CodeIdentifierSyntax[] parse_codeidentifier_list()
        {
            List<CodeIdentifierSyntax> list = null;
            while (true)
            {
                var node = parse_codeidentifier();
                if (node == null)
                    break;

                if (list == null)
                    list = new List<CodeIdentifierSyntax>();

                list.Add(node);
            }

            if (list != null)
                return list.ToArray();
            else
                return null;
        }

        public CodeIdentifierSyntax parse_codeidentifier()
        {
            var start = lexer.get_previous_position();

            var identifier = lexer.parse_identifier(keywords);
            if (identifier != null)
            {
                if (!is_identifier(identifier))
                {
                    return null;
                }
            }
            else
            {
                    return null;
            }
            var extension = parse_extension();

            lexer.parse_punctuation(",");

            var end = lexer.get_position();

            var ret = new CodeIdentifierSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                identifier = identifier,
                extension = extension,
            };

            return ret;
        }

        public CodeSyntax parse_code()
        {
            var start = lexer.get_previous_position();

            var success_code_1 = lexer.parse_keyword("code");
            if (!success_code_1)
                    return null;

            var name = lexer.parse_identifier(keywords);
            if (name != null)
            {
                if (!is_identifier(name))
                {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
                }
            }
            else
            {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            }

            var success_colon_3 = lexer.parse_punctuation(":");
            if (!success_colon_3)
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );

            var id = lexer.parse_literal();
            if (id == null)
            {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            }

            var success_from_5 = lexer.parse_keyword("from");
            if (!success_from_5)
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            var version = parse_codeidentifier();
            if (version == null)
                throw new ParserException("Unable to parse codeidentifier.", new Span { file = file_name, start = new Position { line = lexer.previous_line, column = lexer.previous_column }, end = new Position { line = lexer.line, column = lexer.column } } );
            var display = parse_display();

            var end = lexer.get_position();

            var ret = new CodeSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                name = name,
                id = id,
                version = version,
                display = display,
            };

            return ret;
        }

        public DisplaySyntax parse_display()
        {
            var start = lexer.get_previous_position();

            var success_display_1 = lexer.parse_keyword("display");
            if (!success_display_1)
                    return null;

            var id = lexer.parse_literal();
            if (id == null)
            {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            }

            var end = lexer.get_position();

            var ret = new DisplaySyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                id = id,
            };

            return ret;
        }

        public ConceptSyntax parse_concept()
        {
            var start = lexer.get_previous_position();

            var success_concept_1 = lexer.parse_keyword("concept");
            if (!success_concept_1)
                    return null;

            var name = lexer.parse_identifier(keywords);
            if (name != null)
            {
                if (!is_identifier(name))
                {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
                }
            }
            else
            {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            }

            var success_colon_3 = lexer.parse_punctuation(":");
            if (!success_colon_3)
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );

            var success_left_curly_4 = lexer.parse_punctuation("{");
            if (!success_left_curly_4)
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            var identifiers = parse_codeidentifier_list();
            if (identifiers == null)
                throw new ParserException("Unable to parse codeidentifier list.", new Span { file = file_name, start = new Position { line = lexer.previous_line, column = lexer.previous_column }, end = new Position { line = lexer.line, column = lexer.column } } );

            var success_right_curly_6 = lexer.parse_punctuation("}");
            if (!success_right_curly_6)
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            var display = parse_display();

            var end = lexer.get_position();

            var ret = new ConceptSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                name = name,
                identifiers = identifiers,
                display = display,
            };

            return ret;
        }

        public ParameterSyntax parse_parameter()
        {
            var start = lexer.get_previous_position();

            var success_parameter_1 = lexer.parse_keyword("parameter");
            if (!success_parameter_1)
                    return null;

            var name = lexer.parse_identifier(keywords);
            if (name != null)
            {
                if (!is_identifier(name))
                {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
                }
            }
            else
            {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            }
            var type = parse_typespec();
            var defaultValue = parse_default();

            var end = lexer.get_position();

            var ret = new ParameterSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                name = name,
                type = type,
                defaultValue = defaultValue,
            };

            return ret;
        }

        public object[] parse_typespec_list()
        {
            List<object> list = null;
            while (true)
            {
                var node = parse_typespec();
                if (node == null)
                    break;

                if (list == null)
                    list = new List<object>();

                list.Add(node);
            }

            if (list != null)
                return list.ToArray();
            else
                return null;
        }

        public object parse_typespec()
        {
            {
                var node = parse_tuple();
                if (node != null)
                    return node;
            }
            {
                var node = parse_type();
                if (node != null)
                    return node;
            }

            return null;
        }

        public TypeSyntax parse_type()
        {
            var start = lexer.get_previous_position();

            var success_colon_1 = lexer.parse_punctuation(":");
            if (!success_colon_1)
                    return null;
            var name = parse_name();
            if (name == null)
                throw new ParserException("Unable to parse name.", new Span { file = file_name, start = new Position { line = lexer.previous_line, column = lexer.previous_column }, end = new Position { line = lexer.line, column = lexer.column } } );
            var generics = parse_genericarguments();

            var end = lexer.get_position();

            var ret = new TypeSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                name = name,
                generics = generics,
            };

            return ret;
        }

        public GenericArgumentsSyntax parse_genericarguments()
        {
            var start = lexer.get_previous_position();

            var success_left_angular_1 = lexer.parse_punctuation("<");
            if (!success_left_angular_1)
                    return null;
            var generics = parse_genericargument_list();

            var success_right_angular_3 = lexer.parse_punctuation(">");
            if (!success_right_angular_3)
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );

            var end = lexer.get_position();

            var ret = new GenericArgumentsSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                generics = generics,
            };

            return ret;
        }

        public GenericArgumentSyntax[] parse_genericargument_list()
        {
            List<GenericArgumentSyntax> list = null;
            while (true)
            {
                var node = parse_genericargument();
                if (node == null)
                    break;

                if (list == null)
                    list = new List<GenericArgumentSyntax>();

                list.Add(node);
            }

            if (list != null)
                return list.ToArray();
            else
                return null;
        }

        public GenericArgumentSyntax parse_genericargument()
        {
            var start = lexer.get_previous_position();
            var spec = parse_type();
            if (spec == null)
                return null;

            lexer.parse_punctuation(",");

            var end = lexer.get_position();

            var ret = new GenericArgumentSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                spec = spec,
            };

            return ret;
        }

        public TupleSyntax parse_tuple()
        {
            var start = lexer.get_previous_position();

            var success_left_curly_1 = lexer.parse_punctuation("{");
            if (!success_left_curly_1)
                    return null;
            var members = parse_element_list();

            var success_right_curly_3 = lexer.parse_punctuation("}");
            if (!success_right_curly_3)
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );

            var end = lexer.get_position();

            var ret = new TupleSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                members = members,
            };

            return ret;
        }

        public ElementSyntax[] parse_element_list()
        {
            List<ElementSyntax> list = null;
            while (true)
            {
                var node = parse_element();
                if (node == null)
                    break;

                if (list == null)
                    list = new List<ElementSyntax>();

                list.Add(node);
            }

            if (list != null)
                return list.ToArray();
            else
                return null;
        }

        public ElementSyntax parse_element()
        {
            var start = lexer.get_previous_position();

            var name = lexer.parse_identifier(keywords);
            if (name != null)
            {
                if (!is_identifier(name))
                {
                    return null;
                }
            }
            else
            {
                    return null;
            }
            var typeSpec = parse_typespec();
            if (typeSpec == null)
                throw new ParserException("Unable to parse typespec.", new Span { file = file_name, start = new Position { line = lexer.previous_line, column = lexer.previous_column }, end = new Position { line = lexer.line, column = lexer.column } } );

            lexer.parse_punctuation(",");

            var end = lexer.get_position();

            var ret = new ElementSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                name = name,
                typeSpec = typeSpec,
            };

            return ret;
        }

        public DefaultSyntax parse_default()
        {
            var start = lexer.get_previous_position();

            var success_default_1 = lexer.parse_keyword("default");
            if (!success_default_1)
                    return null;
            var members = parse_expression();

            var end = lexer.get_position();

            var ret = new DefaultSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                members = members,
            };

            return ret;
        }

        public object[] parse_statement_list()
        {
            List<object> list = null;
            while (true)
            {
                var node = parse_statement();
                if (node == null)
                    break;

                if (list == null)
                    list = new List<object>();

                list.Add(node);
            }

            if (list != null)
                return list.ToArray();
            else
                return null;
        }

        public object parse_statement()
        {
            {
                var node = parse_context();
                if (node != null)
                    return node;
            }
            {
                var node = parse_define();
                if (node != null)
                    return node;
            }
            {
                var node = parse_function();
                if (node != null)
                    return node;
            }

            return null;
        }

        public ContextSyntax parse_context()
        {
            var start = lexer.get_previous_position();

            var success_context_1 = lexer.parse_keyword("context");
            if (!success_context_1)
                    return null;
            var name = parse_name();
            if (name == null)
                throw new ParserException("Unable to parse name.", new Span { file = file_name, start = new Position { line = lexer.previous_line, column = lexer.previous_column }, end = new Position { line = lexer.line, column = lexer.column } } );

            var end = lexer.get_position();

            var ret = new ContextSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                name = name,
            };

            return ret;
        }

        public DefineSyntax parse_define()
        {
            var start = lexer.get_previous_position();

            var success_define_1 = lexer.parse_keyword("define");
            if (!success_define_1)
                    return null;
            var accessModifier = parse_accessmodifier();
            var definition = parse_definition();
            if (definition == null)
                throw new ParserException("Unable to parse definition.", new Span { file = file_name, start = new Position { line = lexer.previous_line, column = lexer.previous_column }, end = new Position { line = lexer.line, column = lexer.column } } );

            var end = lexer.get_position();

            var ret = new DefineSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                accessModifier = accessModifier,
                definition = definition,
            };

            return ret;
        }

        public object parse_accessmodifier()
        {
            {
                var node = parse_private();
                if (node != null)
                    return node;
            }
            {
                var node = parse_public();
                if (node != null)
                    return node;
            }

            return null;
        }

        public PrivateSyntax parse_private()
        {
            var start = lexer.get_previous_position();

            var success_private_1 = lexer.parse_keyword("private");
            if (!success_private_1)
                    return null;

            var end = lexer.get_position();

            var ret = new PrivateSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
            };

            return ret;
        }

        public PublicSyntax parse_public()
        {
            var start = lexer.get_previous_position();

            var success_public_1 = lexer.parse_keyword("public");
            if (!success_public_1)
                    return null;

            var end = lexer.get_position();

            var ret = new PublicSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
            };

            return ret;
        }

        public object[] parse_definition_list()
        {
            List<object> list = null;
            while (true)
            {
                var node = parse_definition();
                if (node == null)
                    break;

                if (list == null)
                    list = new List<object>();

                list.Add(node);
            }

            if (list != null)
                return list.ToArray();
            else
                return null;
        }

        public object parse_definition()
        {
            {
                var node = parse_item();
                if (node != null)
                    return node;
            }
            {
                var node = parse_function();
                if (node != null)
                    return node;
            }

            return null;
        }

        public ItemSyntax parse_item()
        {
            var start = lexer.get_previous_position();

            var name = lexer.parse_identifier(keywords);
            if (name != null)
            {
                if (!is_identifier(name))
                {
                    return null;
                }
            }
            else
            {
                    return null;
            }

            var success_colon_2 = lexer.parse_punctuation(":");
            if (!success_colon_2)
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            var operation = parse_operation();
            if (operation == null)
                throw new ParserException("Unable to parse operation.", new Span { file = file_name, start = new Position { line = lexer.previous_line, column = lexer.previous_column }, end = new Position { line = lexer.line, column = lexer.column } } );

            var end = lexer.get_position();

            var ret = new ItemSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                name = name,
                operation = operation,
            };

            return ret;
        }

        public FunctionSyntax parse_function()
        {
            var start = lexer.get_previous_position();

            var success_function_1 = lexer.parse_keyword("function");
            if (!success_function_1)
                    return null;

            var name = lexer.parse_identifier(keywords);
            if (name != null)
            {
                if (!is_identifier(name))
                {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
                }
            }
            else
            {
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            }

            var success_left_paren_3 = lexer.parse_punctuation("(");
            if (!success_left_paren_3)
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            var operands = parse_element_list();

            var success_right_paren_5 = lexer.parse_punctuation(")");
            if (!success_right_paren_5)
                    throw new ParserException("Unable to parse.", new Span { file = file_name, start = start, end = new Position { line = lexer.previous_line, column = lexer.previous_column } } );
            var returns = parse_returns();
            var implementation = parse_implementation();
            if (implementation == null)
                throw new ParserException("Unable to parse implementation.", new Span { file = file_name, start = new Position { line = lexer.previous_line, column = lexer.previous_column }, end = new Position { line = lexer.line, column = lexer.column } } );

            var end = lexer.get_position();

            var ret = new FunctionSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                name = name,
                operands = operands,
                returns = returns,
                implementation = implementation,
            };

            return ret;
        }

        public ReturnsSyntax parse_returns()
        {
            var start = lexer.get_previous_position();

            var success_returns_1 = lexer.parse_keyword("returns");
            if (!success_returns_1)
                    return null;
            var type = parse_typespec();
            if (type == null)
                throw new ParserException("Unable to parse typespec.", new Span { file = file_name, start = new Position { line = lexer.previous_line, column = lexer.previous_column }, end = new Position { line = lexer.line, column = lexer.column } } );

            var end = lexer.get_position();

            var ret = new ReturnsSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                type = type,
            };

            return ret;
        }

        public object parse_implementation()
        {
            {
                var node = parse_operation();
                if (node != null)
                    return node;
            }
            {
                var node = parse_external();
                if (node != null)
                    return node;
            }

            return null;
        }

        public ExternalSyntax parse_external()
        {
            var start = lexer.get_previous_position();

            var success_external_1 = lexer.parse_keyword("external");
            if (!success_external_1)
                    return null;

            var end = lexer.get_position();

            var ret = new ExternalSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
            };

            return ret;
        }

        public OperationSyntax parse_operation()
        {
            var start = lexer.get_previous_position();
            var operands = parse_operand_list();
            if (operands == null)
                return null;

            lexer.parse_colon();

            var end = lexer.get_position();

            var ret = new OperationSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                operands = operands,
            };

            return ret;
        }

        public OperandSyntax[] parse_operand_list()
        {
            List<OperandSyntax> list = null;
            while (true)
            {
                var node = parse_operand();
                if (node == null)
                    break;

                if (list == null)
                    list = new List<OperandSyntax>();

                list.Add(node);
            }

            if (list != null)
                return list.ToArray();
            else
                return null;
        }

        public OperandSyntax parse_operand()
        {
            var start = lexer.get_previous_position();
            var expression = parse_expression();
            if (expression == null)
                return null;
            var postfixes = parse_memberaccess_list();

            var end = lexer.get_position();

            var ret = new OperandSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                expression = expression,
                postfixes = postfixes,
            };

            return ret;
        }

        public MemberAccessSyntax[] parse_memberaccess_list()
        {
            List<MemberAccessSyntax> list = null;
            while (true)
            {
                var node = parse_memberaccess();
                if (node == null)
                    break;

                if (list == null)
                    list = new List<MemberAccessSyntax>();

                list.Add(node);
            }

            if (list != null)
                return list.ToArray();
            else
                return null;
        }

        public MemberAccessSyntax parse_memberaccess()
        {
            var start = lexer.get_previous_position();

            var success_dot_1 = lexer.parse_punctuation(".");
            if (!success_dot_1)
                    return null;
            var member = parse_name();
            if (member == null)
                throw new ParserException("Unable to parse name.", new Span { file = file_name, start = new Position { line = lexer.previous_line, column = lexer.previous_column }, end = new Position { line = lexer.line, column = lexer.column } } );

            var end = lexer.get_position();

            var ret = new MemberAccessSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                member = member,
            };

            return ret;
        }

        public object parse_expression()
        {
            {
                var node = parse_literal();
                if (node != null)
                    return node;
            }
            {
                var node = parse_name();
                if (node != null)
                    return node;
            }

            return null;
        }

        public LiteralSyntax parse_literal()
        {
            var start = lexer.get_previous_position();

            var literal = lexer.parse_literal();
            if (literal == null)
            {
                    return null;
            }

            var end = lexer.get_position();

            var ret = new LiteralSyntax
            {
                span = new Span
                {
                    file = file_name,
                    start = start,
                    end = end
                },
                literal = literal,
            };

            return ret;
        }

        public bool is_at_end()
        {
            return lexer.is_at_end();
        }

        public ulong get_current_line()
        {
            return lexer.get_position().line;
        }

        public ulong get_current_column()
        {
            return lexer.get_position().column;
        }

        public ulong get_previous_line()
        {
            return lexer.get_previous_position().line;
        }

        public ulong get_previous_column()
        {
            return lexer.get_previous_position().column;
        }

        bool is_identifier(string id)
        {
            if (keywords.Contains(id))
                return false;
            return true;
        }
    }

    public class ProgramSyntax
    {
        public Span span;
        public FileSyntax[] files;
    }

    public class FileSyntax
    {
        public Span span;
        public LibrarySyntax library;
        public UsingSyntax[] usings;
        public IncludeSyntax[] includes;
        public object[] definitions;
    }

    public class LibrarySyntax
    {
        public Span span;
        public NameSyntax name;
        public VersionSyntax version;
    }

    public class NameSyntax
    {
        public Span span;
        public string name;
        public ExtensionSyntax[] extensions;
    }

    public class ExtensionSyntax
    {
        public Span span;
        public string name;
    }

    public class VersionSyntax
    {
        public Span span;
        public Literal name;
    }

    public class UsingSyntax
    {
        public Span span;
        public string model;
        public VersionSyntax version;
    }

    public class IncludeSyntax
    {
        public Span span;
        public NameSyntax name;
        public VersionSyntax version;
        public CalledSyntax called;
    }

    public class CalledSyntax
    {
        public Span span;
        public Literal name;
    }

    public class PrivateDeclarationSyntax
    {
        public Span span;
        public object declaration;
    }

    public class PublicDeclarationSyntax
    {
        public Span span;
        public object export;
    }

    public class CodeSystemSyntax
    {
        public Span span;
        public string name;
        public Literal id;
        public VersionSyntax version;
    }

    public class ValueSetSyntax
    {
        public Span span;
        public string name;
        public Literal id;
        public VersionSyntax version;
        public CodeSystemSyntax codeSystems;
    }

    public class CodeSystemsSyntax
    {
        public Span span;
        public CodeIdentifierSyntax[] identifiers;
    }

    public class CodeIdentifierSyntax
    {
        public Span span;
        public string identifier;
        public ExtensionSyntax extension;
    }

    public class CodeSyntax
    {
        public Span span;
        public string name;
        public Literal id;
        public CodeIdentifierSyntax version;
        public DisplaySyntax display;
    }

    public class DisplaySyntax
    {
        public Span span;
        public Literal id;
    }

    public class ConceptSyntax
    {
        public Span span;
        public string name;
        public CodeIdentifierSyntax[] identifiers;
        public DisplaySyntax display;
    }

    public class ParameterSyntax
    {
        public Span span;
        public string name;
        public object type;
        public DefaultSyntax defaultValue;
    }

    public class TypeSyntax
    {
        public Span span;
        public NameSyntax name;
        public GenericArgumentsSyntax generics;
    }

    public class GenericArgumentsSyntax
    {
        public Span span;
        public GenericArgumentSyntax[] generics;
    }

    public class GenericArgumentSyntax
    {
        public Span span;
        public TypeSyntax spec;
    }

    public class TupleSyntax
    {
        public Span span;
        public ElementSyntax[] members;
    }

    public class ElementSyntax
    {
        public Span span;
        public string name;
        public object typeSpec;
    }

    public class DefaultSyntax
    {
        public Span span;
        public object members;
    }

    public class ContextSyntax
    {
        public Span span;
        public NameSyntax name;
    }

    public class DefineSyntax
    {
        public Span span;
        public object accessModifier;
        public object definition;
    }

    public class PrivateSyntax
    {
        public Span span;
    }

    public class PublicSyntax
    {
        public Span span;
    }

    public class ItemSyntax
    {
        public Span span;
        public string name;
        public OperationSyntax operation;
    }

    public class FunctionSyntax
    {
        public Span span;
        public string name;
        public ElementSyntax[] operands;
        public ReturnsSyntax returns;
        public object implementation;
    }

    public class ReturnsSyntax
    {
        public Span span;
        public object type;
    }

    public class ExternalSyntax
    {
        public Span span;
    }

    public class OperationSyntax
    {
        public Span span;
        public OperandSyntax[] operands;
    }

    public class OperandSyntax
    {
        public Span span;
        public object expression;
        public MemberAccessSyntax[] postfixes;
    }

    public class MemberAccessSyntax
    {
        public Span span;
        public NameSyntax member;
    }

    public class LiteralSyntax
    {
        public Span span;
        public Literal literal;
    }
}
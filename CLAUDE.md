# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Caquelon is a .NET library (`Fire.Cql`) implementing HL7 Clinical Quality Language (CQL). It parses CQL expressions using an ANTLR-generated parser and represents them using the official HL7 Expression Logical Model (ELM).

## Build and Test Commands

```bash
# Build everything
dotnet build

# Run all tests
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~Fire.Cql.Tests.CqlTypesTest"

# Run a single test method
dotnet test --filter "FullyQualifiedName~Fire.Cql.Tests.CqlTypesTest.Quantity"
```

## Architecture

### Solution Structure

- `src/Fire.Cql/` — Main library (targets net10.0, depends on Antlr4.Runtime.Standard)
- `test/Fire.Cql.Tests/` — xUnit test project

### Generated Code (do not edit by hand)

- **ANTLR parser** (`src/Fire.Cql/Grammar/`): `cqlLexer.cs`, `cqlParser.cs`, `cqlVisitor.cs`, `cqlBaseVisitor.cs` are generated from `cql.g4` (which imports `fhirpath.g4`). The `.g4` grammars are the source of truth.
- **ELM model** (`src/Fire.Cql/Elm/Generated/`): C# classes generated from the XSD schemas in `src/Fire.Cql/Elm/schema/` using `xscgen`. The generation command is documented in the header of `Fire.Cql.Elm.cs`.

### Test Codegen Pipeline

Test `.cs` files are generated from XML test definitions using DSSSL (an SGML stylesheet language):

1. **XML test suites** (`test/Fire.Cql.Tests/*.xml`): Official HL7 CQL test cases. `tests.xml` is the master document that includes all individual test XML files via SGML entity references.
2. **DSSSL stylesheet** (`test/Fire.Cql.Tests/codegen/test.dsl`): Transforms the XML into xUnit C# test classes. Composed of `utilities.scm` (string helpers), `rules.scm` (element rules), and `module.scm` (C# template).
3. **SGML declaration** (`test/Fire.Cql.Tests/xml.dcl`): Enables DSSSL processing of the XML test files.

The generated test classes all call `Helpers.CheckBool()` which is the single integration point — it takes a CQL expression string, should parse/build ELM/evaluate it, and return a boolean result.

## Commit Policy

- Single-line commit messages only — no multi-line descriptions, no co-authorship tags
- No marketing language — be factual and direct

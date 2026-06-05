using System;
using System.IO;
using LINVAST.Builders;
using LINVAST.Exceptions;
using LINVAST.Imperative;
using LINVAST.Nodes;

namespace LINVAST.Cli
{
    internal static class AstInput
    {
        public static ASTNode Build(string source, string? language)
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("Input source is required.", nameof(source));

            string? extension = NormalizeLanguage(language);
            if (source == "-") {
                if (extension is null)
                    throw new ArgumentException("Reading from stdin requires --language.");

                string code = Console.In.ReadToEnd();
                return BuildFromSource(code, extension);
            }

            if (!File.Exists(source))
                throw new FileNotFoundException("Input file does not exist.", source);

            if (extension is not null)
                return BuildFromSource(File.ReadAllText(source), extension);

            return new ImperativeASTFactory().BuildFromFile(source);
        }

        private static ASTNode BuildFromSource(string code, string extension)
        {
            IAbstractASTBuilder builder = extension switch
            {
                ".c" => new Imperative.Builders.C.CASTBuilder(),
                ".go" => new Imperative.Builders.Go.GoASTBuilder(),
                ".java" => new Imperative.Builders.Java.JavaASTBuilder(),
                ".lua" => new Imperative.Builders.Lua.LuaASTBuilder(),
                ".psc" => new Imperative.Builders.Pseudo.PseudoASTBuilder(),
                _ => throw new UnsupportedLanguageException(),
            };

            return builder.BuildFromSource(code);
        }

        private static string? NormalizeLanguage(string? language)
        {
            if (string.IsNullOrWhiteSpace(language))
                return null;

            string normalized = language.Trim().TrimStart('.').ToLowerInvariant();
            return normalized switch
            {
                "c" => ".c",
                "go" or "golang" => ".go",
                "java" => ".java",
                "lua" => ".lua",
                "psc" or "pseudo" => ".psc",
                _ => "." + normalized,
            };
        }
    }
}

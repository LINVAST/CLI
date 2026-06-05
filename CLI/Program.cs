using System;
using System.IO;
using CommandLine;
using LINVAST.Exceptions;
using LINVAST.Nodes;
using Serilog;
using Serilog.Events;

namespace LINVAST.Cli
{
    internal static class Program
    {
        internal static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<CompareOptions, ASTOptions>(args)
                .MapResult(
                    (CompareOptions o) => CompareSources(o),
                    (ASTOptions o) => GenerateAST(o),
                    errs => 1
                );
        }


        private static int GenerateAST(ASTOptions o)
        {
            SetupLogger(o.Verbose, o.Quiet);

            if (string.IsNullOrWhiteSpace(o.Source)) {
                Log.Fatal("Missing source path");
                return 1;
            }

            if (!TryBuild(o.Source, o.Language, out ASTNode? ast))
                return 1;
            if (ast is null)
                return 1;

            Log.Debug("AST created");

            string output;
            switch (o.Format.Trim().ToLowerInvariant()) {
                case "json":
                    Log.Debug("Generating JSON...");
                    output = ast.ToJson(o.Compact);
                    break;
                case "text":
                    output = ast.GetText();
                    break;
                default:
                    Log.Fatal("Unsupported output format '{Format}'. Use 'json' or 'text'.", o.Format);
                    return 1;
            }

            if (o.Stats)
                Console.Error.WriteLine(AstStatistics.Describe(ast));

            if (!TryWriteOutput(o.OutputPath, output))
                return 1;

            return 0;
        }

        private static int CompareSources(CompareOptions o)
        {
            SetupLogger(o.Verbose, o.Quiet);

            if (string.IsNullOrWhiteSpace(o.Source) || string.IsNullOrWhiteSpace(o.Destination)) {
                Log.Fatal("Missing source/destination path");
                return 1;
            }

            if (o.Source == "-" && o.Destination == "-") {
                Log.Fatal("Only one compare input can be read from stdin.");
                return 1;
            }

            string? sourceLanguage = o.SourceLanguage ?? o.Language;
            string? destinationLanguage = o.DestinationLanguage ?? o.Language;
            if (!TryBuild(o.Source, sourceLanguage, out ASTNode? src) || !TryBuild(o.Destination, destinationLanguage, out ASTNode? dst))
                return 1;
            if (src is null || dst is null)
                return 1;

            if (!AstComparer.HaveSameStructure(src, dst, out string? structureError)) {
                Log.Fatal("Structure match failed: {Details}", structureError);
                return 1;
            }

            if (!AstComparer.AreEqual(src, dst, out string? compareError)) {
                Log.Fatal("AST comparison failed: {Details}", compareError);
                return 1;
            }

            Log.Information("ASTs match.");

            return 0;
        }

        private static bool TryBuild(string source, string? language, out ASTNode? ast)
        {
            Log.Information("Creating AST for input: {Source}", source == "-" ? "stdin" : source);

            ast = null;
            try {
                ast = AstInput.Build(source, language);
                return true;
            } catch (SyntaxErrorException e) {
                Log.Fatal(e, "[{Source}] Syntax error: {Details}", source, e.Message ?? "unknown");
            } catch (NotImplementedException e) {
                Log.Fatal(e, "[{Source}] Not supported: {Details}", source, e.Message ?? "unknown");
            } catch (UnsupportedLanguageException e) {
                Log.Fatal(e, "[{Source}] Unsupported language. Use a supported file extension or pass --language.", source);
            } catch (FileNotFoundException e) {
                Log.Fatal(e, "Input file does not exist: {Path}", e.FileName ?? source);
            } catch (IOException e) {
                Log.Fatal(e, "[{Source}] I/O error: {Details}", source, e.Message);
            } catch (ArgumentException e) {
                Log.Fatal(e, "[{Source}] {Details}", source, e.Message);
            } catch (Exception e) {
                Log.Fatal(e, "[{Source}] Exception occurred", source);
            }

            return false;
        }

        private static bool TryWriteOutput(string? path, string content)
        {
            if (string.IsNullOrWhiteSpace(path)) {
                Console.WriteLine(content);
                return true;
            }

            try {
                string? directory = Path.GetDirectoryName(Path.GetFullPath(path));
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(path, content);
                Log.Information("Wrote output to {Path}", path);
                return true;
            } catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException) {
                Log.Fatal(e, "Failed to save output to file {Path}", path);
                return false;
            }
        }

        private static void SetupLogger(bool verbose, bool quiet)
        {
            var lcfg = new LoggerConfiguration()
                .WriteTo.Console(
                    standardErrorFromLevel: LogEventLevel.Verbose,
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .Enrich.FromLogContext()
                ;

            if (quiet)
                lcfg.MinimumLevel.Error();
            else if (verbose)
                lcfg.MinimumLevel.Verbose();
            else
                lcfg.MinimumLevel.Warning();

            Log.Logger = lcfg.CreateLogger();
        }
    }
}

using CommandLine;

namespace LINVAST.Cli
{
    [Verb("cmp", HelpText = "Compare source against the specification source")]
    internal sealed class CompareOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages")]
        public bool Verbose { get; set; }

        [Option('q', "quiet", Required = false, HelpText = "Only write errors")]
        public bool Quiet { get; set; }

        [Option('l', "language", Required = false, HelpText = "Language/extension for both inputs when reading from stdin or overriding file extensions")]
        public string? Language { get; set; }

        [Option("source-language", Required = false, HelpText = "Language/extension for the specification input")]
        public string? SourceLanguage { get; set; }

        [Option("destination-language", Required = false, HelpText = "Language/extension for the test input")]
        public string? DestinationLanguage { get; set; }

        [Value(0, Required = true, HelpText = "Specification path")]
        public string? Source { get; set; }

        [Value(1, Required = true, HelpText = "Test source path")]
        public string? Destination { get; set; }
    }
    
    [Verb("ast", HelpText = "AST generation commands")]
    internal sealed class ASTOptions
    {
        [Option('v', "verbose", Default = false, Required = false, HelpText = "Verbose output")]
        public bool Verbose { get; set; }

        [Option('q', "quiet", Required = false, HelpText = "Only write errors")]
        public bool Quiet { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output path")]
        public string? OutputPath { get; set; }

        [Option('l', "language", Required = false, HelpText = "Language/extension when reading from stdin or overriding the source extension")]
        public string? Language { get; set; }

        [Option('f', "format", Default = "json", Required = false, HelpText = "Output format: json or text")]
        public string Format { get; set; } = "json";
        
        [Option('c', "compact", Default = false, Required = false, HelpText = "Compact AST output")]
        public bool Compact { get; set; }

        [Option("stats", Default = false, Required = false, HelpText = "Write AST node statistics to stderr")]
        public bool Stats { get; set; }

        [Value(0, Required = true, HelpText = "Source path")]
        public string? Source { get; set; }
    }
}

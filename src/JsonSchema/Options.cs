// Copyright (c) Umbraco.
// See LICENSE for more details.

using CommandLine;

namespace JsonSchema
{
    internal class Options
    {
        [Option('o', "outputFile", Required = false, HelpText = "Set path of the output file.", Default = "appsettings-schema.json")]
        public string OutputFile { get; set; } = null!;
    }
}

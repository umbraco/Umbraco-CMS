using CommandLine;

internal class Options
{
    [Option("outputFile", Default = "appsettings-schema.Umbraco.Cms.json", HelpText = "Output file to save the generated JSON schema for Umbraco CMS.")]
    public required string OutputFile { get; set; }
}

using CommandLine;

internal class Options
{
    [Option('a', "assembly", Required = true, HelpText = "Assembly file (DLL) to load, get the specified type and generate the JSON schema from.")]
    public string AssemblyFilePath { get; set; } = null!;

    [Option('t', "type", Required = true, HelpText = "Type name (including namespace) to generate the JSON schema from.")]
    public string TypeName { get; set; } = null!;

    [Option('o', "output", Required = true, HelpText = "Output file path to write the generated JSON schema to.")]
    public string OutputFilePath { get; set; } = null!;
}

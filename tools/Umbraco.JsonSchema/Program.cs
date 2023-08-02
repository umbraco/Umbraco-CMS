using CommandLine;

await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(async options =>
{
    // Load assembly and get type
    var assemblyFile = Path.GetFullPath(options.AssemblyFilePath);
    if (!File.Exists(assemblyFile))
    {
        throw new FileNotFoundException($"Could not find file '{assemblyFile}'.", assemblyFile);
    }

    var plc = new PluginLoadContext(assemblyFile);
    var assembly = plc.LoadFromAssemblyPath(assemblyFile);
    var type = assembly.GetType(options.TypeName, true);

    // Generate schema
    var jsonSchemaGenerator = new UmbracoJsonSchemaGenerator();
    var jsonSchema = jsonSchemaGenerator.Generate(type);

    await File.WriteAllTextAsync(options.OutputFilePath, jsonSchema.ToJson());
});

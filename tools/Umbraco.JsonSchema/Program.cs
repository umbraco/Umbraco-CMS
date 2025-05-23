using CommandLine;

await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(async options =>
{
    // Generate CMS schema
    var jsonSchemaGenerator = new UmbracoJsonSchemaGenerator();
    NJsonSchema.JsonSchema jsonSchema = jsonSchemaGenerator.Generate(typeof(UmbracoCmsSchema));

    await File.WriteAllTextAsync(options.OutputFile, jsonSchema.ToJson());
});

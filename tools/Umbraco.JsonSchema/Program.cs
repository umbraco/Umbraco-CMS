using CommandLine;
using Umbraco.JsonSchema.Core;

await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(async options =>
{
    // Generate CMS schema
    var jsonSchemaGenerator = new UmbracoJsonSchemaGenerator();
    var jsonSchema = jsonSchemaGenerator.Generate(typeof(UmbracoCmsSchema));

    await File.WriteAllTextAsync(options.OutputFile, jsonSchema.ToJson());
});

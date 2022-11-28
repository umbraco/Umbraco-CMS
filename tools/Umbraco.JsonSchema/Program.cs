using CommandLine;
using Umbraco.Cms.Core.Configuration.Models;

await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(async options =>
{
    // Generate CMS schema
    var jsonSchemaGenerator = new UmbracoJsonSchemaGenerator();
    var jsonSchema = jsonSchemaGenerator.Generate(typeof(UmbracoCmsSchema));

    // TODO: When the UmbracoPath setter is removed from GlobalSettings (scheduled for V12), remove this line as well
    jsonSchema.Definitions[nameof(GlobalSettings)]?.Properties?.Remove(nameof(GlobalSettings.UmbracoPath));

    await File.WriteAllTextAsync(options.OutputFile, jsonSchema.ToJson());
});

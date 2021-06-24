using System;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NJsonSchema;
using NJsonSchema.Generation;

namespace JsonSchema
{
    class Program
    {
        public class Options
        {
            [Option('o', "outputFile", Required = false, HelpText = "Set path of the output file.")]
            public string OutputFile { get; set; }
        }

        static async Task Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsedAsync<Options>(Execute);
        }

        private static async Task Execute(Options options)
        {
            var result = GenerateJsonSchema();

            if (string.IsNullOrEmpty(options.OutputFile))
            {
                Console.WriteLine(result);
            }
            else
            {
                await System.IO.File.WriteAllTextAsync(options.OutputFile, result);
            }
        }

        private static string GenerateJsonSchema()
        {
            var settings = new JsonSchemaGeneratorSettings()
            {
                SchemaType = SchemaType.JsonSchema,
                AlwaysAllowAdditionalObjectProperties = true,
                SerializerSettings = new JsonSerializerSettings(),
                TypeNameGenerator = new UmbracoPrefixedTypeNameGenerator()
            };
            settings.SerializerSettings.Converters.Add(new StringEnumConverter());

            var generator = new JsonSchemaGenerator(settings);

            var schema = generator.Generate(typeof(UmbracoCmsConfigRoot));

            return schema.ToJson(Formatting.Indented);
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Schema;
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
            [Option('o', "outputFile", Required = false, HelpText = "Set path of the output file.", Default = "../../../../Umbraco.Web.UI.NetCore/umbraco/config/appsettings-schema.json")]
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

            var path = Path.Combine(Environment.CurrentDirectory, options.OutputFile);
            await File.WriteAllTextAsync(path, result);


            Console.WriteLine("File written at " + path);
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

            var schema = generator.Generate(typeof(AppSettings));

            return schema.ToJson(Formatting.Indented);
        }
    }
}

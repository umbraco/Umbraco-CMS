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
            try
            {
                Parser.Default.ParseArguments<Options>(args)
                    .WithParsedAsync<Options>(Execute);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private static async Task Execute(Options options)
        {
            var generator = new UmbracoJsonSchemaGenerator();
            var schema = await generator.Generate();

            var path = Path.Combine(Environment.CurrentDirectory, options.OutputFile);
            await File.WriteAllTextAsync(path, schema);

            Console.WriteLine("File written at " + path);
        }
    }
}

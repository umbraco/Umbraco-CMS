// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using System.Threading.Tasks;
using CommandLine;

namespace JsonSchema
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                await Parser.Default.ParseArguments<Options>(args)
                    .WithParsedAsync(Execute);
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

            var cmsSchema = await generator.GenerateCmsFile();
            await WriteSchemaToFile(cmsSchema, options.CmsOutputFile);

            var schema = await generator.GenerateMainFile();
            await WriteSchemaToFile(schema, options.MainOutputFile);
        }

        private static async Task WriteSchemaToFile(string schema,  string filePath)
        {
            var mainPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, filePath));
            Console.WriteLine("Path to use {0}", mainPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath)!);
            Console.WriteLine("Ensured directory exists");
            await File.WriteAllTextAsync(mainPath, schema);

            Console.WriteLine("File written at {0}", mainPath);
        }
    }
}

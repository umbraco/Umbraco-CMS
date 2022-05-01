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
            var schema = await generator.Generate();

            var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, options.OutputFile));
            Console.WriteLine("Path to use {0}", path);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            Console.WriteLine("Ensured directory exists");
            await File.WriteAllTextAsync(path, schema);

            Console.WriteLine("File written at {0}", path);
        }
    }
}

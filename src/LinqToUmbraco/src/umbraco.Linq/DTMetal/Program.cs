using System;
using System.Reflection;
using umbraco.Linq.DTMetal.Engine;
using System.IO;

namespace umbraco.Linq.DTMetal
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("umbraco Presenents - LINQ to umbraco Generator version " + Assembly.GetEntryAssembly().GetName().Version);
            Console.WriteLine("Copyright (C) umbraco 2009." + Environment.NewLine);

            ArgsParser parsedArgs;
            try
            {
                parsedArgs = new ArgsParser(args);
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine(Strings.ArgumentMissing, ex.ParamName);
                Console.WriteLine();
                ShowHelp();
                return;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                ShowHelp();
                return;
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                ShowHelp();
                return;
            }

            switch (parsedArgs.Mode)
            {
                case RunMode.Xml:
                    DTMLGenerator gen = new DTMLGenerator(parsedArgs.OutputFilePath, parsedArgs.ConnectionString, parsedArgs.DataContextName, parsedArgs.DisablePluralization);
                    gen.GenerateDTMLFile();
                    break;
                case RunMode.Class:
                    CodeBuilder.ClassGenerator builder = CodeBuilder.ClassGenerator.CreateBuilder(parsedArgs.DtmlPath, parsedArgs.Namespace, parsedArgs.Language);
                    builder.GenerateCode();
                    builder.Save();
                    break;
                case RunMode.Help:
                    ShowHelp();
                    break;
                default:
                    break;
            }
            

        }

        private static void ShowHelp()
        {
            string file = Assembly.GetEntryAssembly().GetName().Name + ".exe";
            Console.WriteLine("Usage: {0} [options]", file);
            Console.WriteLine();

            Console.WriteLine("Syntax:");
            Console.WriteLine("\t\t-mode:[{xml}:{class}]");
            Console.WriteLine();
            Console.WriteLine("\tTo generate the DTML");
            Console.WriteLine("\t\t-connectionString:{umbraco Connection String}");
            Console.WriteLine("\t\t-output:{output file path} (optional)");
            Console.WriteLine("\t\t-dataContext:{DataContext name} (optional)");
            Console.WriteLine("\t\t-disablePluralization (optional. Default \"false\"");
            Console.WriteLine();
            Console.WriteLine("\tTo generate classes");
            Console.WriteLine("\t\t/dtml:{Path to DTML file for generation}");
            Console.WriteLine("\t\t/language:[{csharp}:{vb}] (optional. Default C#)");
            Console.WriteLine("\t\t/namespace:{Namespace for generated types} (optional. Default \"umbraco\")");

            Console.WriteLine("Samples:");
            Console.WriteLine(@"{0} -mode:xml -output:""c:\temp\"" -connectionString:""server=.\sqlexpress;database=MyumbracoSite;user id=my_user;password=my_password"" -dataContext:Myumbraco", file);
            Console.WriteLine(@"{0} /mode:class /language:csharp /dtml:""C:\Users\apowell\Documents\umbraco\Codeplex\umbraco\branches\try-umbraco-linq\trunk\src\umbraco.Linq\DTMetal\bin\Debug\Myumbraco.dtml""", file);
        }
    }
}

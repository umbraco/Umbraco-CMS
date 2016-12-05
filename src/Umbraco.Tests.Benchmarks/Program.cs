using System;
using System.Reflection;
using BenchmarkDotNet.Running;

namespace Umbraco.Tests.Benchmarks
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                var type = Assembly.GetExecutingAssembly().GetType("Umbraco.Tests.Benchmarks." +args[0]);
                if (type == null)
                {
                    Console.WriteLine("Unknown benchmark.");
                }
                else
                {
                    var summary = BenchmarkRunner.Run(type);
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine("?");
            }
        }
    }
}

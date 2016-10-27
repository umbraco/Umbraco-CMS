using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace Umbraco.Tests.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ModelToSqlExpressionHelperBenchmarks>();
            //var summary = BenchmarkRunner.Run<BulkInsertBenchmarks>();

            Console.ReadLine();
        }
    }
}

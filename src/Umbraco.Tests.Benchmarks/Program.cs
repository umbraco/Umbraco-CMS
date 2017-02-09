using BenchmarkDotNet.Running;

namespace Umbraco.Tests.Benchmarks
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var switcher = new BenchmarkSwitcher(new[]
            {
                typeof(BulkInsertBenchmarks),
                typeof(ModelToSqlExpressionHelperBenchmarks),
                typeof(XmlBenchmarks),
                typeof(LinqCastBenchmarks),
                //typeof(DeepCloneBenchmarks),
                typeof(XmlPublishedContentInitBenchmarks),

            });
            switcher.Run(args);            
        }
    }
}

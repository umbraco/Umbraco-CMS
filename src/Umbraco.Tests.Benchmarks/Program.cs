using BenchmarkDotNet.Running;

namespace Umbraco.Tests.Benchmarks
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            new BenchmarkSwitcher(typeof(Program).Assembly).Run(args);
        }
    }
}

using BenchmarkDotNet.Running;

namespace Umbraco.Tests.IntegrationBenchmarks;

internal static class Program
{
    private static void Main(string[] args) => new BenchmarkSwitcher(typeof(Program).Assembly).Run(args);
}

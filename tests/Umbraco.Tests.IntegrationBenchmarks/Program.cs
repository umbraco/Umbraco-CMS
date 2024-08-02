using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Umbraco.Tests.IntegrationBenchmarks;

internal static class Program
{
    private static void Main(string[] args) => new BenchmarkSwitcher(typeof(Program).Assembly).Run(args, SetupBenchMarkConfig());

    //used to test the setup
    // private static void Main(string[] args)
    // {
    //     var test = new HybridCacheBenchmark();
    //     test.BenchmarkSetup();
    //     test.GetCachedCanAssignNoFactory();
    //     test.BenchmarkCleanup();
    // }

    private static IConfig SetupBenchMarkConfig() =>
        ManualConfig.Create(DefaultConfig.Instance)
            .WithOptions(ConfigOptions.DisableOptimizationsValidator);
}

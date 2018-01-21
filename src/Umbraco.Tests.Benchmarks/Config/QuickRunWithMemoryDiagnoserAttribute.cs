using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;

namespace Umbraco.Tests.Benchmarks.Config
{
    /// <summary>
    /// Configures the benchmark to run with less warmup and a shorter iteration time than the standard benchmark.
    /// Memory usage diagnosis is included in the benchmark
    /// </summary>
    public class QuickRunWithMemoryDiagnoserAttribute : QuickRunAttribute
    {
        public QuickRunWithMemoryDiagnoserAttribute()
        {
            ((ManualConfig)this.Config).Add(new MemoryDiagnoser());
        }
    }
}
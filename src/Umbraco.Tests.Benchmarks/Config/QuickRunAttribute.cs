using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using System;

namespace Umbraco.Tests.Benchmarks.Config
{
    /// <summary>
    /// Configures the benchmark to run with less warmup and a shorter iteration time than the standard benchmark.
    /// </summary>
    public class QuickRunAttribute : Attribute, IConfigSource
    {
        public QuickRunAttribute()
        {
            Config = ManualConfig.CreateEmpty()
                .With(Job.Default.WithLaunchCount(1) // benchmark process will be launched only once
                .WithIterationTime(new TimeInterval(100, TimeUnit.Millisecond)) // 100ms per iteration
                .WithWarmupCount(3) // 3 warmup iteration
                .WithTargetCount(3)); // 3 target iteration
        }

        /// <inheritdoc/>
        public IConfig Config { get; }
    }
}
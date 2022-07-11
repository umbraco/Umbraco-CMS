using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Perfolizer.Horology;

namespace Umbraco.Tests.Benchmarks.Config;

/// <summary>
///     Configures the benchmark to run with less warmup and a shorter iteration time than the standard benchmark.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class QuickRunConfigAttribute : Attribute, IConfigSource
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="QuickRunConfigAttribute" /> class.
    /// </summary>
    public QuickRunConfigAttribute() =>
        Config = (ManualConfig)ManualConfig.CreateEmpty()
            .With(Job.Default.WithLaunchCount(1) // benchmark process will be launched only once
                .WithIterationTime(new TimeInterval(100, TimeUnit.Millisecond)) // 100ms per iteration
                .WithWarmupCount(3) // 3 warmup iteration
                .WithIterationCount(3)); // 3 target iteration

    /// <summary>
    ///     Gets the manual configuration.
    /// </summary>
    protected ManualConfig Config { get; }

    /// <inheritdoc />
    IConfig IConfigSource.Config => Config;
}

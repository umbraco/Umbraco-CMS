using BenchmarkDotNet.Diagnosers;

namespace Umbraco.Tests.Benchmarks.Config;

/// <summary>
///     Configures the benchmark to run with less warmup and a shorter iteration time than the standard benchmark,
///     and include memory usage diagnosis.
/// </summary>
public class QuickRunWithMemoryDiagnoserConfigAttribute : QuickRunConfigAttribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="QuickRunWithMemoryDiagnoserConfigAttribute" /> class.
    /// </summary>
    public QuickRunWithMemoryDiagnoserConfigAttribute() => Config.Add(MemoryDiagnoser.Default);
}

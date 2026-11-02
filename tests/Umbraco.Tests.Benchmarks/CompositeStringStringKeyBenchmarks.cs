using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;
using Umbraco.Cms.Core.Collections;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks;

/// <summary>
///     Compares <see cref="CompositeStringStringKey" /> against <see cref="OldCompositeStringStringKey" />, the
///     implementation that lower-cased both parts on construction, on the culture/segment lookups performed by
///     <c>PublishedProperty</c>.
/// </summary>
/// <remarks>
///     <para>
///         The pairs are chosen to show where the two differ: <see cref="string.ToLowerInvariant" /> returns the
///         same instance when a part is already lower-case ASCII, so the baseline only allocates for parts that
///         actually contain an upper-case character - such as the conventional <c>en-US</c> culture form.
///     </para>
/// </remarks>
[QuickRunWithMemoryDiagnoserConfig]
public class CompositeStringStringKeyBenchmarks
{
    private const string Culture = "en-US";
    private const string Segment = "default";

    private const string LowerCaseCulture = "en-us";

    private const string CultureMixedCase = "EN-us";
    private const string SegmentMixedCase = "DEFAULT";

    private const string SomeValue = "property value";

    private ConcurrentDictionary<OldCompositeStringStringKey, string> _oldDictionary = null!;
    private ConcurrentDictionary<CompositeStringStringKey, string> _newDictionary = null!;

    [GlobalSetup]
    public void Setup()
    {
        _oldDictionary = new ConcurrentDictionary<OldCompositeStringStringKey, string>
        {
            [new OldCompositeStringStringKey(Culture, Segment)] = SomeValue,
        };

        _newDictionary = new ConcurrentDictionary<CompositeStringStringKey, string>
        {
            [new CompositeStringStringKey(Culture, Segment)] = SomeValue,
        };
    }

    // --- Construction ---

    [Benchmark(Baseline = true, Description = "ToLowerInvariant: construct key")]
    public OldCompositeStringStringKey Old_Construct()
        => new(Culture, Segment);

    [Benchmark(Description = "OrdinalIgnoreCase: construct key")]
    public CompositeStringStringKey New_Construct()
        => new(Culture, Segment);

    // --- Dictionary lookup, same casing as the stored key ---

    [Benchmark(Description = "ToLowerInvariant: TryGetValue (same case)")]
    public bool Old_TryGetValue_SameCase()
        => _oldDictionary.TryGetValue(new OldCompositeStringStringKey(Culture, Segment), out _);

    [Benchmark(Description = "OrdinalIgnoreCase: TryGetValue (same case)")]
    public bool New_TryGetValue_SameCase()
        => _newDictionary.TryGetValue(new CompositeStringStringKey(Culture, Segment), out _);

    // --- Dictionary lookup, different casing from the stored key ---

    [Benchmark(Description = "ToLowerInvariant: TryGetValue (mixed case)")]
    public bool Old_TryGetValue_MixedCase()
        => _oldDictionary.TryGetValue(new OldCompositeStringStringKey(CultureMixedCase, SegmentMixedCase), out _);

    [Benchmark(Description = "OrdinalIgnoreCase: TryGetValue (mixed case)")]
    public bool New_TryGetValue_MixedCase()
        => _newDictionary.TryGetValue(new CompositeStringStringKey(CultureMixedCase, SegmentMixedCase), out _);

    // --- Dictionary lookup, parts already lower-case (the baseline's non-allocating path) ---

    [Benchmark(Description = "ToLowerInvariant: TryGetValue (already lower-case)")]
    public bool Old_TryGetValue_LowerCase()
        => _oldDictionary.TryGetValue(new OldCompositeStringStringKey(LowerCaseCulture, Segment), out _);

    [Benchmark(Description = "OrdinalIgnoreCase: TryGetValue (already lower-case)")]
    public bool New_TryGetValue_LowerCase()
        => _newDictionary.TryGetValue(new CompositeStringStringKey(LowerCaseCulture, Segment), out _);

    // The construction rows are not comparable: BenchmarkDotNet reports ZeroMeasurement for the current key,
    // as the JIT elides construction of a two-field struct from constants. The allocation column is the
    // meaningful comparison there.
    //
    // | Method                                               | Mean       | Error      | StdDev    | Ratio | Gen0   | Allocated |
    // |----------------------------------------------------- |-----------:|-----------:|----------:|------:|-------:|----------:|
    // | 'ToLowerInvariant: construct key'                    | 26.7926 ns |  2.5657 ns | 0.1406 ns | 1.000 | 0.0074 |      32 B |
    // | 'OrdinalIgnoreCase: construct key'                   |  0.0434 ns |  0.8742 ns | 0.0479 ns | 0.002 |      - |         - |
    // | 'ToLowerInvariant: TryGetValue (same case)'          | 51.2673 ns |  5.2213 ns | 0.2862 ns | 1.914 | 0.0073 |      32 B |
    // | 'OrdinalIgnoreCase: TryGetValue (same case)'         | 20.2004 ns | 30.0808 ns | 1.6488 ns | 0.754 |      - |         - |
    // | 'ToLowerInvariant: TryGetValue (mixed case)'         | 61.1120 ns | 25.7051 ns | 1.4090 ns | 2.281 | 0.0167 |      72 B |
    // | 'OrdinalIgnoreCase: TryGetValue (mixed case)'        | 32.6253 ns | 14.1231 ns | 0.7741 ns | 1.218 |      - |         - |
    // | 'ToLowerInvariant: TryGetValue (already lower-case)' | 37.2797 ns | 13.8898 ns | 0.7613 ns | 1.391 |      - |         - |
    // | 'OrdinalIgnoreCase: TryGetValue (already lower-case)'| 25.2868 ns | 11.6050 ns | 0.6361 ns | 0.944 |      - |         - |
}

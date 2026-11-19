using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;
using Umbraco.Cms.Core.Collections;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks;

/// <summary>
///     Compares <see cref="CompositeStringArrayKey" /> against <see cref="OldCompositeStringArrayKey" />, the
///     implementation that lower-cased every part on construction, on the (culture, segment, fallback) lookups
///     performed by <c>PublishedProperty</c>'s value cache.
/// </summary>
/// <remarks>
///     <para>
///         Both implementations allocate the defensive copy the constructor takes of its <c>params</c> array, so
///         that array is the floor these numbers sit on - only the lower-casing differs between them, and
///         <see cref="string.ToLowerInvariant" /> returns the same instance when a part is already lower-case
///         ASCII. Removing that copy, not the lower-casing, is the larger remaining win here.
///     </para>
/// </remarks>
[QuickRunWithMemoryDiagnoserConfig]
public class CompositeStringArrayKeyBenchmarks
{
    private const string Culture = "en-US";
    private const string Segment = "mobile";
    private const string FallbackKey = "l";

    private const string LowerCaseCulture = "en-us";

    private const string CultureMixedCase = "EN-us";
    private const string SegmentMixedCase = "MOBILE";
    private const string FallbackKeyMixedCase = "L";

    private const string SomeValue = "property value";

    private ConcurrentDictionary<OldCompositeStringArrayKey, string> _oldDictionary = null!;
    private ConcurrentDictionary<CompositeStringArrayKey, string> _newDictionary = null!;

    [GlobalSetup]
    public void Setup()
    {
        _oldDictionary = new ConcurrentDictionary<OldCompositeStringArrayKey, string>
        {
            [new OldCompositeStringArrayKey(Culture, Segment, FallbackKey)] = SomeValue,
        };

        _newDictionary = new ConcurrentDictionary<CompositeStringArrayKey, string>
        {
            [new CompositeStringArrayKey(Culture, Segment, FallbackKey)] = SomeValue,
        };
    }

    // --- Construction ---

    [Benchmark(Baseline = true, Description = "ToLowerInvariant: construct key")]
    public OldCompositeStringArrayKey Old_Construct()
        => new(Culture, Segment, FallbackKey);

    [Benchmark(Description = "OrdinalIgnoreCase: construct key")]
    public CompositeStringArrayKey New_Construct()
        => new(Culture, Segment, FallbackKey);

    // --- Dictionary lookup, same casing as the stored key ---

    [Benchmark(Description = "ToLowerInvariant: TryGetValue (same case)")]
    public bool Old_TryGetValue_SameCase()
        => _oldDictionary.TryGetValue(new OldCompositeStringArrayKey(Culture, Segment, FallbackKey), out _);

    [Benchmark(Description = "OrdinalIgnoreCase: TryGetValue (same case)")]
    public bool New_TryGetValue_SameCase()
        => _newDictionary.TryGetValue(new CompositeStringArrayKey(Culture, Segment, FallbackKey), out _);

    // --- Dictionary lookup, different casing from the stored key ---

    [Benchmark(Description = "ToLowerInvariant: TryGetValue (mixed case)")]
    public bool Old_TryGetValue_MixedCase()
        => _oldDictionary.TryGetValue(
            new OldCompositeStringArrayKey(CultureMixedCase, SegmentMixedCase, FallbackKeyMixedCase), out _);

    [Benchmark(Description = "OrdinalIgnoreCase: TryGetValue (mixed case)")]
    public bool New_TryGetValue_MixedCase()
        => _newDictionary.TryGetValue(
            new CompositeStringArrayKey(CultureMixedCase, SegmentMixedCase, FallbackKeyMixedCase), out _);

    // --- Dictionary lookup, parts already lower-case (the baseline's non-allocating path) ---

    [Benchmark(Description = "ToLowerInvariant: TryGetValue (already lower-case)")]
    public bool Old_TryGetValue_LowerCase()
        => _oldDictionary.TryGetValue(new OldCompositeStringArrayKey(LowerCaseCulture, Segment, FallbackKey), out _);

    [Benchmark(Description = "OrdinalIgnoreCase: TryGetValue (already lower-case)")]
    public bool New_TryGetValue_LowerCase()
        => _newDictionary.TryGetValue(new CompositeStringArrayKey(LowerCaseCulture, Segment, FallbackKey), out _);

    // The 48 B floor is the defensive copy the constructor takes; the params array itself does not show up,
    // as it no longer escapes once the constructor inlines, so it is stack-allocated.
    //
    // | Method                                               | Mean      | Ratio | Gen0   | Allocated |
    // |----------------------------------------------------- |----------:|------:|-------:|----------:|
    // | 'ToLowerInvariant: construct key'                    |  68.99 ns |  1.00 | 0.0189 |      80 B |
    // | 'OrdinalIgnoreCase: construct key'                   |  40.20 ns |  0.58 | 0.0112 |      48 B |
    // | 'ToLowerInvariant: TryGetValue (same case)'          |  79.54 ns |  1.15 | 0.0190 |      80 B |
    // | 'OrdinalIgnoreCase: TryGetValue (same case)'         |  44.44 ns |  0.64 | 0.0114 |      48 B |
    // | 'ToLowerInvariant: TryGetValue (mixed case)'         | 109.55 ns |  1.59 | 0.0342 |     144 B |
    // | 'OrdinalIgnoreCase: TryGetValue (mixed case)'        |  62.43 ns |  0.90 | 0.0109 |      48 B |
    // | 'ToLowerInvariant: TryGetValue (already lower-case)' |  66.09 ns |  0.96 | 0.0113 |      48 B |
    // | 'OrdinalIgnoreCase: TryGetValue (already lower-case)'|  52.43 ns |  0.76 | 0.0111 |      48 B |
}

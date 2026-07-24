using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;
using Umbraco.Cms.Core.Collections;
using Umbraco.Tests.Benchmarks.Config;

namespace Umbraco.Tests.Benchmarks;

[QuickRunWithMemoryDiagnoserConfig]
public class CompositeStringStringKeyBenchmarks
{
    // Represents the culture/segment pair used in variant content property lookups.
    private const string Culture = "en-US";
    private const string Segment = "default";

    // Mixed-case variant to exercise case-insensitive lookup correctness.
    private const string CultureMixedCase = "EN-us";
    private const string SegmentMixedCase = "DEFAULT";

    private const string SomeValue = "property value";

    private ConcurrentDictionary<OldCompositeStringStringKey, string> _currentDict = null!;
    private ConcurrentDictionary<NewIgnoreCaseCompositeStringStringKey, string> _proposedDict = null!;

    [GlobalSetup]
    public void Setup()
    {
        _currentDict = new ConcurrentDictionary<OldCompositeStringStringKey, string>();
        _currentDict[new OldCompositeStringStringKey(Culture, Segment)] = SomeValue;

        _proposedDict = new ConcurrentDictionary<NewIgnoreCaseCompositeStringStringKey, string>();
        _proposedDict[new NewIgnoreCaseCompositeStringStringKey(Culture, Segment)] = SomeValue;
    }

    // --- Construction ---

    [Benchmark(Baseline = true, Description = "Current: construct key (allocates 2 strings)")]
    public OldCompositeStringStringKey Current_Construct()
        => new(Culture, Segment);

    [Benchmark(Description = "Proposed: construct key (zero allocations)")]
    public NewIgnoreCaseCompositeStringStringKey Proposed_Construct()
        => new(Culture, Segment);

    // --- Dictionary lookup (same case as stored) ---

    [Benchmark(Description = "Current: TryGetValue (same case)")]
    public bool Current_TryGetValue_SameCase()
        => _currentDict.TryGetValue(new OldCompositeStringStringKey(Culture, Segment), out _);

    [Benchmark(Description = "Proposed: TryGetValue (same case)")]
    public bool Proposed_TryGetValue_SameCase()
        => _proposedDict.TryGetValue(new NewIgnoreCaseCompositeStringStringKey(Culture, Segment), out _);

    // --- Dictionary lookup (mixed case — validates case-insensitive semantics) ---

    [Benchmark(Description = "Current: TryGetValue (mixed case)")]
    public bool Current_TryGetValue_MixedCase()
        => _currentDict.TryGetValue(new OldCompositeStringStringKey(CultureMixedCase, SegmentMixedCase), out _);

    [Benchmark(Description = "Proposed: TryGetValue (mixed case)")]
    public bool Proposed_TryGetValue_MixedCase()
        => _proposedDict.TryGetValue(new NewIgnoreCaseCompositeStringStringKey(CultureMixedCase, SegmentMixedCase), out _);

    // | Method                                         | Mean       | Error      | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
    // |----------------------------------------------- |-----------:|-----------:|----------:|------:|--------:|-------:|----------:|------------:|
    // | 'Current: construct key (allocates 2 strings)' | 18.5547 ns | 45.7258 ns | 2.5064 ns |  1.01 |    0.16 | 0.0023 |      32 B |        1.00 |
    // | 'Proposed: construct key (zero allocations)'   |  0.1854 ns |  0.0584 ns | 0.0032 ns |  0.01 |    0.00 |      - |         - |        0.00 |
    // | 'Current: TryGetValue (same case)'             | 46.4106 ns | 17.0343 ns | 0.9337 ns |  2.53 |    0.29 | 0.0023 |      32 B |        1.00 |
    // | 'Proposed: TryGetValue (same case)'            | 13.5532 ns |  7.1661 ns | 0.3928 ns |  0.74 |    0.08 |      - |         - |        0.00 |
    // | 'Current: TryGetValue (mixed case)'            | 47.2566 ns | 12.3332 ns | 0.6760 ns |  2.58 |    0.29 | 0.0053 |      72 B |        2.25 |
    // | 'Proposed: TryGetValue (mixed case)'           | 22.7429 ns |  4.5167 ns | 0.2476 ns |  1.24 |    0.14 |      - |         - |        0.00 |
}

public struct OldCompositeStringStringKey : IEquatable<OldCompositeStringStringKey>
{
    private readonly string _key1;
    private readonly string _key2;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OldCompositeStringStringKey" /> struct.
    /// </summary>
    public OldCompositeStringStringKey(string? key1, string? key2)
    {
        _key1 = key1?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(key1));
        _key2 = key2?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(key2));
    }

    /// <summary>
    ///     Determines whether two <see cref="OldCompositeStringStringKey" /> instances are equal.
    /// </summary>
    /// <param name="key1">The first key to compare.</param>
    /// <param name="key2">The second key to compare.</param>
    /// <returns><c>true</c> if the two keys are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(OldCompositeStringStringKey key1, OldCompositeStringStringKey key2)
        => key1._key2 == key2._key2 && key1._key1 == key2._key1;

    /// <summary>
    ///     Determines whether two <see cref="OldCompositeStringStringKey" /> instances are not equal.
    /// </summary>
    /// <param name="key1">The first key to compare.</param>
    /// <param name="key2">The second key to compare.</param>
    /// <returns><c>true</c> if the two keys are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(OldCompositeStringStringKey key1, OldCompositeStringStringKey key2)
        => key1._key2 != key2._key2 || key1._key1 != key2._key1;

    /// <inheritdoc />
    public bool Equals(OldCompositeStringStringKey other)
        => _key2 == other._key2 && _key1 == other._key1;

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is OldCompositeStringStringKey other && _key2 == other._key2 && _key1 == other._key1;

    /// <inheritdoc />
    public override int GetHashCode()
        => (_key2.GetHashCode() * 31) + _key1.GetHashCode();
}

public readonly struct NewIgnoreCaseCompositeStringStringKey : IEquatable<NewIgnoreCaseCompositeStringStringKey>
{
    private readonly string _key1;
    private readonly string _key2;

    public NewIgnoreCaseCompositeStringStringKey(string? key1, string? key2)
    {
        _key1 = key1 ?? throw new ArgumentNullException(nameof(key1));
        _key2 = key2 ?? throw new ArgumentNullException(nameof(key2));
    }

    public static bool operator ==(NewIgnoreCaseCompositeStringStringKey left, NewIgnoreCaseCompositeStringStringKey right)
        => left.Equals(right);

    public static bool operator !=(NewIgnoreCaseCompositeStringStringKey left, NewIgnoreCaseCompositeStringStringKey right)
        => !left.Equals(right);

    public bool Equals(NewIgnoreCaseCompositeStringStringKey other)
        => StringComparer.OrdinalIgnoreCase.Equals(_key1, other._key1)
        && StringComparer.OrdinalIgnoreCase.Equals(_key2, other._key2);

    public override bool Equals(object? obj)
        => obj is NewIgnoreCaseCompositeStringStringKey other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(_key1),
            StringComparer.OrdinalIgnoreCase.GetHashCode(_key2));
}

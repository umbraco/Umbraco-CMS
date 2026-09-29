using Umbraco.Cms.Core.Collections;

namespace Umbraco.Tests.Benchmarks;

/// <summary>
///     The implementation of <see cref="CompositeStringStringKey" /> that normalized both parts with
///     <see cref="string.ToLowerInvariant" /> on construction, retained as the benchmark baseline.
/// </summary>
public readonly struct OldCompositeStringStringKey : IEquatable<OldCompositeStringStringKey>
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
    public static bool operator ==(OldCompositeStringStringKey key1, OldCompositeStringStringKey key2)
        => key1._key2 == key2._key2 && key1._key1 == key2._key1;

    /// <summary>
    ///     Determines whether two <see cref="OldCompositeStringStringKey" /> instances are not equal.
    /// </summary>
    public static bool operator !=(OldCompositeStringStringKey key1, OldCompositeStringStringKey key2)
        => key1._key2 != key2._key2 || key1._key1 != key2._key1;

    /// <inheritdoc />
    public bool Equals(OldCompositeStringStringKey other)
        => _key2 == other._key2 && _key1 == other._key1;

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is OldCompositeStringStringKey other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => (_key2.GetHashCode() * 31) + _key1.GetHashCode();
}

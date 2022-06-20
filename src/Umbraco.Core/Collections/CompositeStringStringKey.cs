namespace Umbraco.Cms.Core.Collections;

/// <summary>
///     Represents a composite key of (string, string) for fast dictionaries.
/// </summary>
/// <remarks>
///     <para>The string parts of the key are case-insensitive.</para>
///     <para>Null is NOT a valid value for neither parts.</para>
/// </remarks>
public struct CompositeStringStringKey : IEquatable<CompositeStringStringKey>
{
    private readonly string _key1;
    private readonly string _key2;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CompositeStringStringKey" /> struct.
    /// </summary>
    public CompositeStringStringKey(string? key1, string? key2)
    {
        _key1 = key1?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(key1));
        _key2 = key2?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(key2));
    }

    public static bool operator ==(CompositeStringStringKey key1, CompositeStringStringKey key2)
        => key1._key2 == key2._key2 && key1._key1 == key2._key1;

    public static bool operator !=(CompositeStringStringKey key1, CompositeStringStringKey key2)
        => key1._key2 != key2._key2 || key1._key1 != key2._key1;

    public bool Equals(CompositeStringStringKey other)
        => _key2 == other._key2 && _key1 == other._key1;

    public override bool Equals(object? obj)
        => obj is CompositeStringStringKey other && _key2 == other._key2 && _key1 == other._key1;

    public override int GetHashCode()
        => (_key2.GetHashCode() * 31) + _key1.GetHashCode();
}

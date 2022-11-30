namespace Umbraco.Cms.Core.Collections;

/// <summary>
///     Represents a composite key of (string, string) for fast dictionaries.
/// </summary>
/// <remarks>
///     <para>The string parts of the key are case-insensitive.</para>
///     <para>Null is a valid value for both parts.</para>
/// </remarks>
public struct CompositeNStringNStringKey : IEquatable<CompositeNStringNStringKey>
{
    private readonly string _key1;
    private readonly string _key2;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CompositeNStringNStringKey" /> struct.
    /// </summary>
    public CompositeNStringNStringKey(string? key1, string? key2)
    {
        _key1 = key1?.ToLowerInvariant() ?? "NULL";
        _key2 = key2?.ToLowerInvariant() ?? "NULL";
    }

    public static bool operator ==(CompositeNStringNStringKey key1, CompositeNStringNStringKey key2)
        => key1._key2 == key2._key2 && key1._key1 == key2._key1;

    public static bool operator !=(CompositeNStringNStringKey key1, CompositeNStringNStringKey key2)
        => key1._key2 != key2._key2 || key1._key1 != key2._key1;

    public bool Equals(CompositeNStringNStringKey other)
        => _key2 == other._key2 && _key1 == other._key1;

    public override bool Equals(object? obj)
        => obj is CompositeNStringNStringKey other && _key2 == other._key2 && _key1 == other._key1;

    public override int GetHashCode()
        => (_key2.GetHashCode() * 31) + _key1.GetHashCode();
}

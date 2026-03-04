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

    /// <summary>
    ///     Determines whether two <see cref="CompositeNStringNStringKey" /> instances are equal.
    /// </summary>
    /// <param name="key1">The first key to compare.</param>
    /// <param name="key2">The second key to compare.</param>
    /// <returns><c>true</c> if the two keys are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(CompositeNStringNStringKey key1, CompositeNStringNStringKey key2)
        => key1._key2 == key2._key2 && key1._key1 == key2._key1;

    /// <summary>
    ///     Determines whether two <see cref="CompositeNStringNStringKey" /> instances are not equal.
    /// </summary>
    /// <param name="key1">The first key to compare.</param>
    /// <param name="key2">The second key to compare.</param>
    /// <returns><c>true</c> if the two keys are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(CompositeNStringNStringKey key1, CompositeNStringNStringKey key2)
        => key1._key2 != key2._key2 || key1._key1 != key2._key1;

    /// <inheritdoc />
    public bool Equals(CompositeNStringNStringKey other)
        => _key2 == other._key2 && _key1 == other._key1;

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is CompositeNStringNStringKey other && _key2 == other._key2 && _key1 == other._key1;

    /// <inheritdoc />
    public override int GetHashCode()
        => (_key2.GetHashCode() * 31) + _key1.GetHashCode();
}

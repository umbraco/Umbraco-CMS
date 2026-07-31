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
        _key1 = key1 ?? throw new ArgumentNullException(nameof(key1));
        _key2 = key2 ?? throw new ArgumentNullException(nameof(key2));
    }

    /// <summary>
    ///     Determines whether two <see cref="CompositeStringStringKey" /> instances are equal.
    /// </summary>
    /// <param name="key1">The first key to compare.</param>
    /// <param name="key2">The second key to compare.</param>
    /// <returns><c>true</c> if the two keys are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(CompositeStringStringKey key1, CompositeStringStringKey key2)
        => key1.Equals(key2);

    /// <summary>
    ///     Determines whether two <see cref="CompositeStringStringKey" /> instances are not equal.
    /// </summary>
    /// <param name="key1">The first key to compare.</param>
    /// <param name="key2">The second key to compare.</param>
    /// <returns><c>true</c> if the two keys are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(CompositeStringStringKey key1, CompositeStringStringKey key2)
        => !key1.Equals(key2);

    /// <inheritdoc />
    public bool Equals(CompositeStringStringKey other)
        => StringComparer.OrdinalIgnoreCase.Equals(_key1, other._key1)
        && StringComparer.OrdinalIgnoreCase.Equals(_key2, other._key2);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is CompositeStringStringKey other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(_key1),
            StringComparer.OrdinalIgnoreCase.GetHashCode(_key2));
}

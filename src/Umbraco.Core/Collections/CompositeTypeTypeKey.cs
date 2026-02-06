namespace Umbraco.Cms.Core.Collections;

/// <summary>
///     Represents a composite key of (Type, Type) for fast dictionaries.
/// </summary>
public struct CompositeTypeTypeKey : IEquatable<CompositeTypeTypeKey>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CompositeTypeTypeKey" /> struct.
    /// </summary>
    public CompositeTypeTypeKey(Type type1, Type type2)
        : this()
    {
        Type1 = type1;
        Type2 = type2;
    }

    /// <summary>
    ///     Gets the first type.
    /// </summary>
    public Type Type1 { get; }

    /// <summary>
    ///     Gets the second type.
    /// </summary>
    public Type Type2 { get; }

    /// <summary>
    ///     Determines whether two <see cref="CompositeTypeTypeKey" /> instances are equal.
    /// </summary>
    /// <param name="key1">The first key to compare.</param>
    /// <param name="key2">The second key to compare.</param>
    /// <returns><c>true</c> if the two keys are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(CompositeTypeTypeKey key1, CompositeTypeTypeKey key2) =>
        key1.Type1 == key2.Type1 && key1.Type2 == key2.Type2;

    /// <summary>
    ///     Determines whether two <see cref="CompositeTypeTypeKey" /> instances are not equal.
    /// </summary>
    /// <param name="key1">The first key to compare.</param>
    /// <param name="key2">The second key to compare.</param>
    /// <returns><c>true</c> if the two keys are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(CompositeTypeTypeKey key1, CompositeTypeTypeKey key2) =>
        key1.Type1 != key2.Type1 || key1.Type2 != key2.Type2;

    /// <inheritdoc />
    public bool Equals(CompositeTypeTypeKey other) => Type1 == other.Type1 && Type2 == other.Type2;

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        CompositeTypeTypeKey other = obj is CompositeTypeTypeKey key ? key : default;
        return Type1 == other.Type1 && Type2 == other.Type2;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            return (Type1.GetHashCode() * 397) ^ Type2.GetHashCode();
        }
    }
}

namespace Umbraco.Cms.Core.Collections;

/// <summary>
///     Represents a composite key of (int, string) for fast dictionaries.
/// </summary>
/// <remarks>
///     <para>The integer part of the key must be greater than, or equal to, zero.</para>
///     <para>The string part of the key is case-insensitive.</para>
///     <para>Null is a valid value for both parts.</para>
/// </remarks>
public struct CompositeIntStringKey : IEquatable<CompositeIntStringKey>
{
    private readonly int _key1;
    private readonly string _key2;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CompositeIntStringKey" /> struct.
    /// </summary>
    public CompositeIntStringKey(int? key1, string? key2)
    {
        if (key1 < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(key1));
        }

        _key1 = key1 ?? -1;
        _key2 = key2?.ToLowerInvariant() ?? "NULL";
    }

    public static bool operator ==(CompositeIntStringKey key1, CompositeIntStringKey key2)
        => key1._key2 == key2._key2 && key1._key1 == key2._key1;

    public static bool operator !=(CompositeIntStringKey key1, CompositeIntStringKey key2)
        => key1._key2 != key2._key2 || key1._key1 != key2._key1;

    public bool Equals(CompositeIntStringKey other)
        => _key2 == other._key2 && _key1 == other._key1;

    public override bool Equals(object? obj)
        => obj is CompositeIntStringKey other && _key2 == other._key2 && _key1 == other._key1;

    public override int GetHashCode()
        => (_key2.GetHashCode() * 31) + _key1;
}

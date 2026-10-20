namespace Umbraco.Cms.Core.Collections;

/// <summary>
///     Represents a composite key of multiple strings for fast dictionaries.
/// </summary>
/// <remarks>
///     <para>The string parts of the key are compared using <see cref="StringComparer.OrdinalIgnoreCase" />.</para>
///     <para>Null is NOT a valid value for any part.</para>
/// </remarks>
public readonly struct CompositeStringArrayKey : IEquatable<CompositeStringArrayKey>
{
    private readonly string[] _keys;
    private readonly int _hashCode;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CompositeStringArrayKey" /> struct.
    /// </summary>
    public CompositeStringArrayKey(params string[] keys)
    {
        _keys = new string[keys.Length];
        var hash = new HashCode();
        for (var i = 0; i < keys.Length; i++)
        {
            _keys[i] = keys[i] ?? throw new ArgumentNullException(nameof(keys));
            hash.Add(_keys[i], StringComparer.OrdinalIgnoreCase);
        }

        _hashCode = hash.ToHashCode();
    }

    /// <summary>
    ///     Determines whether two <see cref="CompositeStringArrayKey" /> instances are equal.
    /// </summary>
    public static bool operator ==(CompositeStringArrayKey key1, CompositeStringArrayKey key2)
        => key1.Equals(key2);

    /// <summary>
    ///     Determines whether two <see cref="CompositeStringArrayKey" /> instances are not equal.
    /// </summary>
    public static bool operator !=(CompositeStringArrayKey key1, CompositeStringArrayKey key2)
        => !key1.Equals(key2);

    /// <inheritdoc />
    public bool Equals(CompositeStringArrayKey other)
    {
        if (_keys.Length != other._keys.Length)
        {
            return false;
        }

        for (var i = 0; i < _keys.Length; i++)
        {
            if (!StringComparer.OrdinalIgnoreCase.Equals(_keys[i], other._keys[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is CompositeStringArrayKey other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => _hashCode;
}

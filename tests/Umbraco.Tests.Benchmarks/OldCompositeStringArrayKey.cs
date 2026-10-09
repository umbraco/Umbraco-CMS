using Umbraco.Cms.Core.Collections;

namespace Umbraco.Tests.Benchmarks;

/// <summary>
///     The implementation of <see cref="CompositeStringArrayKey" /> that normalized every part with
///     <see cref="string.ToLowerInvariant" /> on construction, retained as the benchmark baseline.
/// </summary>
public readonly struct OldCompositeStringArrayKey : IEquatable<OldCompositeStringArrayKey>
{
    private readonly string[] _keys;
    private readonly int _hashCode;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OldCompositeStringArrayKey" /> struct.
    /// </summary>
    public OldCompositeStringArrayKey(params string[] keys)
    {
        _keys = new string[keys.Length];
        var hash = new HashCode();
        for (var i = 0; i < keys.Length; i++)
        {
            _keys[i] = keys[i]?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(keys));
            hash.Add(_keys[i]);
        }

        _hashCode = hash.ToHashCode();
    }

    /// <summary>
    ///     Determines whether two <see cref="OldCompositeStringArrayKey" /> instances are equal.
    /// </summary>
    public static bool operator ==(OldCompositeStringArrayKey key1, OldCompositeStringArrayKey key2)
        => key1.Equals(key2);

    /// <summary>
    ///     Determines whether two <see cref="OldCompositeStringArrayKey" /> instances are not equal.
    /// </summary>
    public static bool operator !=(OldCompositeStringArrayKey key1, OldCompositeStringArrayKey key2)
        => !key1.Equals(key2);

    /// <inheritdoc />
    public bool Equals(OldCompositeStringArrayKey other)
    {
        if (_keys.Length != other._keys.Length)
        {
            return false;
        }

        for (var i = 0; i < _keys.Length; i++)
        {
            if (_keys[i] != other._keys[i])
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is OldCompositeStringArrayKey other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => _hashCode;
}

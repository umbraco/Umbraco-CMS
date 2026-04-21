using System.Collections.Frozen;

namespace Umbraco.Cms.Core;

/// <summary>
///     Provides utility methods for handling enumerations.
/// </summary>
/// <remarks>
///     Taken from http://damieng.com/blog/2010/10/17/enums-better-syntax-improved-performance-and-tryparse-in-net-3-5
/// </remarks>
public static class Enum<T>
    where T : struct
{
    private static readonly T[] _values;
    private static readonly FrozenDictionary<string, T> _insensitiveNameToValue;
    private static readonly FrozenDictionary<string, T> _sensitiveNameToValue;
    private static readonly FrozenDictionary<int, T> _intToValue;
    private static readonly FrozenDictionary<T, string> _valueToName;

    /// <summary>
    ///     Static constructor that initializes the enum value dictionaries.
    /// </summary>
    static Enum()
    {
        _values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();

        Dictionary<int, T> intToValue = new();
        Dictionary<T, string> valueToName = new();
        Dictionary<string, T> sensitiveNameToValue = new();
        Dictionary<string, T> insensitiveNameToValue = new(StringComparer.InvariantCultureIgnoreCase);

        foreach (T value in _values)
        {
            var name = value.ToString();

            intToValue[Convert.ToInt32(value)] = value;
            valueToName[value] = name!;
            sensitiveNameToValue[name!] = value;
            insensitiveNameToValue[name!] = value;
        }

        _intToValue = intToValue.ToFrozenDictionary();
        _valueToName = valueToName.ToFrozenDictionary();
        _sensitiveNameToValue = sensitiveNameToValue.ToFrozenDictionary();
        _insensitiveNameToValue = insensitiveNameToValue.ToFrozenDictionary(StringComparer.InvariantCultureIgnoreCase);
    }

    /// <summary>
    ///     Determines whether the specified value is defined in the enumeration.
    /// </summary>
    /// <param name="value">The enum value to check.</param>
    /// <returns><c>true</c> if the value is defined; otherwise, <c>false</c>.</returns>
    public static bool IsDefined(T value) => _valueToName.ContainsKey(value);

    /// <summary>
    ///     Determines whether the specified string is a defined name in the enumeration.
    /// </summary>
    /// <param name="value">The string value to check.</param>
    /// <returns><c>true</c> if the name is defined; otherwise, <c>false</c>.</returns>
    public static bool IsDefined(string value) => _sensitiveNameToValue.ContainsKey(value);

    /// <summary>
    ///     Determines whether the specified integer is a defined value in the enumeration.
    /// </summary>
    /// <param name="value">The integer value to check.</param>
    /// <returns><c>true</c> if the value is defined; otherwise, <c>false</c>.</returns>
    public static bool IsDefined(int value) => _intToValue.ContainsKey(value);

    /// <summary>
    ///     Gets all values defined in the enumeration.
    /// </summary>
    /// <returns>An enumerable containing all enum values.</returns>
    public static IEnumerable<T> GetValues() => _values;

    /// <summary>
    ///     Gets all names defined in the enumeration.
    /// </summary>
    /// <returns>An array containing all enum names.</returns>
    public static string[] GetNames() => _valueToName.Values.ToArray();

    /// <summary>
    ///     Gets the name of the specified enum value.
    /// </summary>
    /// <param name="value">The enum value.</param>
    /// <returns>The name of the value, or null if not found.</returns>
    public static string? GetName(T value) => _valueToName.GetValueOrDefault(value);

    /// <summary>
    ///     Parses the string representation of an enum value.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <param name="ignoreCase">If <c>true</c>, case is ignored during parsing.</param>
    /// <returns>The parsed enum value.</returns>
    /// <exception cref="ArgumentException">The string is not a valid enum name.</exception>
    public static T Parse(string value, bool ignoreCase = false)
    {
        FrozenDictionary<string, T> names = ignoreCase ? _insensitiveNameToValue : _sensitiveNameToValue;

        return names.TryGetValue(value, out T parsed) ? parsed : Throw();

        T Throw() => throw new ArgumentException($"Value \"{value}\"is not a valid {typeof(T).Name} enumeration value.", nameof(value));
    }

    /// <summary>
    ///     Tries to parse the string representation of an enum value.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <param name="returnValue">When this method returns, contains the parsed value if successful.</param>
    /// <param name="ignoreCase">If <c>true</c>, case is ignored during parsing.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise, <c>false</c>.</returns>
    public static bool TryParse(string value, out T returnValue, bool ignoreCase = false)
    {
        FrozenDictionary<string, T> names = ignoreCase ? _insensitiveNameToValue : _sensitiveNameToValue;

        return names.TryGetValue(value, out returnValue);
    }

    /// <summary>
    ///     Parses the string representation of an enum value, returning null if parsing fails.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>The parsed enum value, or null if parsing failed.</returns>
    public static T? ParseOrNull(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (_insensitiveNameToValue.TryGetValue(value, out T parsed))
        {
            return parsed;
        }

        return null;
    }

    /// <summary>
    ///     Casts an integer to the enum type, returning null if the integer is not a valid value.
    /// </summary>
    /// <param name="value">The integer value to cast.</param>
    /// <returns>The enum value, or null if the integer is not valid.</returns>
    public static T? CastOrNull(int value)
    {
        if (_intToValue.TryGetValue(value, out T foundValue))
        {
            return foundValue;
        }

        return null;
    }
}

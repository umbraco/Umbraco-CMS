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
    private static readonly List<T> _values;
    private static readonly Dictionary<string, T> _insensitiveNameToValue;
    private static readonly Dictionary<string, T> _sensitiveNameToValue;
    private static readonly Dictionary<int, T> _intToValue;
    private static readonly Dictionary<T, string> _valueToName;

    static Enum()
    {
        _values = Enum.GetValues(typeof(T)).Cast<T>().ToList();

        _intToValue = new Dictionary<int, T>();
        _valueToName = new Dictionary<T, string>();
        _sensitiveNameToValue = new Dictionary<string, T>();
        _insensitiveNameToValue = new Dictionary<string, T>(StringComparer.InvariantCultureIgnoreCase);

        foreach (T value in _values)
        {
            string? name = value.ToString();

            _intToValue[Convert.ToInt32(value)] = value;
            _valueToName[value] = name!;
            _sensitiveNameToValue[name!] = value;
            _insensitiveNameToValue[name!] = value;
        }
    }

    public static bool IsDefined(T value) => _valueToName.ContainsKey(value);

    public static bool IsDefined(string value) => _sensitiveNameToValue.ContainsKey(value);

    public static bool IsDefined(int value) => _intToValue.ContainsKey(value);

    public static IEnumerable<T> GetValues() => _values;

    public static string[] GetNames() => _valueToName.Values.ToArray();

    public static string? GetName(T value) => _valueToName.TryGetValue(value, out string? name) ? name : null;

    public static T Parse(string value, bool ignoreCase = false)
    {
        Dictionary<string, T> names = ignoreCase ? _insensitiveNameToValue : _sensitiveNameToValue;

        return names.TryGetValue(value, out T parsed) ? parsed : Throw();

        T Throw() => throw new ArgumentException($"Value \"{value}\"is not a valid {typeof(T).Name} enumeration value.", nameof(value));
    }

    public static bool TryParse(string value, out T returnValue, bool ignoreCase = false)
    {
        Dictionary<string, T> names = ignoreCase ? _insensitiveNameToValue : _sensitiveNameToValue;

        return names.TryGetValue(value, out returnValue);
    }

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

    public static T? CastOrNull(int value)
    {
        if (_intToValue.TryGetValue(value, out T foundValue))
        {
            return foundValue;
        }

        return null;
    }
}

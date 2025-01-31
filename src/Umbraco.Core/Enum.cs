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
    private static readonly List<T> Values;
    private static readonly Dictionary<string, T> InsensitiveNameToValue;
    private static readonly Dictionary<string, T> SensitiveNameToValue;
    private static readonly Dictionary<int, T> IntToValue;
    private static readonly Dictionary<T, string> ValueToName;

    static Enum()
    {
        Values = Enum.GetValues(typeof(T)).Cast<T>().ToList();

        IntToValue = new Dictionary<int, T>();
        ValueToName = new Dictionary<T, string>();
        SensitiveNameToValue = new Dictionary<string, T>();
        InsensitiveNameToValue = new Dictionary<string, T>(StringComparer.InvariantCultureIgnoreCase);

        foreach (T value in Values)
        {
            var name = value.ToString();

            IntToValue[Convert.ToInt32(value)] = value;
            ValueToName[value] = name!;
            SensitiveNameToValue[name!] = value;
            InsensitiveNameToValue[name!] = value;
        }
    }

    public static bool IsDefined(T value) => ValueToName.ContainsKey(value);

    public static bool IsDefined(string value) => SensitiveNameToValue.ContainsKey(value);

    public static bool IsDefined(int value) => IntToValue.ContainsKey(value);

    public static IEnumerable<T> GetValues() => Values;

    public static string[] GetNames() => ValueToName.Values.ToArray();

    public static string? GetName(T value) => ValueToName.GetValueOrDefault(value);

    public static T Parse(string value, bool ignoreCase = false)
    {
        Dictionary<string, T> names = ignoreCase ? InsensitiveNameToValue : SensitiveNameToValue;

        return names.TryGetValue(value, out T parsed) ? parsed : Throw();

        T Throw() => throw new ArgumentException($"Value \"{value}\"is not a valid {typeof(T).Name} enumeration value.", nameof(value));
    }

    public static bool TryParse(string value, out T returnValue, bool ignoreCase = false)
    {
        Dictionary<string, T> names = ignoreCase ? InsensitiveNameToValue : SensitiveNameToValue;

        return names.TryGetValue(value, out returnValue);
    }

    public static T? ParseOrNull(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (InsensitiveNameToValue.TryGetValue(value, out T parsed))
        {
            return parsed;
        }

        return null;
    }

    public static T? CastOrNull(int value)
    {
        if (IntToValue.TryGetValue(value, out T foundValue))
        {
            return foundValue;
        }

        return null;
    }
}

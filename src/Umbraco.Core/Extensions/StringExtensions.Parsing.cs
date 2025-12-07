// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core;

namespace Umbraco.Extensions;

/// <summary>
/// Parsing, detection, and splitting extensions.
/// </summary>
public static partial class StringExtensions
{
    internal static readonly string[] JsonEmpties = { "[]", "{}" };
    private const char DefaultEscapedStringEscapeChar = '\\';

    /// <summary>
    ///     Indicates whether a specified string is null, empty, or
    ///     consists only of white-space characters.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>
    ///     Returns <see langword="true" /> if the value is null,
    ///     empty, or consists only of white-space characters, otherwise
    ///     returns <see langword="false" />.
    /// </returns>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value) => string.IsNullOrWhiteSpace(value);

    [return: NotNullIfNotNull("defaultValue")]
    public static string? IfNullOrWhiteSpace(this string? str, string? defaultValue) =>
        str.IsNullOrWhiteSpace() ? defaultValue : str;

    [return: NotNullIfNotNull(nameof(alternative))]
    public static string? OrIfNullOrWhiteSpace(this string? input, string? alternative) =>
        !string.IsNullOrWhiteSpace(input)
            ? input
            : alternative;

    /// <summary>
    ///     Turns an null-or-whitespace string into a null string.
    /// </summary>
    public static string? NullOrWhiteSpaceAsNull(this string? text)
        => string.IsNullOrWhiteSpace(text) ? null : text;

    /// <summary>
    ///     This tries to detect a json string, this is not a fail safe way but it is quicker than doing
    ///     a try/catch when deserializing when it is not json.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool DetectIsJson(this string input)
    {
        if (input.IsNullOrWhiteSpace())
        {
            return false;
        }

        input = input.Trim();
        return (input[0] is '[' && input[^1] is ']') || (input[0] is '{' && input[^1] is '}');
    }

    public static bool DetectIsEmptyJson(this string input) =>
        JsonEmpties.Contains(Whitespace.Value.Replace(input, string.Empty));

    /// <summary>
    ///     Tries to parse a string into the supplied type by finding and using the Type's "Parse" method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val"></param>
    /// <returns></returns>
    public static T? ParseInto<T>(this string val) => (T?)val.ParseInto(typeof(T));

    /// <summary>
    ///     Tries to parse a string into the supplied type by finding and using the Type's "Parse" method
    /// </summary>
    /// <param name="val"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object? ParseInto(this string val, Type type)
    {
        if (string.IsNullOrEmpty(val) == false)
        {
            TypeConverter tc = TypeDescriptor.GetConverter(type);
            return tc.ConvertFrom(val);
        }

        return val;
    }

    /// <summary>enum try parse.</summary>
    /// <param name="strType">The str type.</param>
    /// <param name="ignoreCase">The ignore case.</param>
    /// <param name="result">The result.</param>
    /// <typeparam name="T">The type</typeparam>
    /// <returns>The enum try parse.</returns>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By Design")]
    [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "By Design")]
    public static bool EnumTryParse<T>(this string strType, bool ignoreCase, out T? result)
    {
        try
        {
            result = (T)Enum.Parse(typeof(T), strType, ignoreCase);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    ///     Parse string to Enum
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="strType">The string to parse</param>
    /// <param name="ignoreCase">The ignore case</param>
    /// <returns>The parsed enum</returns>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "By Design")]
    [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "By Design")]
    public static T EnumParse<T>(this string strType, bool ignoreCase) => (T)Enum.Parse(typeof(T), strType, ignoreCase);

    /// <summary>The to delimited list.</summary>
    /// <param name="list">The list.</param>
    /// <param name="delimiter">The delimiter.</param>
    /// <returns>the list</returns>
    [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "By design")]
    public static IList<string> ToDelimitedList(this string list, string delimiter = ",")
    {
        var delimiters = new[] { delimiter };
        return !list.IsNullOrWhiteSpace()
            ? list.Split(delimiters, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList()
            : new List<string>();
    }

    /// <summary>
    ///     Splits a string with an escape character that allows for the split character to exist in a string
    /// </summary>
    /// <param name="value">The string to split</param>
    /// <param name="splitChar">The character to split on</param>
    /// <param name="escapeChar">The character which can be used to escape the character to split on</param>
    /// <returns>The string split into substrings delimited by the split character</returns>
    public static IEnumerable<string> EscapedSplit(this string value, char splitChar, char escapeChar = DefaultEscapedStringEscapeChar)
    {
        if (value == null)
        {
            yield break;
        }

        var sb = new StringBuilder(value.Length);
        var escaped = false;

        foreach (var chr in value.ToCharArray())
        {
            if (escaped)
            {
                escaped = false;
                sb.Append(chr);
            }
            else if (chr == splitChar)
            {
                yield return sb.ToString();
                sb.Clear();
            }
            else if (chr == escapeChar)
            {
                escaped = true;
            }
            else
            {
                sb.Append(chr);
            }
        }

        yield return sb.ToString();
    }

    /// <summary>
    ///     Checks whether a string "haystack" contains within it any of the strings in the "needles" collection and returns
    ///     true if it does or false if it doesn't
    /// </summary>
    /// <param name="haystack">The string to check</param>
    /// <param name="needles">The collection of strings to check are contained within the first string</param>
    /// <param name="comparison">
    ///     The type of comparison to perform - defaults to <see cref="StringComparison.CurrentCulture" />
    /// </param>
    /// <returns>True if any of the needles are contained with haystack; otherwise returns false</returns>
    /// Added fix to ensure the comparison is used - see http://issues.umbraco.org/issue/U4-11313
    public static bool ContainsAny(this string haystack, IEnumerable<string> needles, StringComparison comparison = StringComparison.CurrentCulture)
    {
        if (haystack == null)
        {
            throw new ArgumentNullException("haystack");
        }

        if (string.IsNullOrEmpty(haystack) || needles == null || !needles.Any())
        {
            return false;
        }

        return needles.Any(value => haystack.IndexOf(value, comparison) >= 0);
    }

    public static bool CsvContains(this string csv, string value)
    {
        if (string.IsNullOrEmpty(csv))
        {
            return false;
        }

        var idCheckList = csv.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);
        return idCheckList.Contains(value);
    }

    // having benchmarked various solutions (incl. for/foreach, split and LINQ based ones),
    // this is by far the fastest way to find string needles in a string haystack
    public static int CountOccurrences(this string haystack, string needle)
        => haystack.Length - haystack.Replace(needle, string.Empty).Length;

    /// <summary>
    ///     Convert a path to node ids in the order from right to left (deepest to shallowest).
    /// </summary>
    /// <param name="path">The path string expected as a comma delimited collection of integers.</param>
    /// <returns>An array of integers matching the provided path.</returns>
    public static int[] GetIdsFromPathReversed(this string path)
    {
        ReadOnlySpan<char> pathSpan = path.AsSpan();

        // Using the explicit enumerator (while/MoveNext) over the SpanSplitEnumerator in a foreach loop to avoid any compiler
        // boxing of the ref struct enumerator.
        // This prevents potential InvalidProgramException across compilers/JITs ("Cannot create boxed ByRef-like values.").
        MemoryExtensions.SpanSplitEnumerator<char> pathSegmentsEnumerator = pathSpan.Split(Constants.CharArrays.Comma);

        List<int> nodeIds = [];
        while (pathSegmentsEnumerator.MoveNext())
        {
            Range rangeOfPathSegment = pathSegmentsEnumerator.Current;
            if (int.TryParse(pathSpan[rangeOfPathSegment], NumberStyles.Integer, CultureInfo.InvariantCulture, out int pathSegment))
            {
                nodeIds.Add(pathSegment);
            }
        }

        var result = new int[nodeIds.Count];
        var resultIndex = 0;
        for (int i = nodeIds.Count - 1; i >= 0; i--)
        {
            result[resultIndex++] = nodeIds[i];
        }

        return result;
    }
}

// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core;

namespace Umbraco.Extensions;

public static partial class StringExtensions
{
#pragma warning disable IDE1006 // Naming Styles (internal Guid is clearer without a _ prefix).
    /// <summary>
    /// Provides a lazily initialized, compiled regular expression that matches one or more whitespace characters.
    /// </summary>
    internal static readonly Lazy<Regex> Whitespace = new(() => new Regex(@"\s+", RegexOptions.Compiled));

    /// <summary>
    /// Provides a collection of JSON string representations that are considered empty objects or arrays.
    /// </summary>
    internal static readonly string[] JsonEmpties = { "[]", "{}" };
#pragma warning restore IDE1006 // Naming Styles

    private static readonly char[] _cleanForXssChars = "*?(){}[];:%<>/\\|&'\"".ToCharArray();

    /// <summary>
    /// A regex to match invalid XML characters.
    /// </summary>
    /// <remarks>
    /// Filters control characters but allows only properly-formed surrogate sequences.
    /// Hat-tip: http://stackoverflow.com/a/961504/5018.
    /// </remarks>
    private static readonly Lazy<Regex> _invalidXmlChars = new(() =>
        new Regex(
            @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
            RegexOptions.Compiled));

    /// <summary>
    /// Attempts to detect whether a string is JSON by checking for opening and closing brackets or braces.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <returns><c>true</c> if the string appears to be JSON; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This is not a fail-safe way to detect JSON, but it is quicker than doing a try/catch when deserialising
    /// when it is not JSON.
    /// </remarks>
    public static bool DetectIsJson(this string input)
    {
        if (input.IsNullOrWhiteSpace())
        {
            return false;
        }

        input = input.Trim();
        return (input[0] is '[' && input[^1] is ']') || (input[0] is '{' && input[^1] is '}');
    }

    /// <summary>
    /// Determines whether a string represents empty JSON (either an empty array or empty object).
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <returns><c>true</c> if the string is "[]" or "{}" (ignoring whitespace); otherwise, <c>false</c>.</returns>
    public static bool DetectIsEmptyJson(this string input) =>
        JsonEmpties.Contains(Whitespace.Value.Replace(input, string.Empty));

    /// <summary>
    /// Cleans a string to aid in preventing XSS attacks.
    /// </summary>
    /// <param name="input">The string to clean.</param>
    /// <param name="ignoreFromClean">Characters to ignore when cleaning.</param>
    /// <returns>The cleaned string with HTML stripped and potentially dangerous characters removed.</returns>
    public static string CleanForXss(this string input, params char[] ignoreFromClean)
    {
        // remove any HTML
        input = input.StripHtml();

        // strip out any potential chars involved with XSS
        return input.ExceptChars(new HashSet<char>(_cleanForXssChars.Except(ignoreFromClean)));
    }

    /// <summary>
    /// Indicates whether a specified string is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>
    /// <c>true</c> if the value is null, empty, or consists only of white-space characters;
    /// otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value) => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Returns the default value if the string is null or whitespace; otherwise, returns the original string.
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <param name="defaultValue">The default value to return if the string is null or whitespace.</param>
    /// <returns>The default value if the string is null or whitespace; otherwise, the original string.</returns>
    [return: NotNullIfNotNull("defaultValue")]
    public static string? IfNullOrWhiteSpace(this string? str, string? defaultValue) =>
        str.IsNullOrWhiteSpace() ? defaultValue : str;

    /// <summary>
    /// Returns the alternative value if the string is null or whitespace; otherwise, returns the original string.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <param name="alternative">The alternative value to return if the string is null or whitespace.</param>
    /// <returns>The original string if it is not null or whitespace; otherwise, the alternative value.</returns>
    [return: NotNullIfNotNull(nameof(alternative))]
    public static string? OrIfNullOrWhiteSpace(this string? input, string? alternative) =>
        !string.IsNullOrWhiteSpace(input)
            ? input
            : alternative;

    /// <summary>
    /// Checks whether a string haystack contains within it any of the strings in the needles collection.
    /// </summary>
    /// <param name="haystack">The string to check.</param>
    /// <param name="needles">The collection of strings to check are contained within the first string.</param>
    /// <param name="comparison">
    /// The type of comparison to perform. Defaults to <see cref="StringComparison.CurrentCulture"/>.
    /// </param>
    /// <returns><c>true</c> if any of the needles are contained within haystack; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when haystack is null.</exception>
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

    /// <summary>
    /// Determines whether a comma-separated value string contains the specified value.
    /// </summary>
    /// <param name="csv">The comma-separated string to search.</param>
    /// <param name="value">The value to search for.</param>
    /// <returns><c>true</c> if the CSV contains the specified value; otherwise, <c>false</c>.</returns>
    public static bool CsvContains(this string csv, string value)
    {
        if (string.IsNullOrEmpty(csv))
        {
            return false;
        }

        var idCheckList = csv.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);
        return idCheckList.Contains(value);
    }

    /// <summary>
    /// Returns a new string in which all occurrences of Unicode characters that are invalid in XML files
    /// are replaced with an empty string.
    /// </summary>
    /// <param name="text">The string to filter.</param>
    /// <returns>The string with invalid XML characters removed.</returns>
    public static string ToValidXmlString(this string text) =>
        string.IsNullOrEmpty(text) ? text : _invalidXmlChars.Value.Replace(text, string.Empty);

    /// <summary>
    /// Turns a null-or-whitespace string into a null string.
    /// </summary>
    /// <param name="text">The string to check.</param>
    /// <returns>Null if the string is null or whitespace; otherwise, the original string.</returns>
    public static string? NullOrWhiteSpaceAsNull(this string? text)
        => string.IsNullOrWhiteSpace(text) ? null : text;

    /// <summary>
    /// Checks if a given path is a full path including a drive letter or root.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns><c>true</c> if the path is fully qualified; otherwise, <c>false</c>.</returns>
    public static bool IsFullPath(this string path) => Path.IsPathFullyQualified(path);

    /// <summary>
    /// Checks whether a string is a valid email address.
    /// </summary>
    /// <param name="email">The string to check.</param>
    /// <returns><c>true</c> if the string is a valid email address; otherwise, <c>false</c>.</returns>
    public static bool IsEmail(this string? email) =>
        string.IsNullOrWhiteSpace(email) is false && new EmailAddressAttribute().IsValid(email);
}

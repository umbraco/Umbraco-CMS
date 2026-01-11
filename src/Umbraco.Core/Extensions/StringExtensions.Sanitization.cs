// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core;

namespace Umbraco.Extensions;

public static partial class StringExtensions
{
    internal static readonly Lazy<Regex> Whitespace = new(() => new Regex(@"\s+", RegexOptions.Compiled));

    internal static readonly string[] JsonEmpties = { "[]", "{}" };

    private static readonly char[] CleanForXssChars = "*?(){}[];:%<>/\\|&'\"".ToCharArray();

    // From: http://stackoverflow.com/a/961504/5018
    // filters control characters but allows only properly-formed surrogate sequences
    private static readonly Lazy<Regex> InvalidXmlChars = new(() =>
        new Regex(
            @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
            RegexOptions.Compiled));

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
    ///     Cleans string to aid in preventing xss attacks.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="ignoreFromClean"></param>
    /// <returns></returns>
    public static string CleanForXss(this string input, params char[] ignoreFromClean)
    {
        // remove any HTML
        input = input.StripHtml();

        // strip out any potential chars involved with XSS
        return input.ExceptChars(new HashSet<char>(CleanForXssChars.Except(ignoreFromClean)));
    }

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

    /// <summary>
    ///     An extension method that returns a new string in which all occurrences of an
    ///     unicode characters that are invalid in XML files are replaced with an empty string.
    /// </summary>
    /// <param name="text">Current instance of the string</param>
    /// <returns>Updated string</returns>
    /// <summary>
    ///     removes any unusual unicode characters that can't be encoded into XML
    /// </summary>
    public static string ToValidXmlString(this string text) =>
        string.IsNullOrEmpty(text) ? text : InvalidXmlChars.Value.Replace(text, string.Empty);

    /// <summary>
    ///     Turns an null-or-whitespace string into a null string.
    /// </summary>
    public static string? NullOrWhiteSpaceAsNull(this string? text)
        => string.IsNullOrWhiteSpace(text) ? null : text;

    /// <summary>
    ///     Checks if a given path is a full path including drive letter
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsFullPath(this string path) => Path.IsPathFullyQualified(path);

    /// <summary>
    ///     Checks whether a string is a valid email address.
    /// </summary>
    /// <param name="email">The string check</param>
    /// <returns>Returns a bool indicating whether the string is an email address.</returns>
    public static bool IsEmail(this string? email) =>
        string.IsNullOrWhiteSpace(email) is false && new EmailAddressAttribute().IsValid(email);
}

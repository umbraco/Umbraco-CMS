// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Extensions;

/// <summary>
/// String manipulation and modification extensions.
/// </summary>
public static partial class StringExtensions
{
    internal static readonly Lazy<Regex> Whitespace = new(() => new Regex(@"\s+", RegexOptions.Compiled));

    /// <summary>
    ///     Trims the specified value from a string; accepts a string input whereas the in-built implementation only accepts
    ///     char or char[].
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="forRemoving">For removing.</param>
    /// <returns></returns>
    public static string Trim(this string value, string forRemoving)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value.TrimEnd(forRemoving).TrimStart(forRemoving);
    }

    public static string TrimEnd(this string value, string forRemoving)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        if (string.IsNullOrEmpty(forRemoving))
        {
            return value;
        }

        while (value.EndsWith(forRemoving, StringComparison.InvariantCultureIgnoreCase))
        {
            value = value.Remove(value.LastIndexOf(forRemoving, StringComparison.InvariantCultureIgnoreCase));
        }

        return value;
    }

    public static string TrimStart(this string value, string forRemoving)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        if (string.IsNullOrEmpty(forRemoving))
        {
            return value;
        }

        while (value.StartsWith(forRemoving, StringComparison.InvariantCultureIgnoreCase))
        {
            value = value[forRemoving.Length..];
        }

        return value;
    }

    public static string EnsureStartsWith(this string input, string toStartWith)
    {
        if (input.StartsWith(toStartWith))
        {
            return input;
        }

        return toStartWith + input.TrimStart(toStartWith);
    }

    public static string EnsureStartsWith(this string input, char value) =>
        input.StartsWith(value.ToString(CultureInfo.InvariantCulture)) ? input : value + input;

    public static string EnsureEndsWith(this string input, char value) =>
        input.EndsWith(value.ToString(CultureInfo.InvariantCulture)) ? input : input + value;

    public static string EnsureEndsWith(this string input, string toEndWith) =>
        input.EndsWith(toEndWith.ToString(CultureInfo.InvariantCulture)) ? input : input + toEndWith;

    /// <summary>
    ///     Returns a copy of the string with the first character converted to uppercase.
    /// </summary>
    /// <param name="input">The string.</param>
    /// <returns>The converted string.</returns>
    public static string ToFirstUpper(this string input) =>
        string.IsNullOrWhiteSpace(input)
            ? input
            : input[..1].ToUpper() + input[1..];

    /// <summary>
    ///     Returns a copy of the string with the first character converted to lowercase.
    /// </summary>
    /// <param name="input">The string.</param>
    /// <returns>The converted string.</returns>
    public static string ToFirstLower(this string input) =>
        string.IsNullOrWhiteSpace(input)
            ? input
            : input[..1].ToLower() + input[1..];

    /// <summary>
    ///     Returns a copy of the string with the first character converted to uppercase using the casing rules of the
    ///     specified culture.
    /// </summary>
    /// <param name="input">The string.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The converted string.</returns>
    public static string ToFirstUpper(this string input, CultureInfo culture) =>
        string.IsNullOrWhiteSpace(input)
            ? input
            : input[..1].ToUpper(culture) + input[1..];

    /// <summary>
    ///     Returns a copy of the string with the first character converted to lowercase using the casing rules of the
    ///     specified culture.
    /// </summary>
    /// <param name="input">The string.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The converted string.</returns>
    public static string ToFirstLower(this string input, CultureInfo culture) =>
        string.IsNullOrWhiteSpace(input)
            ? input
            : input[..1].ToLower(culture) + input[1..];

    /// <summary>
    ///     Returns a copy of the string with the first character converted to uppercase using the casing rules of the
    ///     invariant culture.
    /// </summary>
    /// <param name="input">The string.</param>
    /// <returns>The converted string.</returns>
    public static string ToFirstUpperInvariant(this string input) =>
        string.IsNullOrWhiteSpace(input)
            ? input
            : input[..1].ToUpperInvariant() + input[1..];

    /// <summary>
    ///     Returns a copy of the string with the first character converted to lowercase using the casing rules of the
    ///     invariant culture.
    /// </summary>
    /// <param name="input">The string.</param>
    /// <returns>The converted string.</returns>
    public static string ToFirstLowerInvariant(this string input) =>
        string.IsNullOrWhiteSpace(input)
            ? input
            : input[..1].ToLowerInvariant() + input[1..];

    /// <summary>
    ///     Returns a new string in which all occurrences of specified strings are replaced by other specified strings.
    /// </summary>
    /// <param name="text">The string to filter.</param>
    /// <param name="replacements">The replacements definition.</param>
    /// <returns>The filtered string.</returns>
    public static string ReplaceMany(this string text, IDictionary<string, string> replacements)
    {
        if (text == null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        if (replacements == null)
        {
            throw new ArgumentNullException(nameof(replacements));
        }

        foreach (KeyValuePair<string, string> item in replacements)
        {
            text = text.Replace(item.Key, item.Value);
        }

        return text;
    }

    /// <summary>
    ///     Returns a new string in which all occurrences of specified characters are replaced by a specified character.
    /// </summary>
    /// <param name="text">The string to filter.</param>
    /// <param name="chars">The characters to replace.</param>
    /// <param name="replacement">The replacement character.</param>
    /// <returns>The filtered string.</returns>
    public static string ReplaceMany(this string text, char[] chars, char replacement)
    {
        if (text == null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        if (chars == null)
        {
            throw new ArgumentNullException(nameof(chars));
        }

        for (var i = 0; i < chars.Length; i++)
        {
            text = text.Replace(chars[i], replacement);
        }

        return text;
    }

    /// <summary>
    ///     Returns a new string in which only the first occurrence of a specified string is replaced by a specified
    ///     replacement string.
    /// </summary>
    /// <param name="text">The string to filter.</param>
    /// <param name="search">The string to replace.</param>
    /// <param name="replace">The replacement string.</param>
    /// <returns>The filtered string.</returns>
    public static string ReplaceFirst(this string text, string search, string replace)
    {
        if (text == null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        ReadOnlySpan<char> spanText = text.AsSpan();
        var pos = spanText.IndexOf(search, StringComparison.InvariantCulture);

        if (pos < 0)
        {
            return text;
        }

        return string.Concat(spanText[..pos], replace.AsSpan(), spanText[(pos + search.Length)..]);
    }

    /// <summary>
    ///     An extension method that returns a new string in which all occurrences of a
    ///     specified string in the current instance are replaced with another specified string.
    ///     StringComparison specifies the type of search to use for the specified string.
    /// </summary>
    /// <param name="source">Current instance of the string</param>
    /// <param name="oldString">Specified string to replace</param>
    /// <param name="newString">Specified string to inject</param>
    /// <param name="stringComparison">String Comparison object to specify search type</param>
    /// <returns>Updated string</returns>
    public static string Replace(this string source, string oldString, string newString, StringComparison stringComparison)
    {
        // This initialization ensures the first check starts at index zero of the source. On successive checks for
        // a match, the source is skipped to immediately after the last replaced occurrence for efficiency
        // and to avoid infinite loops when oldString and newString compare equal.
        var index = -1 * newString.Length;

        // Determine if there are any matches left in source, starting from just after the result of replacing the last match.
        while ((index = source.IndexOf(oldString, index + newString.Length, stringComparison)) >= 0)
        {
            // Remove the old text.
            source = source.Remove(index, oldString.Length);

            // Add the replacement text.
            source = source.Insert(index, newString);
        }

        return source;
    }

    public static string ReplaceNonAlphanumericChars(this string input, string replacement)
    {
        // any character that is not alphanumeric, convert to a hyphen
        var mName = input;
        foreach (var c in mName.ToCharArray().Where(c => !char.IsLetterOrDigit(c)))
        {
            mName = mName.Replace(c.ToString(CultureInfo.InvariantCulture), replacement);
        }

        return mName;
    }

    public static string ReplaceNonAlphanumericChars(this string input, char replacement)
    {
        var chars = input.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (!char.IsLetterOrDigit(chars[i]))
            {
                chars[i] = replacement;
            }
        }

        return new string(chars);
    }

    public static string ExceptChars(this string str, HashSet<char> toExclude)
    {
        var sb = new StringBuilder(str.Length);
        foreach (var c in str.Where(c => toExclude.Contains(c) == false))
        {
            sb.Append(c);
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Truncates the specified text string.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="maxLength">Length of the max.</param>
    /// <param name="suffix">The suffix.</param>
    /// <returns></returns>
    public static string Truncate(this string text, int maxLength, string suffix = "...")
    {
        // replaces the truncated string to a ...
        var truncatedString = text;

        if (maxLength <= 0)
        {
            return truncatedString;
        }

        var strLength = maxLength - suffix.Length;

        if (strLength <= 0)
        {
            return truncatedString;
        }

        if (text == null || text.Length <= maxLength)
        {
            return truncatedString;
        }

        truncatedString = text[..strLength];
        truncatedString = truncatedString.TrimEnd();
        truncatedString += suffix;

        return truncatedString;
    }

    /// <summary>
    ///     Removes new lines and tabs
    /// </summary>
    /// <param name="txt"></param>
    /// <returns></returns>
    public static string StripWhitespace(this string txt) => Whitespace.Value.Replace(txt, string.Empty);

    /// <summary>
    ///     Strips carrage returns and line feeds from the specified text.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns></returns>
    public static string StripNewLines(this string input) => input.Replace("\r", string.Empty).Replace("\n", string.Empty);

    /// <summary>
    ///     Converts to single line by replacing line breaks with spaces.
    /// </summary>
    public static string ToSingleLine(this string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        text = text.Replace("\r\n", " "); // remove CRLF
        text = text.Replace("\r", " "); // remove CR
        text = text.Replace("\n", " "); // remove LF
        return text;
    }

    // this is from SqlMetal and just makes it a bit of fun to allow pluralization
    public static string MakePluralName(this string name)
    {
        if (name.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
        {
            name += "es";
            return name;
        }

        if (name.EndsWith("y", StringComparison.OrdinalIgnoreCase) && name.Length > 1 &&
            !IsVowel(name[^2]))
        {
            name = name.Remove(name.Length - 1, 1);
            name += "ies";
            return name;
        }

        if (!name.EndsWith("s", StringComparison.OrdinalIgnoreCase))
        {
            name += "s";
        }

        return name;
    }

    public static bool IsVowel(this char c)
    {
        switch (c)
        {
            case 'O':
            case 'U':
            case 'Y':
            case 'A':
            case 'E':
            case 'I':
            case 'o':
            case 'u':
            case 'y':
            case 'a':
            case 'e':
            case 'i':
                return true;
        }

        return false;
    }

    public static bool IsLowerCase(this char ch) => ch.ToString(CultureInfo.InvariantCulture) ==
                                                    ch.ToString(CultureInfo.InvariantCulture).ToLowerInvariant();

    public static bool IsUpperCase(this char ch) => ch.ToString(CultureInfo.InvariantCulture) ==
                                                    ch.ToString(CultureInfo.InvariantCulture).ToUpperInvariant();

    // FORMAT STRINGS

    /// <summary>
    ///     Cleans a string to produce a string that can safely be used in an alias.
    /// </summary>
    /// <param name="alias">The text to filter.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <returns>The safe alias.</returns>
    public static string ToSafeAlias(this string alias, IShortStringHelper? shortStringHelper) =>
        shortStringHelper?.CleanStringForSafeAlias(alias) ?? string.Empty;

    /// <summary>
    ///     Cleans a string to produce a string that can safely be used in an alias.
    /// </summary>
    /// <param name="alias">The text to filter.</param>
    /// <param name="camel">A value indicating that we want to camel-case the alias.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <returns>The safe alias.</returns>
    public static string ToSafeAlias(this string alias, IShortStringHelper shortStringHelper, bool camel)
    {
        var a = shortStringHelper.CleanStringForSafeAlias(alias);
        if (string.IsNullOrWhiteSpace(a) || camel == false)
        {
            return a;
        }

        return char.ToLowerInvariant(a[0]) + a[1..];
    }

    /// <summary>
    ///     Cleans a string, in the context of a specified culture, to produce a string that can safely be used in an alias.
    /// </summary>
    /// <param name="alias">The text to filter.</param>
    /// <param name="culture">The culture.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <returns>The safe alias.</returns>
    public static string ToSafeAlias(this string alias, IShortStringHelper shortStringHelper, string culture) =>
        shortStringHelper.CleanStringForSafeAlias(alias, culture);

    // the new methods to get a url segment

    /// <summary>
    ///     Cleans a string to produce a string that can safely be used in an url segment.
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <returns>The safe url segment.</returns>
    public static string ToUrlSegment(this string text, IShortStringHelper shortStringHelper)
    {
        if (text == null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(text));
        }

        return shortStringHelper.CleanStringForUrlSegment(text);
    }

    /// <summary>
    ///     Cleans a string, in the context of a specified culture, to produce a string that can safely be used in an url
    ///     segment.
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The safe url segment.</returns>
    public static string ToUrlSegment(this string text, IShortStringHelper shortStringHelper, string? culture)
    {
        if (text == null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(text));
        }

        return shortStringHelper.CleanStringForUrlSegment(text, culture);
    }

    /// <summary>
    ///     Cleans a string.
    /// </summary>
    /// <param name="text">The text to clean.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="stringType">
    ///     A flag indicating the target casing and encoding of the string. By default,
    ///     strings are cleaned up to camelCase and Ascii.
    /// </param>
    /// <returns>The clean string.</returns>
    /// <remarks>The string is cleaned in the context of the ICurrent.ShortStringHelper default culture.</remarks>
    public static string ToCleanString(this string text, IShortStringHelper shortStringHelper, CleanStringType stringType) => shortStringHelper.CleanString(text, stringType);

    /// <summary>
    ///     Cleans a string, using a specified separator.
    /// </summary>
    /// <param name="text">The text to clean.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="stringType">
    ///     A flag indicating the target casing and encoding of the string. By default,
    ///     strings are cleaned up to camelCase and Ascii.
    /// </param>
    /// <param name="separator">The separator.</param>
    /// <returns>The clean string.</returns>
    /// <remarks>The string is cleaned in the context of the ICurrent.ShortStringHelper default culture.</remarks>
    public static string ToCleanString(this string text, IShortStringHelper shortStringHelper, CleanStringType stringType, char separator) => shortStringHelper.CleanString(text, stringType, separator);

    /// <summary>
    ///     Cleans a string in the context of a specified culture.
    /// </summary>
    /// <param name="text">The text to clean.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="stringType">
    ///     A flag indicating the target casing and encoding of the string. By default,
    ///     strings are cleaned up to camelCase and Ascii.
    /// </param>
    /// <param name="culture">The culture.</param>
    /// <returns>The clean string.</returns>
    public static string ToCleanString(this string text, IShortStringHelper shortStringHelper, CleanStringType stringType, string culture) => shortStringHelper.CleanString(text, stringType, culture);

    /// <summary>
    ///     Cleans a string in the context of a specified culture, using a specified separator.
    /// </summary>
    /// <param name="text">The text to clean.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="stringType">
    ///     A flag indicating the target casing and encoding of the string. By default,
    ///     strings are cleaned up to camelCase and Ascii.
    /// </param>
    /// <param name="separator">The separator.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>The clean string.</returns>
    public static string ToCleanString(this string text, IShortStringHelper shortStringHelper, CleanStringType stringType, char separator, string culture) =>
        shortStringHelper.CleanString(text, stringType, separator, culture);

    // note: LegacyCurrent.ShortStringHelper will produce 100% backward-compatible output for SplitPascalCasing.
    // other helpers may not. DefaultCurrent.ShortStringHelper produces better, but non-compatible, results.

    /// <summary>
    ///     Splits a Pascal cased string into a phrase separated by spaces.
    /// </summary>
    /// <param name="phrase">The text to split.</param>
    /// <param name="shortStringHelper"></param>
    /// <returns>The split text.</returns>
    public static string SplitPascalCasing(this string phrase, IShortStringHelper shortStringHelper) =>
        shortStringHelper.SplitPascalCasing(phrase, ' ');

    /// <summary>
    ///     Cleans a string, in the context of the invariant culture, to produce a string that can safely be used as a
    ///     filename,
    ///     both internally (on disk) and externally (as a url).
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <param name="shortStringHelper"></param>
    /// <returns>The safe filename.</returns>
    public static string ToSafeFileName(this string text, IShortStringHelper shortStringHelper) =>
        shortStringHelper.CleanStringForSafeFileName(text);

    // NOTE: Not sure what this actually does but is used a few places, need to figure it out and then move to StringExtensions and obsolete.
    // it basically is yet another version of SplitPascalCasing
    // plugging string extensions here to be 99% compatible
    // the only diff. is with numbers, Number6Is was "Number6 Is", and the new string helper does it too,
    // but the legacy one does "Number6Is"... assuming it is not a big deal.
    internal static string SpaceCamelCasing(this string phrase, IShortStringHelper shortStringHelper) =>
        phrase.Length < 2 ? phrase : phrase.SplitPascalCasing(shortStringHelper).ToFirstUpperInvariant();

    /// <summary>
    ///     Cleans a string, in the context of the invariant culture, to produce a string that can safely be used as a
    ///     filename,
    ///     both internally (on disk) and externally (as a url).
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <param name="shortStringHelper"></param>
    /// <param name="culture">The culture.</param>
    /// <returns>The safe filename.</returns>
    public static string ToSafeFileName(this string text, IShortStringHelper shortStringHelper, string culture) =>
        shortStringHelper.CleanStringForSafeFileName(text, culture);
}

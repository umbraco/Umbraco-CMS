// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Extensions;

public static partial class StringExtensions
{
    private const char DefaultEscapedStringEscapeChar = '\\';

    [GeneratedRegex(@"<[a-zA-Z/!][\s\S]*?>")]
    private static partial Regex StringHtmlRegex();

    /// <summary>
    /// Removes all whitespace characters including new lines, tabs, and spaces.
    /// </summary>
    /// <param name="txt">The string to strip whitespace from.</param>
    /// <returns>The string with all whitespace removed.</returns>
    public static string StripWhitespace(this string txt)
    {
        if (string.IsNullOrEmpty(txt))
        {
            return txt;
        }

        // Check if any whitespace exists to avoid allocating StringBuilder unless needed.
        var hasWhitespace = false;
        foreach (var c in txt)
        {
            if (char.IsWhiteSpace(c))
            {
                hasWhitespace = true;
                break;
            }
        }

        if (hasWhitespace is false)
        {
            return txt;
        }

        var sb = new StringBuilder(txt.Length);
        foreach (var c in txt)
        {
            if (!char.IsWhiteSpace(c))
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Strips the file extension from a file name.
    /// </summary>
    /// <param name="fileName">The file name to process.</param>
    /// <returns>The file name without the extension, or the original file name if no valid extension is found.</returns>
    public static string StripFileExtension(this string fileName)
    {
        // filenames cannot contain line breaks
        if (fileName.Contains('\n') || fileName.Contains('\r'))
        {
            return fileName;
        }

        ReadOnlySpan<char> spanFileName = fileName.AsSpan();
        var lastIndex = spanFileName.LastIndexOf('.');
        if (lastIndex > 0)
        {
            ReadOnlySpan<char> ext = spanFileName[lastIndex..];

            // file extensions cannot contain whitespace
            if (ext.Contains(' '))
            {
                return fileName;
            }

            return new string(spanFileName[..lastIndex]);
        }

        return fileName;
    }

    /// <summary>
    /// Determines the extension of the path or URL.
    /// </summary>
    /// <param name="file">The file path or URL to extract the extension from.</param>
    /// <returns>The file extension including the leading period, or an empty string if no extension is found.</returns>
    public static string GetFileExtension(this string file)
    {
        // Find any characters between the last . and the start of a query string or the end of the string
        const string pattern = @"(?<extension>\.[^\.\?]+)(\?.*|$)";
        Match match = Regex.Match(file, pattern);
        return match.Success
            ? match.Groups["extension"].Value
            : string.Empty;
    }

    /// <summary>
    /// Replaces all non-alphanumeric characters in a string with the specified replacement string.
    /// </summary>
    /// <param name="input">The string to process.</param>
    /// <param name="replacement">The string to replace non-alphanumeric characters with.</param>
    /// <returns>The string with all non-alphanumeric characters replaced.</returns>
    public static string ReplaceNonAlphanumericChars(this string input, string replacement)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var sb = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
            else
            {
                sb.Append(replacement);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Replaces all non-alphanumeric characters in a string with the specified replacement character.
    /// </summary>
    /// <param name="input">The string to process.</param>
    /// <param name="replacement">The character to replace non-alphanumeric characters with.</param>
    /// <returns>The string with all non-alphanumeric characters replaced.</returns>
    public static string ReplaceNonAlphanumericChars(this string input, char replacement)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

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

    /// <summary>
    /// Returns a new string with all characters from the specified exclusion set removed.
    /// </summary>
    /// <param name="str">The string to filter.</param>
    /// <param name="toExclude">The set of characters to remove from the string.</param>
    /// <returns>The filtered string with excluded characters removed.</returns>
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
    /// Appends one or more query strings to a URL.
    /// </summary>
    /// <param name="url">The base URL to append query strings to.</param>
    /// <param name="queryStrings">The query strings to append.</param>
    /// <returns>The URL with the query strings appended.</returns>
    /// <remarks>
    /// This method ensures that the resulting URL is structured correctly, that there is only one '?' and that
    /// parameters are delimited properly with '&amp;'.
    /// </remarks>
    public static string AppendQueryStringToUrl(this string url, params string[] queryStrings)
    {
        // remove any prefixed '&' or '?'
        for (var i = 0; i < queryStrings.Length; i++)
        {
            queryStrings[i] = queryStrings[i].TrimStart(Constants.CharArrays.QuestionMarkAmpersand)
                .TrimEnd(Constants.CharArrays.Ampersand);
        }

        var nonEmpty = queryStrings.Where(x => !x.IsNullOrWhiteSpace()).ToArray();

        if (url.Contains('?'))
        {
            return url + string.Join("&", nonEmpty).EnsureStartsWith('&');
        }

        return url + string.Join("&", nonEmpty).EnsureStartsWith('?');
    }

    /// <summary>
    /// Converts a singular noun to its plural form using common English pluralisation rules.
    /// </summary>
    /// <param name="name">The singular noun to pluralise.</param>
    /// <returns>The plural form of the noun.</returns>
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
            // Change the y to i and add es.
            name = name[..^1];
            name += "ies";
            return name;
        }

        if (!name.EndsWith("s", StringComparison.OrdinalIgnoreCase))
        {
            name += "s";
        }

        return name;
    }

    /// <summary>
    /// Determines whether the specified character is a vowel (A, E, I, O, U, Y).
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns><c>true</c> if the character is a vowel; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Trims the specified string from both the start and end of the value.
    /// </summary>
    /// <param name="value">The string to trim.</param>
    /// <param name="forRemoving">The string to remove from both ends.</param>
    /// <returns>The trimmed string.</returns>
    /// <remarks>
    /// This method accepts a string input whereas the built-in implementation only accepts char or char[].
    /// </remarks>
    public static string Trim(this string value, string forRemoving)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value.TrimEnd(forRemoving).TrimStart(forRemoving);
    }

    /// <summary>
    /// Trims the specified string from the end of the value.
    /// </summary>
    /// <param name="value">The string to trim.</param>
    /// <param name="forRemoving">The string to remove from the end.</param>
    /// <returns>The trimmed string.</returns>
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

        // Trim all occurrences from the end.
        while (value.EndsWith(forRemoving, StringComparison.InvariantCultureIgnoreCase))
        {
            value = value[..value.LastIndexOf(forRemoving, StringComparison.InvariantCultureIgnoreCase)];
        }

        return value;
    }

    /// <summary>
    /// Trims the specified string from the start of the value.
    /// </summary>
    /// <param name="value">The string to trim.</param>
    /// <param name="forRemoving">The string to remove from the start.</param>
    /// <returns>The trimmed string.</returns>
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

    /// <summary>
    /// Ensures that the string starts with the specified string.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <param name="toStartWith">The string that the input should start with.</param>
    /// <returns>The input string if it already starts with the specified value; otherwise, the value prepended to the input.</returns>
    public static string EnsureStartsWith(this string input, string toStartWith) =>
        input.StartsWith(toStartWith) ? input : toStartWith + input;

    /// <summary>
    /// Ensures that the string starts with the specified character.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <param name="value">The character that the input should start with.</param>
    /// <returns>The input string if it already starts with the specified character; otherwise, the character prepended to the input.</returns>
    public static string EnsureStartsWith(this string input, char value) =>
        input.Length > 0 && input[0] == value ? input : value + input;

    /// <summary>
    /// Ensures that the string ends with the specified string.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <param name="toEndWith">The string that the input should end with.</param>
    /// <returns>The input string if it already ends with the specified value; otherwise, the value appended to the input.</returns>
    public static string EnsureEndsWith(this string input, string toEndWith) =>
        input.EndsWith(toEndWith) ? input : input + toEndWith;

    /// <summary>
    /// Ensures that the string ends with the specified character.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <param name="value">The character that the input should end with.</param>
    /// <returns>The input string if it already ends with the specified character; otherwise, the character appended to the input.</returns>
    public static string EnsureEndsWith(this string input, char value) =>
        input.Length > 0 && input[^1] == value ? input : input + value;

    /// <summary>
    /// Splits a delimited string into a list of strings.
    /// </summary>
    /// <param name="list">The delimited string to split.</param>
    /// <param name="delimiter">The delimiter used to separate items. Defaults to comma.</param>
    /// <returns>A list of strings from the delimited input, with empty entries removed and items trimmed.</returns>
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
    /// Strips all HTML tags from a string.
    /// </summary>
    /// <param name="text">The text to strip HTML from.</param>
    /// <returns>The string with all HTML tags removed.</returns>
    public static string StripHtml(this string text)
    {
        // Match valid HTML tags: must start with letter, /, or ! after <.
        // This avoids matching empty <> or mathematical expressions like "5 < 10 > 3".
        const string pattern = @"<[a-zA-Z/!][\s\S]*?>";
        return StringHtmlRegex().Replace(text, string.Empty);
    }

    /// <summary>
    /// Ensures that the folder path ends with a directory separator character.
    /// </summary>
    /// <param name="currentFolder">The folder path to normalise.</param>
    /// <returns>The folder path with a trailing directory separator character.</returns>
    public static string NormaliseDirectoryPath(this string currentFolder)
    {
        currentFolder = currentFolder
            .IfNull(x => string.Empty)
            .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return currentFolder;
    }

    /// <summary>
    /// Truncates a string to the specified maximum length and appends a suffix.
    /// </summary>
    /// <param name="text">The text to truncate.</param>
    /// <param name="maxLength">The maximum length of the resulting string including the suffix.</param>
    /// <param name="suffix">The suffix to append when truncation occurs. Defaults to "...".</param>
    /// <returns>The truncated string with suffix, or the original string if it does not exceed the maximum length.</returns>
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
    /// Strips carriage returns and line feeds from the specified text.
    /// </summary>
    /// <param name="input">The string to process.</param>
    /// <returns>The string with all carriage returns and line feeds removed.</returns>
    public static string StripNewLines(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Check if any newlines exist to avoid allocating the StringBuilder unnecessarily.
        if (input.IndexOf('\r') < 0 && input.IndexOf('\n') < 0)
        {
            return input;
        }

        var sb = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            if (c is not '\r' and not '\n')
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts a multi-line string to a single line by replacing line breaks with spaces.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>The text as a single line, or the original text if it is null or empty.</returns>
    public static string ToSingleLine(this string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        // Check if any newlines exist to avoid allocating the StringBuilder unnecessarily.
        if (text.IndexOf('\r') < 0 && text.IndexOf('\n') < 0)
        {
            return text;
        }

        var sb = new StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '\r')
            {
                sb.Append(' ');

                // Skip the \n if this is a \r\n pair.
                if (i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }
            }
            else if (c == '\n')
            {
                sb.Append(' ');
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns a new string in which all occurrences of specified strings are replaced by other specified strings.
    /// </summary>
    /// <param name="text">The string to filter.</param>
    /// <param name="replacements">A dictionary mapping strings to find to their replacement values.</param>
    /// <returns>The filtered string with all replacements applied.</returns>
    /// <exception cref="ArgumentNullException">Thrown when text or replacements is null.</exception>
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
    /// Returns a new string in which all occurrences of specified characters are replaced by a specified character.
    /// </summary>
    /// <param name="text">The string to filter.</param>
    /// <param name="chars">The characters to replace.</param>
    /// <param name="replacement">The replacement character.</param>
    /// <returns>The filtered string with all specified characters replaced.</returns>
    /// <exception cref="ArgumentNullException">Thrown when text or chars is null.</exception>
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
    /// Returns a new string in which only the first occurrence of a specified string is replaced by a specified replacement string.
    /// </summary>
    /// <param name="text">The string to filter.</param>
    /// <param name="search">The string to search for.</param>
    /// <param name="replace">The replacement string.</param>
    /// <returns>The filtered string with the first occurrence replaced.</returns>
    /// <exception cref="ArgumentNullException">Thrown when text is null.</exception>
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
    /// Returns a new string in which all occurrences of a specified string in the current instance are replaced with
    /// another specified string, using the specified comparison type.
    /// </summary>
    /// <param name="source">The current string instance.</param>
    /// <param name="oldString">The string to replace.</param>
    /// <param name="newString">The replacement string.</param>
    /// <param name="stringComparison">The type of string comparison to use when searching.</param>
    /// <returns>The string with all occurrences replaced.</returns>
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

    /// <summary>
    /// Escapes all regular expression special characters in the string by prefixing them with a backslash.
    /// </summary>
    /// <param name="text">The string to escape.</param>
    /// <returns>The string with all regex special characters escaped.</returns>
    public static string EscapeRegexSpecialCharacters(this string text)
    {
        var regexSpecialCharacters = new Dictionary<string, string>
        {
            { ".", @"\." },
            { "(", @"\(" },
            { ")", @"\)" },
            { "]", @"\]" },
            { "[", @"\[" },
            { "{", @"\{" },
            { "}", @"\}" },
            { "?", @"\?" },
            { "!", @"\!" },
            { "$", @"\$" },
            { "^", @"\^" },
            { "+", @"\+" },
            { "*", @"\*" },
            { "|", @"\|" },
            { "<", @"\<" },
            { ">", @"\>" },
        };
        return ReplaceMany(text, regexSpecialCharacters);
    }

    /// <summary>
    /// Converts a file name to a friendly name suitable for content items.
    /// </summary>
    /// <param name="fileName">The file name to convert.</param>
    /// <returns>A friendly name with the extension stripped, underscores and dashes converted to spaces, and title case applied.</returns>
    public static string ToFriendlyName(this string fileName)
    {
        // strip the file extension
        fileName = fileName.StripFileExtension();

        // underscores and dashes to spaces
        fileName = fileName.ReplaceMany(Constants.CharArrays.UnderscoreDash, ' ');

        // any other conversions ?

        // Pascalcase (to be done last)
        fileName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(fileName);

        // Replace multiple consecutive spaces with a single space
        fileName = string.Join(" ", fileName.Split(Constants.CharArrays.Space, StringSplitOptions.RemoveEmptyEntries));

        return fileName;
    }

    // FORMAT STRINGS

    /// <summary>
    /// Cleans a string to produce a string that can safely be used in an alias.
    /// </summary>
    /// <param name="alias">The text to filter.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <returns>The safe alias.</returns>
    public static string ToSafeAlias(this string alias, IShortStringHelper? shortStringHelper) =>
        shortStringHelper?.CleanStringForSafeAlias(alias) ?? string.Empty;

    /// <summary>
    /// Cleans a string to produce a string that can safely be used in an alias.
    /// </summary>
    /// <param name="alias">The text to filter.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="camel">A value indicating whether to camel-case the alias.</param>
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
    /// Cleans a string, in the context of a specified culture, to produce a string that can safely be used in an alias.
    /// </summary>
    /// <param name="alias">The text to filter.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="culture">The culture code.</param>
    /// <returns>The safe alias.</returns>
    public static string ToSafeAlias(this string alias, IShortStringHelper shortStringHelper, string culture) =>
        shortStringHelper.CleanStringForSafeAlias(alias, culture);

    // the new methods to get a url segment

    /// <summary>
    /// Cleans a string to produce a string that can safely be used in a URL segment.
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <returns>The safe URL segment.</returns>
    /// <exception cref="ArgumentNullException">Thrown when text is null.</exception>
    /// <exception cref="ArgumentException">Thrown when text is empty or consists only of white-space characters.</exception>
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
    /// Cleans a string, in the context of a specified culture, to produce a string that can safely be used in a URL segment.
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="culture">The culture code.</param>
    /// <returns>The safe URL segment.</returns>
    /// <exception cref="ArgumentNullException">Thrown when text is null.</exception>
    /// <exception cref="ArgumentException">Thrown when text is empty or consists only of white-space characters.</exception>
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
    /// Cleans a string according to the specified string type.
    /// </summary>
    /// <param name="text">The text to clean.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="stringType">
    /// A flag indicating the target casing and encoding of the string. By default,
    /// strings are cleaned up to camelCase and ASCII.
    /// </param>
    /// <returns>The cleaned string.</returns>
    /// <remarks>The string is cleaned in the context of the short string helper's default culture.</remarks>
    public static string ToCleanString(this string text, IShortStringHelper shortStringHelper, CleanStringType stringType) => shortStringHelper.CleanString(text, stringType);

    /// <summary>
    /// Cleans a string according to the specified string type, using a specified separator.
    /// </summary>
    /// <param name="text">The text to clean.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="stringType">
    /// A flag indicating the target casing and encoding of the string. By default,
    /// strings are cleaned up to camelCase and ASCII.
    /// </param>
    /// <param name="separator">The separator character to use.</param>
    /// <returns>The cleaned string.</returns>
    /// <remarks>The string is cleaned in the context of the short string helper's default culture.</remarks>
    public static string ToCleanString(this string text, IShortStringHelper shortStringHelper, CleanStringType stringType, char separator) => shortStringHelper.CleanString(text, stringType, separator);

    /// <summary>
    /// Cleans a string according to the specified string type, in the context of a specified culture.
    /// </summary>
    /// <param name="text">The text to clean.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="stringType">
    /// A flag indicating the target casing and encoding of the string. By default,
    /// strings are cleaned up to camelCase and ASCII.
    /// </param>
    /// <param name="culture">The culture code.</param>
    /// <returns>The cleaned string.</returns>
    public static string ToCleanString(this string text, IShortStringHelper shortStringHelper, CleanStringType stringType, string culture) => shortStringHelper.CleanString(text, stringType, culture);

    /// <summary>
    /// Cleans a string according to the specified string type, in the context of a specified culture, using a specified separator.
    /// </summary>
    /// <param name="text">The text to clean.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="stringType">
    /// A flag indicating the target casing and encoding of the string. By default,
    /// strings are cleaned up to camelCase and ASCII.
    /// </param>
    /// <param name="separator">The separator character to use.</param>
    /// <param name="culture">The culture code.</param>
    /// <returns>The cleaned string.</returns>
    public static string ToCleanString(this string text, IShortStringHelper shortStringHelper, CleanStringType stringType, char separator, string culture) =>
        shortStringHelper.CleanString(text, stringType, separator, culture);

    // note: LegacyCurrent.ShortStringHelper will produce 100% backward-compatible output for SplitPascalCasing.
    // other helpers may not. DefaultCurrent.ShortStringHelper produces better, but non-compatible, results.

    /// <summary>
    /// Splits a Pascal-cased string into a phrase separated by spaces.
    /// </summary>
    /// <param name="phrase">The text to split.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <returns>The split text with spaces between words.</returns>
    public static string SplitPascalCasing(this string phrase, IShortStringHelper shortStringHelper) =>
        shortStringHelper.SplitPascalCasing(phrase, ' ');

    /// <summary>
    /// Cleans a string, in the context of the invariant culture, to produce a string that can safely be used as a filename,
    /// both internally (on disk) and externally (as a URL).
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <returns>The safe filename.</returns>
    public static string ToSafeFileName(this string text, IShortStringHelper shortStringHelper) =>
        shortStringHelper.CleanStringForSafeFileName(text);

    // NOTE: Not sure what this actually does but is used a few places, need to figure it out and then move to StringExtensions and obsolete.
    // it basically is yet another version of SplitPascalCasing
    // plugging string extensions here to be 99% compatible
    // the only diff. is with numbers, Number6Is was "Number6 Is", and the new string helper does it too,
    // but the legacy one does "Number6Is"... assuming it is not a big deal.

    /// <summary>
    /// Splits a Pascal-cased string into words separated by spaces and converts the first character to uppercase.
    /// </summary>
    /// <param name="phrase">The phrase to process.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <returns>The phrase with words separated by spaces and first character in uppercase.</returns>
    internal static string SpaceCamelCasing(this string phrase, IShortStringHelper shortStringHelper) =>
        phrase.Length < 2 ? phrase : phrase.SplitPascalCasing(shortStringHelper).ToFirstUpperInvariant();

    /// <summary>
    /// Cleans a string, in the context of a specified culture, to produce a string that can safely be used as a filename,
    /// both internally (on disk) and externally (as a URL).
    /// </summary>
    /// <param name="text">The text to filter.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="culture">The culture code.</param>
    /// <returns>The safe filename.</returns>
    public static string ToSafeFileName(this string text, IShortStringHelper shortStringHelper, string culture) =>
        shortStringHelper.CleanStringForSafeFileName(text, culture);

    /// <summary>
    /// Splits a string on a specified character while supporting an escape character to include the split character in values.
    /// </summary>
    /// <param name="value">The string to split.</param>
    /// <param name="splitChar">The character to split on.</param>
    /// <param name="escapeChar">The character used to escape the split character. Defaults to backslash.</param>
    /// <returns>An enumerable of substrings delimited by the split character, with escape sequences resolved.</returns>
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
}

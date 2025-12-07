// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core;

namespace Umbraco.Extensions;

/// <summary>
/// XSS, HTML, file path, and sanitization extensions.
/// </summary>
public static partial class StringExtensions
{
    private static readonly char[] CleanForXssChars = "*?(){}[];:%<>/\\|&'\"".ToCharArray();

    // From: http://stackoverflow.com/a/961504/5018
    // filters control characters but allows only properly-formed surrogate sequences
    private static readonly Lazy<Regex> InvalidXmlChars = new(() =>
        new Regex(
            @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
            RegexOptions.Compiled));

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
    ///     Strips all HTML from a string.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>Returns the string without any HTML tags.</returns>
    public static string StripHtml(this string text)
    {
        const string pattern = @"<(.|\n)*?>";
        return Regex.Replace(text, pattern, string.Empty, RegexOptions.Compiled);
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
    ///     Determines the extension of the path or URL
    /// </summary>
    /// <param name="file"></param>
    /// <returns>Extension of the file</returns>
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
    ///     Ensures that the folder path ends with a DirectorySeparatorChar
    /// </summary>
    /// <param name="currentFolder"></param>
    /// <returns></returns>
    public static string NormaliseDirectoryPath(this string currentFolder)
    {
        currentFolder = currentFolder
            .IfNull(x => string.Empty)
            .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return currentFolder;
    }

    /// <summary>
    ///     Checks if a given path is a full path including drive letter
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsFullPath(this string path) => Path.IsPathFullyQualified(path);

    /// <summary>
    ///     This will append the query string to the URL
    /// </summary>
    /// <param name="url"></param>
    /// <param name="queryStrings"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This methods ensures that the resulting URL is structured correctly, that there's only one '?' and that things are
    ///     delimited properly with '&amp;'
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
    ///     Converts a file name to a friendly name for a content item
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
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

    /// <summary>
    ///     Checks whether a string is a valid email address.
    /// </summary>
    /// <param name="email">The string check</param>
    /// <returns>Returns a bool indicating whether the string is an email address.</returns>
    public static bool IsEmail(this string? email) =>
        string.IsNullOrWhiteSpace(email) is false && new EmailAddressAttribute().IsValid(email);

    /// <summary>
    ///     Returns a stream from a string
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    internal static Stream GenerateStreamFromString(this string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}

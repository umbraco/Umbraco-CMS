using System.Text.RegularExpressions;

namespace Umbraco.Cms.Search.Extension.FileToText.Extensions;

internal static partial class StringExtensions
{
    public static string TrimWhitespaceAndNewlines(this string value)
        => TrimWhitespaceAndNewlinesRegex().Replace(value.ReplaceLineEndings(" "), " ");

    [GeneratedRegex("\\s{2,}")]
    private static partial Regex TrimWhitespaceAndNewlinesRegex();
}

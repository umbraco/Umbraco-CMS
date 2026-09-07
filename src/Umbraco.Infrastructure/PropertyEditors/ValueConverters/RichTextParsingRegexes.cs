using System.Text.RegularExpressions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

internal static partial class RichTextParsingRegexes
{
    /// <summary>
    /// Returns a regular expression that matches Umbraco RTE block elements, including optional inline variants and class attributes.
    /// </summary>
    /// <returns>A <see cref="Regex"/> for matching Umbraco RTE block elements.</returns>
    [GeneratedRegex("<umb-rte-block(?:-inline)?(?: class=\"(.[^\"]*)\")? data-content-key=\"(?<key>.[^\"]*)\">(?:<!--Umbraco-Block-->)?<\\/umb-rte-block(?:-inline)?>")]
    public static partial Regex BlockRegex();
}

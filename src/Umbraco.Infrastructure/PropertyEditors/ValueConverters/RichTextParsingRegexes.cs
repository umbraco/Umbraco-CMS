using System.Text.RegularExpressions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

internal static partial class RichTextParsingRegexes
{
    /// <summary>
    /// Returns a regular expression that matches Umbraco RTE block elements, including optional inline variants and any other attributes (e.g. class, data-key) around data-content-key.
    /// </summary>
    /// <returns>A <see cref="Regex"/> for matching Umbraco RTE block elements.</returns>
    [GeneratedRegex("<umb-rte-block(?:-inline)?(?:\\s+[\\w-]+=\"[^\"]*\")*?\\s+data-content-key=\"(?<key>[^\"]+)\"(?:\\s+[\\w-]+=\"[^\"]*\")*\\s*>(?:<!--Umbraco-Block-->)?<\\/umb-rte-block(?:-inline)?>")]
    public static partial Regex BlockRegex();
}

using System.Text.RegularExpressions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

internal static partial class RichTextParsingRegexes
{
    [GeneratedRegex("<umb-rte-block(?:-inline)?(?: class=\"(.[^\"]*)\")? data-content-key=\"(?<key>.[^\"]*)\">(?:<!--Umbraco-Block-->)?<\\/umb-rte-block(?:-inline)?>")]
    public static partial Regex BlockRegex();
}

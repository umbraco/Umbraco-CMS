using System.Text.RegularExpressions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

internal static partial class RichTextParsingRegexes
{
    [GeneratedRegex("<umb-rte-block.*data-content-udi=\"(?<udi>.[^\\\"]*)\">.*<\\/umb-rte-block>")]
    public static partial Regex BlockRegex();
}

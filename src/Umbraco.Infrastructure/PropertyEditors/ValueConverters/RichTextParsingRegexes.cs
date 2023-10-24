using System.Text.RegularExpressions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

internal static partial class RichTextParsingRegexes
{
    [GeneratedRegex("<umb-rte-block(?:-inline)?.*data-content-udi=\"(?<udi>.[^\"]*)\">.*<\\/umb-rte-block(?:-inline)?>")]
    public static partial Regex BlockRegex();
}

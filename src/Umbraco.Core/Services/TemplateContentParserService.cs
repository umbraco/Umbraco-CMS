using System.Text.RegularExpressions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public partial class TemplateContentParserService : ITemplateContentParserService
{
    public string? MasterTemplateAlias(string? viewContent)
    {
        if (viewContent.IsNullOrWhiteSpace())
        {
            return null;
        }

        Match match = LayoutRegex().Match(viewContent);

        if (match.Success == false || match.Groups.TryGetValue("layout", out Group? layoutGroup) == false)
        {
            return null;
        }

        var layout = layoutGroup.Value;
        return layout != "null"
            ? layout.Replace(".cshtml", string.Empty, StringComparison.OrdinalIgnoreCase)
            : null;
    }

    [GeneratedRegex("\\s*Layout\\s*=\\s*\"?(?<layout>[\\w\\s\\.]*)\"?;", RegexOptions.Compiled)]
    private static partial Regex LayoutRegex();
}

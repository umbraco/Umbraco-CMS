using Umbraco.Cms.Core.Strings.Css;

namespace Umbraco.Cms.Core.Models;

public class RichTextStylesheetData
{
    public string? Content { get; set; }

    public StylesheetRule[] Rules { get; set; } = Array.Empty<StylesheetRule>();
}

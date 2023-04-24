namespace Umbraco.Cms.Api.Management.ViewModels.RichTextStylesheet;

public class InterpolateRichTextStylesheetRequestModel
{
    public string? Content { get; set; }

    public IEnumerable<RichTextRuleViewModel>? Rules { get; set; }
}

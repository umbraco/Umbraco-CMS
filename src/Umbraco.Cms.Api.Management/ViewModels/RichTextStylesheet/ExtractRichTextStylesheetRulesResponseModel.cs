namespace Umbraco.Cms.Api.Management.ViewModels.RichTextStylesheet;

public class ExtractRichTextStylesheetRulesResponseModel
{
    public required IEnumerable<RichTextRuleViewModel> Rules { get; set; } = Enumerable.Empty<RichTextRuleViewModel>();
}

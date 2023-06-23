namespace Umbraco.Cms.Api.Management.ViewModels.RichTextStylesheet;

public class RichTextStylesheetRulesResponseModel
{
    public required IEnumerable<RichTextRuleViewModel> Rules { get; set; } = Enumerable.Empty<RichTextRuleViewModel>();
}

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings.Css;

namespace Umbraco.Cms.Core.Services;

public interface IRichTextStylesheetService
{
    Task<string> InterpolateRichTextRules(RichTextStylesheetData data);

    Task<IEnumerable<StylesheetRule>> ExtractRichTextRules(RichTextStylesheetData data);

    Task<Attempt<IEnumerable<StylesheetRule>, StylesheetOperationStatus>> GetRulesByPathAsync(string path);
}

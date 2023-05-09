using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.RichTextStylesheet;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using StylesheetRule = Umbraco.Cms.Core.Strings.Css.StylesheetRule;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet;

[ApiVersion("1.0")]
public class GetRichTextRulesByPath : StylesheetControllerBase
{
    private readonly IRichTextStylesheetService _richTextStylesheetService;
    private readonly IUmbracoMapper _umbracoMapper;

    public GetRichTextRulesByPath(
        IRichTextStylesheetService richTextStylesheetService,
        IUmbracoMapper umbracoMapper)
    {
        _richTextStylesheetService = richTextStylesheetService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("rich-text/rules")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(RichTextStylesheetRulesResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPath(string path)
    {
        Attempt<IEnumerable<StylesheetRule>, StylesheetOperationStatus> rulesAttempt = await _richTextStylesheetService.GetRulesByPathAsync(path);

        if (rulesAttempt.Success is false)
        {
            return StylesheetOperationStatusResult(rulesAttempt.Status);
        }

        return Ok(new RichTextStylesheetRulesResponseModel
        {
            Rules = _umbracoMapper.MapEnumerable<StylesheetRule, RichTextRuleViewModel>(rulesAttempt.Result)
        });
    }
}

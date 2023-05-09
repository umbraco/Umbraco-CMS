using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.RichTextStylesheet;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings.Css;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet;

[ApiVersion("1.0")]
public class ExtractRichTextRulesController : StylesheetControllerBase
{
    private readonly IRichTextStylesheetService _richTextStylesheetService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ExtractRichTextRulesController(
        IRichTextStylesheetService richTextStylesheetService,
        IUmbracoMapper umbracoMapper)
    {
        _richTextStylesheetService = richTextStylesheetService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpPost("rich-text/extract-rules")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ExtractRichTextStylesheetRulesResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExtractRichTextRules(ExtractRichTextStylesheetRulesRequestModel requestModel)
    {
        RichTextStylesheetData? model = _umbracoMapper.Map<RichTextStylesheetData>(requestModel);

        IEnumerable<StylesheetRule> rules = await _richTextStylesheetService.ExtractRichTextRules(model!);

        return Ok(new ExtractRichTextStylesheetRulesResponseModel
        {
            Rules = _umbracoMapper.MapEnumerable<StylesheetRule, RichTextRuleViewModel>(rules),
        });
    }
}

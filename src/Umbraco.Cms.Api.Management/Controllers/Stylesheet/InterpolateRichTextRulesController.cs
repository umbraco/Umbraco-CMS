using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.RichTextStylesheet;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet;

[ApiVersion("1.0")]
public class InterpolateRichTextRulesController : StylesheetControllerBase
{
    private readonly IRichTextStylesheetService _richTextStylesheetService;
    private readonly IUmbracoMapper _umbracoMapper;

    public InterpolateRichTextRulesController(
        IRichTextStylesheetService richTextStylesheetService,
        IUmbracoMapper umbracoMapper)
    {
        _richTextStylesheetService = richTextStylesheetService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpPost("rich-text/interpolate-rules")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(InterpolateRichTextStylesheetResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> InterpolateRichTextRules(InterpolateRichTextStylesheetRequestModel requestModel)
    {
        RichTextStylesheetData? model = _umbracoMapper.Map<RichTextStylesheetData>(requestModel);

        var content = await _richTextStylesheetService.InterpolateRichTextRules(model!);

        return Ok(new InterpolateRichTextStylesheetResponseModel
        {
            Content = content,
        });
    }
}

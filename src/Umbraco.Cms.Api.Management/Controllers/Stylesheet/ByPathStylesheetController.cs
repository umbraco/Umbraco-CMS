using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Stylesheet;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet;

[ApiVersion("1.0")]
public class ByPathStylesheetController : StylesheetControllerBase
{
    private readonly IStylesheetService _stylesheetService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByPathStylesheetController(
        IStylesheetService stylesheetService,
        IUmbracoMapper umbracoMapper)
    {
        _stylesheetService = stylesheetService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(StylesheetResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> ByPath(string path)
    {
        IStylesheet? stylesheet = await _stylesheetService.GetAsync(path);

        if (stylesheet is null)
        {
            return StylesheetNotFound();
        }

        StylesheetResponseModel? viewModel = _umbracoMapper.Map<StylesheetResponseModel>(stylesheet);

        return Ok(viewModel);
    }
}

using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
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

    [HttpGet("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(StylesheetResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByPath(CancellationToken cancellationToken, string path)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        IStylesheet? stylesheet = await _stylesheetService.GetAsync(path);

        return stylesheet is not null
            ? Ok(_umbracoMapper.Map<StylesheetResponseModel>(stylesheet))
            : StylesheetNotFound();
    }
}

using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.Stylesheet;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet;

    /// <summary>
    /// Provides API endpoints for managing stylesheets identified by their path.
    /// </summary>
[ApiVersion("1.0")]
public class ByPathStylesheetController : StylesheetControllerBase
{
    private readonly IStylesheetService _stylesheetService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Stylesheet.ByPathStylesheetController"/> class, which manages stylesheet resources by their path.
    /// </summary>
    /// <param name="stylesheetService">Service used to manage and retrieve stylesheet entities.</param>
    /// <param name="umbracoMapper">The mapper used to convert between Umbraco domain models and API models.</param>
    public ByPathStylesheetController(
        IStylesheetService stylesheetService,
        IUmbracoMapper umbracoMapper)
    {
        _stylesheetService = stylesheetService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves a stylesheet resource identified by the specified file path.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="path">The virtual or system file path of the stylesheet to retrieve.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="StylesheetResponseModel"/> if the stylesheet is found;
    /// otherwise, a <see cref="NotFoundResult"/> with problem details.
    /// </returns>
    [HttpGet("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(StylesheetResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a stylesheet by path.")]
    [EndpointDescription("Gets a stylesheet identified by the provided file path.")]
    public async Task<IActionResult> ByPath(CancellationToken cancellationToken, string path)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        IStylesheet? stylesheet = await _stylesheetService.GetAsync(path);

        return stylesheet is not null
            ? Ok(_umbracoMapper.Map<StylesheetResponseModel>(stylesheet))
            : StylesheetNotFound();
    }
}

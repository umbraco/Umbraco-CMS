using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.PartialView;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView;

    /// <summary>
    /// Controller for managing partial views identified by their path.
    /// </summary>
[ApiVersion("1.0")]
public class ByPathPartialViewController : PartialViewControllerBase
{
    private readonly IPartialViewService _partialViewService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByPathPartialViewController"/> class, providing services for managing partial views by path.
    /// </summary>
    /// <param name="partialViewService">Service used to manage and retrieve partial views.</param>
    /// <param name="mapper">The Umbraco object mapper used for mapping between domain and API models.</param>
    public ByPathPartialViewController(
        IPartialViewService partialViewService,
        IUmbracoMapper mapper)
    {
        _partialViewService = partialViewService;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a partial view based on the specified file path.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="path">The file path of the partial view to retrieve.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="PartialViewResponseModel"/> with the partial view data if found; otherwise, a <see cref="ProblemDetails"/> result with a 404 status code if the partial view does not exist.
    /// </returns>
    [HttpGet("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PartialViewResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a partial view by path.")]
    [EndpointDescription("Gets a partial view identified by the provided file path.")]
    public async Task<IActionResult> ByPath(CancellationToken cancellationToken, string path)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        IPartialView? partialView = await _partialViewService.GetAsync(path);

        return partialView is not null
            ? Ok(_mapper.Map<PartialViewResponseModel>(partialView))
            : PartialViewNotFound();
    }
}

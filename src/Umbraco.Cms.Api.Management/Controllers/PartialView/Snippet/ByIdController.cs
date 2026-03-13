using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Snippets;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Snippets;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Snippet;

/// <summary>
/// Controller for retrieving and managing partial view snippets by their unique identifier.
/// </summary>
[ApiVersion("1.0")]
public class ByIdController : PartialViewControllerBase
{
    private readonly IPartialViewService _partialViewService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.PartialView.Snippet.ByIdController"/> class.
    /// </summary>
    /// <param name="partialViewService">Service for managing partial views.</param>
    /// <param name="umbracoMapper">The mapper used for mapping Umbraco objects.</param>
    public ByIdController(IPartialViewService partialViewService, IUmbracoMapper umbracoMapper)
    {
        _partialViewService = partialViewService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>Gets a partial view snippet identified by the provided Id.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="id">The identifier of the partial view snippet.</param>
    /// <returns>An <see cref="IActionResult"/> containing the partial view snippet response model if found; otherwise, a not found response.</returns>
    [HttpGet("snippet/{id}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PartialViewSnippetResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a partial view snippet.")]
    [EndpointDescription("Gets a partial view snippet identified by the provided Id.")]
    public async Task<IActionResult> GetById(
        CancellationToken cancellationToken,
        string id)
    {
        PartialViewSnippet? snippet = await _partialViewService.GetSnippetAsync(id);
        return snippet is not null
            ? Ok(_umbracoMapper.Map<PartialViewSnippetResponseModel>(snippet)!)
            : PartialViewNotFound();
    }
}

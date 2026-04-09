using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.LogViewer;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer.SavedSearch;

/// <summary>
/// Controller for retrieving saved log viewer searches by name.
/// </summary>
[ApiVersion("1.0")]
public class ByNameSavedSearchLogViewerController : SavedSearchLogViewerControllerBase
{
    private readonly ILogViewerService _logViewerService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.LogViewer.SavedSearch.ByNameSavedSearchLogViewerController"/> class,
    /// which handles log viewer operations for saved searches by name.
    /// </summary>
    /// <param name="logViewerService">The service used to interact with log viewer functionality.</param>
    /// <param name="umbracoMapper">The mapper used to map Umbraco domain models to API models.</param>
    public ByNameSavedSearchLogViewerController(ILogViewerService logViewerService, IUmbracoMapper umbracoMapper)
    {
        _logViewerService = logViewerService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a saved log search by name.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="name">The name of the saved log search.</param>
    /// <returns>The saved log search or not found result.</returns>
    [HttpGet("{name}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(SavedLogSearchResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a saved log search by name.")]
    [EndpointDescription("Gets a saved log search identified by the provided name.")]
    public async Task<IActionResult> ByName(
        CancellationToken cancellationToken,
        string name)
    {
        ILogViewerQuery? savedLogQuery = await _logViewerService.GetSavedLogQueryByNameAsync(name);

        if (savedLogQuery is null)
        {
            return SavedSearchNotFound();
        }

        return Ok(_umbracoMapper.Map<SavedLogSearchResponseModel>(savedLogQuery));
    }
}

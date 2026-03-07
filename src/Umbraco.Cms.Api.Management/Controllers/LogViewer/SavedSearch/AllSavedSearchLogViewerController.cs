using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.LogViewer;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer.SavedSearch;

    /// <summary>
    /// API controller for managing and retrieving all saved searches in the log viewer.
    /// </summary>
[ApiVersion("1.0")]
public class AllSavedSearchLogViewerController : SavedSearchLogViewerControllerBase
{
    private readonly ILogViewerService _logViewerService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllSavedSearchLogViewerController"/> class, which manages operations related to all saved log viewer searches.
    /// </summary>
    /// <param name="logViewerService">The service used to interact with log viewer functionality.</param>
    /// <param name="umbracoMapper">The mapper used to convert between Umbraco models and API models.</param>
    public AllSavedSearchLogViewerController(ILogViewerService logViewerService, IUmbracoMapper umbracoMapper)
    {
        _logViewerService = logViewerService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a paginated list of all saved log searches.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <returns>The paged result of the saved log searches.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<SavedLogSearchResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of saved log searches.")]
    [EndpointDescription("Gets a collection of saved log searches defined in the Umbraco installation.")]
    public async Task<ActionResult<PagedViewModel<SavedLogSearchResponseModel>>> AllSavedSearches(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
    {
        PagedModel<ILogViewerQuery> savedLogQueries = await _logViewerService.GetSavedLogQueriesAsync(skip, take);

        var viewModel = new PagedViewModel<SavedLogSearchResponseModel>
        {
            Total = savedLogQueries.Total,
            Items = _umbracoMapper.MapEnumerable<ILogViewerQuery, SavedLogSearchResponseModel>(savedLogQueries.Items)
        };

        return Ok(viewModel);
    }
}

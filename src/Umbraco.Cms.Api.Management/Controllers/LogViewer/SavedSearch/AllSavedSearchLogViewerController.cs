using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.LogViewer;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer.SavedSearch;

public class AllSavedSearchLogViewerController : SavedSearchLogViewerControllerBase
{
    private readonly ILogViewerService _logViewerService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllSavedSearchLogViewerController(ILogViewerService logViewerService, IUmbracoMapper umbracoMapper)
    {
        _logViewerService = logViewerService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a paginated list of all saved log searches.
    /// </summary>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <returns>The paged result of the saved log searches.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<SavedLogSearchViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<SavedLogSearchViewModel>>> AllSavedSearches(int skip = 0, int take = 100)
    {
        IReadOnlyList<ILogViewerQuery> savedLogQueries = await _logViewerService.GetSavedLogQueriesAsync();

        return Ok(_umbracoMapper.Map<PagedViewModel<SavedLogSearchViewModel>>(savedLogQueries.Skip(skip).Take(take)));
    }
}

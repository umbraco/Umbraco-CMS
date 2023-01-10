using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.LogViewer;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer.SavedSearch;

public class AllSavedSearchLogViewerController : SavedSearchLogViewerControllerBase
{
    private readonly ILogViewer _logViewer;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllSavedSearchLogViewerController(ILogViewer logViewer, IUmbracoMapper umbracoMapper)
        : base(logViewer)
    {
        _logViewer = logViewer;
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
        IEnumerable<SavedLogSearch> savedSearches = _logViewer
            .GetSavedSearches()
            .Skip(skip)
            .Take(take);

        return await Task.FromResult(Ok(_umbracoMapper.Map<PagedViewModel<SavedLogSearchViewModel>>(savedSearches)));
    }
}

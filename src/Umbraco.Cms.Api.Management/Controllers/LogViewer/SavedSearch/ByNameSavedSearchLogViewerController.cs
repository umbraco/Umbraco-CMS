using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.LogViewer;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer.SavedSearch;

public class ByNameSavedSearchLogViewerController : SavedSearchLogViewerControllerBase
{
    private readonly ILogViewerService _logViewerService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByNameSavedSearchLogViewerController(ILogViewerService logViewerService, IUmbracoMapper umbracoMapper)
    {
        _logViewerService = logViewerService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a saved log search by name.
    /// </summary>
    /// <param name="name">The name of the saved log search.</param>
    /// <returns>The saved log search or not found result.</returns>
    [HttpGet("{name}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(SavedLogSearchViewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<SavedLogSearchViewModel>> ByName(string name)
    {
        ILogViewerQuery? savedLogQuery = await _logViewerService.GetSavedLogQueryByNameAsync(name);

        if (savedLogQuery is null)
        {
            return NotFound();
        }

        return Ok(_umbracoMapper.Map<SavedLogSearchViewModel>(savedLogQuery));
    }
}

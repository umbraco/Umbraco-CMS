using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Log;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Controllers.Log.SavedQuery;

public class ByNameSavedQueryLogController : SavedQueryLogControllerBase
{
    private readonly ILogViewer _logViewer;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByNameSavedQueryLogController(ILogViewer logViewer, IUmbracoMapper umbracoMapper)
        : base(logViewer)
    {
        _logViewer = logViewer;
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
        SavedLogSearch? savedSearch = _logViewer.GetSavedSearchByName(name);

        if (savedSearch is null)
        {
            return await Task.FromResult(NotFound());
        }

        return await Task.FromResult(Ok(_umbracoMapper.Map<SavedLogSearchViewModel>(savedSearch)));
    }
}

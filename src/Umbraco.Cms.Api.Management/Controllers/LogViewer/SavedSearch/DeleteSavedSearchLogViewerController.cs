using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Logging.Viewer;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer.SavedSearch;

public class DeleteSavedSearchLogViewerController : SavedSearchLogViewerControllerBase
{
    private readonly ILogViewer _logViewer;

    public DeleteSavedSearchLogViewerController(ILogViewer logViewer)
        : base(logViewer) => _logViewer = logViewer;

    /// <summary>
    ///     Deletes a saved log search with a given name.
    /// </summary>
    /// <param name="name">The name of the saved log search.</param>
    /// <returns>The result of the deletion.</returns>
    [HttpDelete("{name}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(string name)
    {
        SavedLogSearch? savedSearch = _logViewer.GetSavedSearchByName(name);

        if (savedSearch is null)
        {
            return await Task.FromResult(NotFound());
        }

        _logViewer.DeleteSavedSearch(name);

        return await Task.FromResult(Ok());
    }
}

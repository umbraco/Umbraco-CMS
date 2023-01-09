using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Logging.Viewer;

namespace Umbraco.Cms.Api.Management.Controllers.Log.SavedQuery;

public class DeleteSavedQueryLogController : SavedQueryLogControllerBase
{
    private readonly ILogViewer _logViewer;

    public DeleteSavedQueryLogController(ILogViewer logViewer)
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
        // We need to get the count of all saved searches before trying to delete,
        // so we can compare the results since DeleteSavedSearch() does not
        // indicate whether the deletion was successful.
        int countBeforeDeletion = _logViewer.GetSavedSearches().Count;

        IReadOnlyList<SavedLogSearch> modifiedSearches = _logViewer.DeleteSavedSearch(name, null);

        // A saved query with that name does not exist
        if (countBeforeDeletion == modifiedSearches.Count)
        {
            return await Task.FromResult(NotFound());
        }

        return await Task.FromResult(Ok());
    }
}

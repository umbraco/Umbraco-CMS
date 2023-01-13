using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer.SavedSearch;

public class DeleteSavedSearchLogViewerController : SavedSearchLogViewerControllerBase
{
    private readonly ILogViewerService _logViewerService;

    public DeleteSavedSearchLogViewerController(ILogViewerService logViewerService) => _logViewerService = logViewerService;

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
        bool isSuccessful = await _logViewerService.DeleteSavedLogQueryAsync(name);

        if (isSuccessful == false)
        {
            return await Task.FromResult(NotFound());
        }

        return await Task.FromResult(Ok());
    }
}

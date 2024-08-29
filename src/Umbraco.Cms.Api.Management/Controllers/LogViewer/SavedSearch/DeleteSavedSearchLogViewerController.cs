using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer.SavedSearch;

[ApiVersion("1.0")]
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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, string name)
    {
        Attempt<ILogViewerQuery?, LogViewerOperationStatus> result = await _logViewerService.DeleteSavedLogQueryAsync(name);

        if (result.Success)
        {
            return Ok();
        }

        return LogViewerOperationStatusResult(result.Status);
    }
}

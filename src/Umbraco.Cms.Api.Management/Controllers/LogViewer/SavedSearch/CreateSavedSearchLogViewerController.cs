using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.LogViewer;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer.SavedSearch;

[ApiVersion("1.0")]
public class CreateSavedSearchLogViewerController : SavedSearchLogViewerControllerBase
{
    private readonly ILogViewerService _logViewerService;

    public CreateSavedSearchLogViewerController(ILogViewerService logViewerService) => _logViewerService = logViewerService;

    /// <summary>
    ///     Creates a saved log search.
    /// </summary>
    /// <param name="savedSearch">The log search to be saved.</param>
    /// <returns>The location of the saved log search after the creation.</returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken, SavedLogSearchRequestModel savedSearch)
    {
        Attempt<ILogViewerQuery?, LogViewerOperationStatus> result =
            await _logViewerService.AddSavedLogQueryAsync(savedSearch.Name, savedSearch.Query);

        if (result.Success)
        {
            return CreatedAtAction<ByNameSavedSearchLogViewerController>(
                    controller => nameof(controller.ByName), new { name = savedSearch.Name }, savedSearch.Name);
        }

        return LogViewerOperationStatusResult(result.Status);
    }
}

using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.LogViewer;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer.SavedSearch;

/// <summary>
/// Controller responsible for handling the creation of saved search entries in the log viewer.
/// </summary>
[ApiVersion("1.0")]
public class CreateSavedSearchLogViewerController : SavedSearchLogViewerControllerBase
{
    private readonly ILogViewerService _logViewerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateSavedSearchLogViewerController"/> class with the specified log viewer service.
    /// </summary>
    /// <param name="logViewerService">An instance of <see cref="ILogViewerService"/> used to manage log viewing operations.</param>
    public CreateSavedSearchLogViewerController(ILogViewerService logViewerService) => _logViewerService = logViewerService;

    /// <summary>
    ///     Creates a saved log search.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="savedSearch">The log search to be saved.</param>
    /// <returns>The location of the saved log search after the creation.</returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [EndpointSummary("Creates a saved log search.")]
    [EndpointDescription("Creates a new saved log search with the provided name and query configuration.")]
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

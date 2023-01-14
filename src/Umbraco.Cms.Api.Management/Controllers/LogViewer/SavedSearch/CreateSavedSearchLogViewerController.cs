using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.LogViewer;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer.SavedSearch;

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
    public async Task<IActionResult> Create(SavedLogSearchViewModel savedSearch)
    {
        bool isSuccessful = await _logViewerService.AddSavedLogQueryAsync(savedSearch.Name, savedSearch.Query);

        if (isSuccessful == false)
        {
            var invalidModelProblem = new ProblemDetails
            {
                Title = "Duplicate log search name",
                Detail = $"""Log search with name "{savedSearch.Name}" already exists""",
                Status = StatusCodes.Status400BadRequest,
                Type = "Error",
            };

            return await Task.FromResult(BadRequest(invalidModelProblem));
        }

        // FIXME: (elit0451) Make use of the extension method of CreatedAtAction
        return await Task.FromResult(Created($"management/api/v1/log-viewer/saved-search/{savedSearch.Name}", null));
    }
}

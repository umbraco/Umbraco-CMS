using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Log;
using Umbraco.Cms.Core.Logging.Viewer;

namespace Umbraco.Cms.Api.Management.Controllers.Log.SavedQuery;

public class CreateSavedQueryLogController : SavedQueryLogControllerBase
{
    private readonly ILogViewer _logViewer;

    public CreateSavedQueryLogController(ILogViewer logViewer)
        : base(logViewer) => _logViewer = logViewer;

    /// <summary>
    ///     Creates a saved log search.
    /// </summary>
    /// <param name="savedSearch">The query to be saved.</param>
    /// <returns>The result of the creation.</returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(SavedLogSearchViewModel savedSearch)
    {
        try
        {
            _logViewer.AddSavedSearch(savedSearch.Name, savedSearch.Query);
        }
        catch (DuplicateNameException ex)
        {
            var invalidModelProblem = new ProblemDetails
            {
                Title = "Duplicate query name",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Type = "Error",
            };

            return await Task.FromResult(BadRequest(invalidModelProblem));
        }

        return await Task.FromResult(Ok());
    }
}

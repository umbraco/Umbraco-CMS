using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Search;
using Umbraco.Search.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Indexer;

[ApiVersion("1.0")]
public class RebuildIndexerController : IndexerControllerBase
{
    private readonly ILogger<RebuildIndexerController> _logger;
    private readonly IIndexingRebuilderService _indexingRebuilderService;

    public RebuildIndexerController(
        ILogger<RebuildIndexerController> logger,
        IIndexingRebuilderService indexingRebuilderService)
    {
        _logger = logger;
        _indexingRebuilderService = indexingRebuilderService;
    }

    /// <summary>
    ///     Rebuilds the index
    /// </summary>
    /// <param name="indexName"></param>
    /// <returns></returns>
    [HttpPost("{indexName}/rebuild")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Rebuild(string indexName)
    {

        if (!_indexingRebuilderService.CanRebuild(indexName))
        {
            var invalidModelProblem = new ProblemDetails
            {
                Title = "Could not validate the populator",
                Detail =
                    $"The index {indexName} could not be rebuilt because we could not validate its associated {typeof(IIndexPopulator)}",
                Status = StatusCodes.Status400BadRequest,
                Type = "Error",
            };

            return await Task.FromResult(BadRequest(invalidModelProblem));
        }

        _logger.LogInformation("Rebuilding index '{IndexName}'", indexName);

        if (_indexingRebuilderService.TryRebuild(indexName))
        {
            return await Task.FromResult(Ok());
        }

        var problemDetails = new ProblemDetails
        {
            Title = "Index could not be rebuilt",
            Detail = $"The index {indexName} could not be rebuild. Check the log for details on this error.",
            Status = StatusCodes.Status400BadRequest,
            Type = "Error",
        };

        return await Task.FromResult(Conflict(problemDetails));
    }
}

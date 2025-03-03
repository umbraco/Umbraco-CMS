using Asp.Versioning;
using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Indexer;

[ApiVersion("1.0")]
public class RebuildIndexerController : IndexerControllerBase
{
    private readonly ILogger<RebuildIndexerController> _logger;
    private readonly IIndexingRebuilderService _indexingRebuilderService;
    private readonly IExamineManager _examineManager;

    public RebuildIndexerController(
        ILogger<RebuildIndexerController> logger,
        IIndexingRebuilderService indexingRebuilderService,
        IExamineManager examineManager)
    {
        _logger = logger;
        _indexingRebuilderService = indexingRebuilderService;
        _examineManager = examineManager;
    }

    /// <summary>
    ///     Rebuilds the index
    /// </summary>
    /// <param name="indexName"></param>
    /// <returns></returns>
    [HttpPost("{indexName}/rebuild")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Rebuild(CancellationToken cancellationToken, string indexName)
    {
        if (!_examineManager.TryGetIndex(indexName, out IIndex? index))
        {
            var invalidModelProblem = new ProblemDetails
            {
                Title = "Index Not Found",
                Detail = $"No index found with name = {indexName}",
                Status = StatusCodes.Status400BadRequest,
                Type = "Error",
            };

            return await Task.FromResult(NotFound(invalidModelProblem));
        }

        if (!_indexingRebuilderService.CanRebuild(index.Name))
        {
            var invalidModelProblem = new ProblemDetails
            {
                Title = "Could not validate the populator",
                Detail =
                    $"The index {index?.Name} could not be rebuilt because we could not validate its associated {typeof(IIndexPopulator)}",
                Status = StatusCodes.Status400BadRequest,
                Type = "Error",
            };

            return await Task.FromResult(BadRequest(invalidModelProblem));
        }

        _logger.LogInformation("Rebuilding index '{IndexName}'", indexName);

        if (_indexingRebuilderService.TryRebuild(index, indexName))
        {
            return await Task.FromResult(Ok());
        }

        var problemDetails = new ProblemDetails
        {
            Title = "Index could not be rebuilt",
            Detail = $"The index {index.Name} could not be rebuild. Check the log for details on this error.",
            Status = StatusCodes.Status400BadRequest,
            Type = "Error",
        };

        return await Task.FromResult(Conflict(problemDetails));
    }
}

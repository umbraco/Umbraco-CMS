using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.ManagementApi.Services;
using Umbraco.New.Cms.Infrastructure.Services;

namespace Umbraco.Cms.ManagementApi.Controllers.ExamineManagement;

[ApiVersion("1.0")]
public class RebuildExamineManagementController : ExamineManagementControllerBase
{
    private readonly IExamineManagerService _examineManagerService;
    private readonly ILogger<RebuildExamineManagementController> _logger;
    private readonly IIndexRebuilder _indexRebuilder;
    private readonly ITemporaryIndexingService _temporaryIndexingService;

    public RebuildExamineManagementController(
        IExamineManagerService examineManagerService,
        ILogger<RebuildExamineManagementController> logger,
        IIndexRebuilder indexRebuilder,
        ITemporaryIndexingService temporaryIndexingService)
    {
        _examineManagerService = examineManagerService;
        _logger = logger;
        _indexRebuilder = indexRebuilder;
        _temporaryIndexingService = temporaryIndexingService;
    }

    /// <summary>
    ///     Rebuilds the index
    /// </summary>
    /// <param name="indexName"></param>
    /// <returns></returns>
    [HttpPost("rebuild")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Rebuild(string indexName)
    {
        if (!_examineManagerService.ValidateIndex(indexName, out IIndex? index))
        {
            var invalidModelProblem = new ProblemDetails
            {
                Title = "Index Not Found",
                Detail = $"No index found with name = {indexName}",
                Status = StatusCodes.Status400BadRequest,
                Type = "Error",
            };

            return await Task.FromResult(BadRequest(invalidModelProblem));
        }

        if (!_examineManagerService.ValidatePopulator(index!))
        {
            var invalidModelProblem = new ProblemDetails
            {
                Title = "Could not validate the populator",
                Detail = $"The index {index?.Name} cannot be rebuilt because we could not validate its associated {typeof(IIndexPopulator)}",
                Status = StatusCodes.Status400BadRequest,
                Type = "Error",
            };

            return await Task.FromResult(BadRequest(invalidModelProblem));
        }

        _logger.LogInformation("Rebuilding index '{IndexName}'", indexName);

        // remove it in case there's a handler there already
        index!.IndexOperationComplete -= Indexer_IndexOperationComplete;

        //now add a single handler
        index.IndexOperationComplete += Indexer_IndexOperationComplete;

        try
        {
            _temporaryIndexingService.Set(indexName);
            _indexRebuilder.RebuildIndex(indexName);

            return await Task.FromResult(Ok());
        }
        catch (Exception ex)
        {
            //ensure it's not listening
            index.IndexOperationComplete -= Indexer_IndexOperationComplete;
            _logger.LogError(ex, "An error occurred rebuilding index");
            var invalidModelProblem = new ProblemDetails
            {
                Title = "Index cannot be rebuilt",
                Detail = $"The index {index.Name} cannot be rebuilt because it does not have an associated {typeof(IIndexPopulator)}",
                Status = StatusCodes.Status400BadRequest,
                Type = "Error",
            };

            return await Task.FromResult(Conflict(invalidModelProblem));
        }
    }

    private void Indexer_IndexOperationComplete(object? sender, EventArgs e)
    {
        var indexer = (IIndex?)sender;

        _logger.LogDebug("Logging operation completed for index {IndexName}", indexer?.Name);

        if (indexer is not null)
        {
            //ensure it's not listening anymore
            indexer.IndexOperationComplete -= Indexer_IndexOperationComplete;
        }

        _logger.LogInformation($"Rebuilding index '{indexer?.Name}' done.");

        _temporaryIndexingService.Clear(indexer?.Name);
    }
}

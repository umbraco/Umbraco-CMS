using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.ManagementApi.Services;

namespace Umbraco.Cms.ManagementApi.Controllers.ExamineManagement;

[ApiVersion("1.0")]
public class RebuildIndexExamineManagementController : ExamineManagementControllerBase
{
    private readonly IExamineManagerService _examineManagerService;
    private readonly ILogger<RebuildIndexExamineManagementController> _logger;
    private readonly IIndexRebuilder _indexRebuilder;
    private readonly IAppPolicyCache _runtimeCache;

    public RebuildIndexExamineManagementController(
        IExamineManagerService examineManagerService,
        ILogger<RebuildIndexExamineManagementController> logger,
        AppCaches runtimeCache,
        IIndexRebuilder indexRebuilder)
    {
        _examineManagerService = examineManagerService;
        _logger = logger;
        _indexRebuilder = indexRebuilder;
        _runtimeCache = runtimeCache.RuntimeCache;
    }

    /// <summary>
    ///     Rebuilds the index
    /// </summary>
    /// <param name="indexName"></param>
    /// <returns></returns>
    [HttpPost("rebuildIndex")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> RebuildIndex(string indexName)
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

            return BadRequest(invalidModelProblem);
        }

        if (!_examineManagerService.ValidatePopulator(index!))
        {
            var invalidModelProblem = new ProblemDetails
            {
                Title = "Index cannot be rebuilt",
                Detail = $"The index {index?.Name} cannot be rebuilt because it does not have an associated {typeof(IIndexPopulator)}",
                Status = StatusCodes.Status400BadRequest,
                Type = "Error",
            };

            return BadRequest(invalidModelProblem);
        }

        _logger.LogInformation("Rebuilding index '{IndexName}'", indexName);

        // remove it in case there's a handler there already
        index!.IndexOperationComplete -= Indexer_IndexOperationComplete;

        //now add a single handler
        index.IndexOperationComplete += Indexer_IndexOperationComplete;

        try
        {
            var cacheKey = "temp_indexing_op_" + index.Name;
            //put temp val in cache which is used as a rudimentary way to know when the indexing is done
            _runtimeCache.Insert(cacheKey, () => "tempValue", TimeSpan.FromMinutes(5));

            _indexRebuilder.RebuildIndex(indexName);

            return Ok();
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

            return Conflict(invalidModelProblem);
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

        var cacheKey = "temp_indexing_op_" + indexer?.Name;
        _runtimeCache.Clear(cacheKey);
    }
}

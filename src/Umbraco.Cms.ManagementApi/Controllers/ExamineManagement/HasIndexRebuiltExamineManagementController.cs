using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.ManagementApi.Factories;
using Umbraco.Cms.ManagementApi.Services;
using Umbraco.Cms.ManagementApi.ViewModels.ExamineManagement;

namespace Umbraco.Cms.ManagementApi.Controllers.ExamineManagement;

[ApiVersion("1.0")]
public class HasIndexRebuiltExamineManagementController : ExamineManagementControllerBase
{
    private readonly IAppPolicyCache _runtimeCache;
    private readonly IExamineIndexViewModelFactory _examineIndexViewModelFactory;
    private readonly IExamineManagerService _examineManagerService;

    public HasIndexRebuiltExamineManagementController(
        AppCaches runtimeCache,
        IExamineIndexViewModelFactory examineIndexViewModelFactory,
        IExamineManagerService examineManagerService)
    {
        _runtimeCache = runtimeCache.RuntimeCache;
        _examineIndexViewModelFactory = examineIndexViewModelFactory;
        _examineManagerService = examineManagerService;
    }

    /// <summary>
    ///     Check if the index has been rebuilt
    /// </summary>
    /// <param name="indexName"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This is kind of rudimentary since there's no way we can know that the index has rebuilt, we
    ///     have a listener for the index op complete so we'll just check if that key is no longer there in the runtime cache
    /// </remarks>
    [HttpGet("hasIndexRebuilt")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ExamineIndexViewModel), StatusCodes.Status200OK)]
    // This endpoint for now will throw errors if the ExamineIndexViewModel ever has providerProperties defined
    // This is because System.Text.Json cannot serialize dictionary<string, object>
    // This has been fixed in .NET 7, so this will work when we upgrade: https://github.com/dotnet/runtime/issues/67588
    public async Task<ActionResult<ExamineIndexViewModel?>> HasIndexRebuilt(string indexName)
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

        var cacheKey = "temp_indexing_op_" + indexName;
        var found = _runtimeCache.Get(cacheKey);

        // if its still there then it's not done
        return found != null
            ? null
            : _examineIndexViewModelFactory.Create(index!);
    }
}

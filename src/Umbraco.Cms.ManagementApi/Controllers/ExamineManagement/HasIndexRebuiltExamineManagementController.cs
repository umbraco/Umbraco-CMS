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
    private readonly IExamineIndexViewModelFactory _examineIndexViewModelFactory;
    private readonly IExamineManagerService _examineManagerService;

    public HasIndexRebuiltExamineManagementController(
        IExamineIndexViewModelFactory examineIndexViewModelFactory,
        IExamineManagerService examineManagerService)
    {
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
    [HttpGet("index")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ExamineIndexViewModel), StatusCodes.Status200OK)]
    // This endpoint for now will throw errors if the ExamineIndexViewModel ever has providerProperties defined
    // This is because System.Text.Json cannot serialize dictionary<string, object>
    // This has been fixed in .NET 7, so this will work when we upgrade: https://github.com/dotnet/runtime/issues/67588
    public async Task<ActionResult<ExamineIndexViewModel?>> Index(string indexName)
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

        return await Task.FromResult(_examineIndexViewModelFactory.Create(index!));
    }
}

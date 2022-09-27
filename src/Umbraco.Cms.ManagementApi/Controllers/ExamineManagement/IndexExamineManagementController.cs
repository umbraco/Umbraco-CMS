using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.ManagementApi.Factories;
using Umbraco.Cms.ManagementApi.ViewModels.ExamineManagement;

namespace Umbraco.Cms.ManagementApi.Controllers.ExamineManagement;

[ApiVersion("1.0")]
public class IndexExamineManagementController : ExamineManagementControllerBase
{
    private readonly IExamineIndexViewModelFactory _examineIndexViewModelFactory;
    private readonly IExamineManager _examineManager;

    public IndexExamineManagementController(
        IExamineIndexViewModelFactory examineIndexViewModelFactory,
        IExamineManager examineManager)
    {
        _examineIndexViewModelFactory = examineIndexViewModelFactory;
        _examineManager = examineManager;
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
    public async Task<ActionResult<ExamineIndexViewModel?>> Index(string indexName)
    {
        if (_examineManager.TryGetIndex(indexName, out IIndex? index))
        {
            return await Task.FromResult(_examineIndexViewModelFactory.Create(index!));
        }

        var invalidModelProblem = new ProblemDetails
        {
            Title = "Index Not Found",
            Detail = $"No index found with name = {indexName}",
            Status = StatusCodes.Status400BadRequest,
            Type = "Error",
        };

        return await Task.FromResult(BadRequest(invalidModelProblem));

    }
}

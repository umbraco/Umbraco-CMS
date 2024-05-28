using Asp.Versioning;
using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Indexer;

namespace Umbraco.Cms.Api.Management.Controllers.Indexer;

[ApiVersion("1.0")]
public class DetailsIndexerController : IndexerControllerBase
{
    private readonly IIndexPresentationFactory _indexPresentationFactory;
    private readonly IExamineManager _examineManager;

    public DetailsIndexerController(
        IIndexPresentationFactory indexPresentationFactory,
        IExamineManager examineManager)
    {
        _indexPresentationFactory = indexPresentationFactory;
        _examineManager = examineManager;
    }

    /// <summary>
    ///     Check if the index has been rebuilt
    /// </summary>
    /// <param name="indexName"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This is kind of rudimentary since there's no way we can know that the index has rebuilt, we
    ///     have a listener for the index op complete so we'll just check if that id is no longer there in the runtime cache
    /// </remarks>
    [HttpGet("{indexName}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IndexResponseModel), StatusCodes.Status200OK)]
    public Task<ActionResult<IndexResponseModel?>> Details(CancellationToken cancellationToken, string indexName)
    {
        if (_examineManager.TryGetIndex(indexName, out IIndex? index))
        {
            return Task.FromResult<ActionResult<IndexResponseModel?>>(_indexPresentationFactory.Create(index));
        }

        var invalidModelProblem = new ProblemDetails
        {
            Title = "Index Not Found",
            Detail = $"No index found with name = {indexName}",
            Status = StatusCodes.Status400BadRequest,
            Type = "Error",
        };

        return Task.FromResult<ActionResult<IndexResponseModel?>>(NotFound(invalidModelProblem));
    }
}

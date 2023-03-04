using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.ManagementApi.Factories;
using Umbraco.Cms.ManagementApi.ViewModels.Search;

namespace Umbraco.Cms.ManagementApi.Controllers.Indexer;

[ApiVersion("1.0")]
public class DetailsIndexerController : IndexerControllerBase
{
    private readonly IIndexViewModelFactory _indexViewModelFactory;
    public DetailsIndexerController(
        IIndexViewModelFactory indexViewModelFactory)
    {
        _indexViewModelFactory = indexViewModelFactory;
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
    [HttpGet("{indexName}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IndexViewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<IndexViewModel?>> Details(string indexName)
    {

            return await Task.FromResult(_indexViewModelFactory.Create(indexName!));
    }
}

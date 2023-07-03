﻿using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Indexer;

namespace Umbraco.Cms.Api.Management.Controllers.Indexer;

[ApiVersion("1.0")]
public class DetailsIndexerController : IndexerControllerBase
{
    private readonly IIndexPresentationFactory _indexViewModelFactory;
    public DetailsIndexerController(
        IIndexPresentationFactory indexViewModelFactory)
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
    ///     have a listener for the index op complete so we'll just check if that id is no longer there in the runtime cache
    /// </remarks>
    [HttpGet("{indexName}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IndexResponseModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<IndexResponseModel>> Details(string indexName)
    {
        try
        {
            var result = await Task.FromResult(_indexViewModelFactory.Create(indexName!));
            return Ok(result);
        }catch(Exception ex)
        {
            var invalidModelProblem = new ProblemDetails
            {
                Title = ex.Message,
                Detail = $"Failed Creating information model for index = {indexName}",
                Status = StatusCodes.Status400BadRequest,
                Type = "Error",
            };

            return await Task.FromResult(BadRequest(invalidModelProblem));

        }

    }
}

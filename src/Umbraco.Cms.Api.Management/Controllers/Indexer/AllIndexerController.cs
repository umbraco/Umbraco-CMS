﻿using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Indexer;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Indexer;

[ApiVersion("1.0")]
public class AllIndexerController : IndexerControllerBase
{
    private readonly IExamineManager _examineManager;
    private readonly IIndexViewModelFactory _indexViewModelFactory;

    public AllIndexerController(
        IExamineManager examineManager,
        IIndexViewModelFactory indexViewModelFactory)
    {
        _examineManager = examineManager;
        _indexViewModelFactory = indexViewModelFactory;
    }

    /// <summary>
    ///     Get the details for indexers
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<IndexResponseModel>), StatusCodes.Status200OK)]
    public Task<PagedViewModel<IndexResponseModel>> All(int skip, int take)
    {
        IndexResponseModel[] indexes = _examineManager.Indexes
            .Select(_indexViewModelFactory.Create)
            .OrderBy(indexModel => indexModel.Name.TrimEnd("Indexer")).ToArray();

        var viewModel = new PagedViewModel<IndexResponseModel> { Items = indexes.Skip(skip).Take(take), Total = indexes.Length };
        return Task.FromResult(viewModel);
    }
}

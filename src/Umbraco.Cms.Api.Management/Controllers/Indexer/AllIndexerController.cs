using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Extensions;
using Umbraco.Search;
using Umbraco.Search.Models;

namespace Umbraco.Cms.Api.Management.Controllers.Indexer;

[ApiVersion("1.0")]
public class AllIndexerController : IndexerControllerBase
{
    private readonly ISearchProvider _searchProvider;
    private readonly IIndexPresentationFactory _indexPresentationFactory;

    public AllIndexerController(
        ISearchProvider searchProvider,
        IIndexPresentationFactory indexPresentationFactory)
    {
        _searchProvider = searchProvider;
        _indexPresentationFactory = indexPresentationFactory;
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
        IndexResponseModel[] indexes = _searchProvider.GetAllIndexes()
            .Select(x=>_indexPresentationFactory.Create(x)) .OrderBy(indexModel => indexModel.Name.TrimEnd("Indexer")).ToArray();

        var viewModel = new PagedViewModel<IndexResponseModel> { Items = indexes.Skip(skip).Take(take), Total = indexes.Length };
        return Task.FromResult(viewModel);
    }
}

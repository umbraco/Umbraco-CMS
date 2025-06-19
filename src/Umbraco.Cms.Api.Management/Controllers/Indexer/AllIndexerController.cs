using Asp.Versioning;
using Examine;
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
    private readonly IIndexPresentationFactory _indexPresentationFactory;

    public AllIndexerController(
        IExamineManager examineManager,
        IIndexPresentationFactory indexPresentationFactory)
    {
        _examineManager = examineManager;
        _indexPresentationFactory = indexPresentationFactory;
    }

    /// <summary>
    ///     Get the details for indexers
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<IndexResponseModel>), StatusCodes.Status200OK)]
    public Task<PagedViewModel<IndexResponseModel>> All(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
    {
        IndexResponseModel[] indexes = _examineManager.Indexes
            .Select(_indexPresentationFactory.Create)
            .OrderBy(indexModel => indexModel.Name.TrimEnd("Indexer")).ToArray();

        var viewModel = new PagedViewModel<IndexResponseModel> { Items = indexes.Skip(skip).Take(take), Total = indexes.Length };
        return Task.FromResult(viewModel);
    }
}

using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.ManagementApi.Factories;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.Search;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.Search;

[ApiVersion("1.0")]
public class IndexListSearchController : SearchControllerBase
{
    private readonly IExamineManager _examineManager;
    private readonly IIndexViewModelFactory _indexViewModelFactory;

    public IndexListSearchController(
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
    [HttpGet("index")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<IndexViewModel>), StatusCodes.Status200OK)]
    public async Task<PagedViewModel<IndexViewModel>> Indexes(int skip, int take)
    {
        IndexViewModel[] indexes = _examineManager.Indexes
            .Select(_indexViewModelFactory.Create)
            .OrderBy(indexModel => indexModel.Name.TrimEnd("Indexer")).ToArray();

        var viewModel = new PagedViewModel<IndexViewModel> { Items = indexes.Skip(skip).Take(take), Total = indexes.Length };
        return viewModel;
    }
}

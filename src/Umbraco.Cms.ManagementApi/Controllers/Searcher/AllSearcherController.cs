using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.Search;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.Searcher;

[ApiVersion("1.0")]
public class AllSearcherController : SearcherControllerBase
{
    private readonly IExamineManager _examineManager;

    public AllSearcherController(IExamineManager examineManager) => _examineManager = examineManager;

    /// <summary>
    ///     Get the details for searchers
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<SearcherViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<SearcherViewModel>>> All(int skip, int take)
    {
        var searchers = new List<SearcherViewModel>(
            _examineManager.RegisteredSearchers.Select(searcher => new SearcherViewModel { Name = searcher.Name })
                .OrderBy(x =>
                    x.Name.TrimEnd("Searcher"))); // order by name , but strip the "Searcher" from the end if it exists
        var viewModel = new PagedViewModel<SearcherViewModel>
        {
            Items = searchers.Skip(skip).Take(take),
            Total = searchers.Count,
        };

        return await Task.FromResult(Ok(viewModel));
    }
}

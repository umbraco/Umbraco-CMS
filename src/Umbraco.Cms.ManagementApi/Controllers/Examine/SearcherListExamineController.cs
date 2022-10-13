using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.ManagementApi.ViewModels.ExamineManagement;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.Examine;

[ApiVersion("1.0")]
public class SearcherListExamineController : ExamineControllerBase
{
    private readonly IExamineManager _examineManager;

    public SearcherListExamineController(IExamineManager examineManager) => _examineManager = examineManager;

    /// <summary>
    ///     Get the details for searchers
    /// </summary>
    /// <returns></returns>
    [HttpGet("searcher")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<SearcherViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<SearcherViewModel>>> Searchers(int skip, int take)
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

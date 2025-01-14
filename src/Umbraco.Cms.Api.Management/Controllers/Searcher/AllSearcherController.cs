using Asp.Versioning;
using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Searcher;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Searcher;

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
    [ProducesResponseType(typeof(PagedViewModel<SearcherResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<SearcherResponse>>> All(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
    {
        var searchers = new List<SearcherResponse>(
            _examineManager.RegisteredSearchers.Select(searcher => new SearcherResponse { Name = searcher.Name })
                .OrderBy(x =>
                    x.Name.TrimEnd("Searcher"))); // order by name , but strip the "Searcher" from the end if it exists
        var viewModel = new PagedViewModel<SearcherResponse>
        {
            Items = searchers.Skip(skip).Take(take),
            Total = searchers.Count,
        };

        return await Task.FromResult(Ok(viewModel));
    }
}

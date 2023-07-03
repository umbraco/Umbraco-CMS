using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Searcher;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Extensions;
using Umbraco.Search;

namespace Umbraco.Cms.Api.Management.Controllers.Searcher;

[ApiVersion("1.0")]
public class AllSearcherController : SearcherControllerBase
{
    private readonly ISearchProvider _searchProvider;

    public AllSearcherController(ISearchProvider searchProvider) => _searchProvider = searchProvider;

    /// <summary>
    ///     Get the details for searchers
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<SearcherResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<SearcherResponse>>> All(int skip, int take)
    {
        var searchers = new List<SearcherResponse>(
            _searchProvider.GetAllSearchers().Select(searcher => new SearcherResponse { Name = searcher })
                .OrderBy(x =>
                    (x.Name ?? string.Empty).TrimEnd("Searcher"))); // order by name , but strip the "Searcher" from the end if it exists
        var viewModel = new PagedViewModel<SearcherResponse>
        {
            Items = searchers.Skip(skip).Take(take),
            Total = searchers.Count,
        };

        return await Task.FromResult(Ok(viewModel));
    }
}

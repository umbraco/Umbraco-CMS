
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Cms.ManagementApi.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.Search;
using Umbraco.Extensions;
using Umbraco.Search;

namespace Umbraco.Cms.Api.Management.Controllers.Searcher;

[ApiVersion("1.0")]
public class QuerySearcherController : SearcherControllerBase
{
    private readonly ISearchProvider _examineManagerService;

    public QuerySearcherController(ISearchProvider examineManagerService) => _examineManagerService = examineManagerService;

    [Microsoft.AspNetCore.Mvc.HttpGet("{searcherName}/query")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<SearchResultResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedViewModel<SearchResultResponseModel>>> Query(string searcherName, string? term, int skip, int take)
    {
        term = term?.Trim();

        if (term.IsNullOrWhiteSpace())
        {
            return new PagedViewModel<SearchResultResponseModel>();
        }

        IUmbracoSearcher? searcher = _examineManagerService.GetSearcher(searcherName);
        if (searcher == null)
        {
            var invalidModelProblem = new ProblemDetails
            {
                Title = "Could not find a valid searcher",
                Detail = "The provided searcher name did not match any of our registered searchers",
                Status = StatusCodes.Status404NotFound,
                Type = "Error",
            };

            return NotFound(invalidModelProblem);
        }

        UmbracoSearchResults? results;

        // NativeQuery will work for a single word/phrase too (but depends on the implementation) the lucene one will work.
        try
        {
            results = searcher
                .NativeQuery(term, skip/take, take);
        }
        catch (Exception)
        {
            var invalidModelProblem = new ProblemDetails
            {
                Title = "Could not parse the query",
                Detail = "Search provider could not parse the query. Please double check if the query is valid. Sometimes this can also happen if your query starts with a wildcard (*)",
                Status = StatusCodes.Status404NotFound,
                Type = "Error",
            };

            return BadRequest(invalidModelProblem);
        }

        return await Task.FromResult(new PagedViewModel<SearchResultResponseModel>
        {
            Total = results?.TotalRecords ?? 0,
            Items = results?.Results?.Select(x => new SearchResultViewModel
            {
                Id = x.Id,
                Score = x.Score,
                Fields = x.Values.OrderBy(y => y.Key).Select(y => new FieldViewModel { Name = y.Key, Values =  y.Value.Select(x=>x?.ToString() ?? String.Empty) }),
            }) ?? new List<SearchResultViewModel>()
        });
    }
}

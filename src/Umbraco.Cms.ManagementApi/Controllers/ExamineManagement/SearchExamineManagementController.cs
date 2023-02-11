using Examine;
using Examine.Search;
using Lucene.Net.QueryParsers.Classic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.ManagementApi.Services;
using Umbraco.Cms.ManagementApi.ViewModels.ExamineManagement;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.ExamineManagement;

[ApiVersion("1.0")]
public class SearchExamineManagementController : ExamineManagementControllerBase
{
    private readonly IExamineManagerService _examineManagerService;

    public SearchExamineManagementController(IExamineManagerService examineManagerService) => _examineManagerService = examineManagerService;

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<PagedViewModel<SearchResultViewModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedViewModel<SearchResultViewModel>>> GetSearchResults(string searcherName, string? query, int skip, int take)
    {
        query = query?.Trim();

        if (query.IsNullOrWhiteSpace())
        {
            return new PagedViewModel<SearchResultViewModel>();
        }

        if (!_examineManagerService.TryFindSearcher(searcherName, out ISearcher searcher))
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

        ISearchResults results;

        // NativeQuery will work for a single word/phrase too (but depends on the implementation) the lucene one will work.
        try
        {
            results = searcher
                .CreateQuery()
                .NativeQuery(query)
                .Execute(QueryOptions.SkipTake(skip, take));
        }
        catch (ParseException)
        {
            var invalidModelProblem = new ProblemDetails
            {
                Title = "Could not parse the query",
                Detail = "Parser could not parse the query. Please double check if the query is valid. Sometimes this can also happen if your query starts with a wildcard (*)",
                Status = StatusCodes.Status404NotFound,
                Type = "Error",
            };

            return BadRequest(invalidModelProblem);
        }

        return await Task.FromResult(new PagedViewModel<SearchResultViewModel>
        {
            Total = results.TotalItemCount,
            Items = results.Select(x => new SearchResultViewModel
            {
                Id = x.Id,
                Score = x.Score,
                Fields = x.AllValues.OrderBy(y => y.Key).Select(y => new FieldViewModel { Name = y.Key, Values = y.Value }),
            }),
        });
    }
}

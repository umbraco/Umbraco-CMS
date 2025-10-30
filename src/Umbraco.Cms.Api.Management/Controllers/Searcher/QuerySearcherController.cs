using Asp.Versioning;
using Examine;
using Examine.Lucene.Search;
using Examine.Search;
using Lucene.Net.QueryParsers.Classic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Searcher;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Searcher;

[ApiVersion("1.0")]
public class QuerySearcherController : SearcherControllerBase
{
    private readonly IExamineManagerService _examineManagerService;

    public QuerySearcherController(IExamineManagerService examineManagerService) => _examineManagerService = examineManagerService;

    [Microsoft.AspNetCore.Mvc.HttpGet("{searcherName}/query")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<SearchResultResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public Task<ActionResult<PagedViewModel<SearchResultResponseModel>>> Query(
        CancellationToken cancellationToken,
        string searcherName,
        string? term,
        int skip = 0,
        int take = 100)
    {
        term = term?.Trim();

        if (term.IsNullOrWhiteSpace())
        {
            return Task.FromResult<ActionResult<PagedViewModel<SearchResultResponseModel>>>(new PagedViewModel<SearchResultResponseModel>());
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

            return Task.FromResult<ActionResult<PagedViewModel<SearchResultResponseModel>>>(NotFound(invalidModelProblem));
        }

        ISearchResults results;

        // NativeQuery will work for a single word/phrase too (but depends on the implementation) the lucene one will work.
        // Due to examine changes we need to supply the skipTakeMaxResults, see https://github.com/umbraco/Umbraco-CMS/issues/17920 for more info
        try
        {
            results = searcher
                .CreateQuery()
                .NativeQuery(term)
                .Execute(new LuceneQueryOptions(skip, take, skipTakeMaxResults: skip + take));
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

            return Task.FromResult<ActionResult<PagedViewModel<SearchResultResponseModel>>>(BadRequest(invalidModelProblem));
        }

        return Task.FromResult<ActionResult<PagedViewModel<SearchResultResponseModel>>>(new PagedViewModel<SearchResultResponseModel>
        {
            Total = results.TotalItemCount,
            Items = results.Select(x => new SearchResultResponseModel
            {
                Id = x.Id,
                Score = x.Score,
                Fields = x.AllValues.OrderBy(y => y.Key).Select(y => new FieldPresentationModel { Name = y.Key, Values = y.Value }),
            }),
        });
    }
}

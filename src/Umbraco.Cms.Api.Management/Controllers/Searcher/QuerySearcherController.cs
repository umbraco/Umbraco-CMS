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

/// <summary>
/// Controller responsible for handling search queries within the management API.
/// </summary>
[ApiVersion("1.0")]
public class QuerySearcherController : SearcherControllerBase
{
    private readonly IExamineManagerService _examineManagerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuerySearcherController"/> class.
    /// </summary>
    /// <param name="examineManagerService">An instance of <see cref="IExamineManagerService"/> used to manage examine operations.</param>
    public QuerySearcherController(IExamineManagerService examineManagerService) => _examineManagerService = examineManagerService;

    /// <summary>
    /// Executes a search query against the specified searcher and returns a paged list of results.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="searcherName">The name of the searcher to query.</param>
    /// <param name="term">The search term to query for. If null or whitespace, an empty result set is returned.</param>
    /// <param name="skip">The number of results to skip for paging.</param>
    /// <param name="take">The number of results to return for paging.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains an <see cref="ActionResult{T}"/> with a <see cref="PagedViewModel{SearchResultResponseModel}"/> of search results.
    /// Returns <c>NotFound</c> if the specified searcher does not exist, or <c>BadRequest</c> if the query cannot be parsed.
    /// </returns>
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

using Examine;
using Examine.Search;
using Lucene.Net.QueryParsers.Classic;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.ManagementApi.ViewModels.ExamineManagement;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.ExamineManagement;

public class SearchExamineManagementController : ExamineManagementControllerBase
{
    private readonly IExamineManager _examineManager;

    public SearchExamineManagementController(IExamineManager examineManager) => _examineManager = examineManager;

    [HttpGet("search")]
    public ActionResult<PagedViewModel<SearchResultViewModel>> GetSearchResults(string searcherName, string? query, int skip, int take)
    {
        query = query?.Trim();

        if (query.IsNullOrWhiteSpace())
        {
            return new PagedViewModel<SearchResultViewModel>();
        }

        ActionResult msg = ValidateSearcher(searcherName, out ISearcher searcher);
        if (!msg.IsSuccessStatusCode())
        {
            return msg;
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
            // will occur if the query parser cannot parse this (i.e. starts with a *)
            return new PagedViewModel<SearchResultViewModel>();
        }

        return new PagedViewModel<SearchResultViewModel>
        {
            Total= results.TotalItemCount,
            Items = results.Select(x => new SearchResultViewModel
            {
                Id = x.Id,
                Score = x.Score,
                Values = x.AllValues.OrderBy(y => y.Key).ToDictionary(y => y.Key, y => y.Value),
            }),
        };
    }

    private ActionResult ValidateSearcher(string searcherName, out ISearcher searcher)
    {
        //try to get the searcher from the indexes
        if (_examineManager.TryGetIndex(searcherName, out IIndex index))
        {
            searcher = index.Searcher;
            return new OkResult();
        }

        //if we didn't find anything try to find it by an explicitly declared searcher
        if (_examineManager.TryGetSearcher(searcherName, out searcher))
        {
            return new OkResult();
        }

        var response1 = new BadRequestObjectResult($"No searcher found with name = {searcherName}");
        HttpContext.SetReasonPhrase("Searcher Not Found");
        return response1;
    }
}

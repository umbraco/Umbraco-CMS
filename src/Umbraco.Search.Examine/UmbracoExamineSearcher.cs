using Examine;
using Examine.Search;
using Umbraco.Cms.Core.Models.Search;

namespace Umbraco.Search.Examine;

public class UmbracoExamineSearcher<T> : IUmbracoSearcher<T>
{
    private readonly ISearcher _examineIndex;
    public UmbracoExamineSearcher(ISearcher examineIndex)
    {
        _examineIndex = examineIndex;
    }


    public UmbracoSearchResults Search(string term, int page, int pageSize)
    {
        var searchResult=  _examineIndex.CreateQuery().ManagedQuery(term).Execute(QueryOptions.SkipTake(pageSize * page, pageSize));
        return new UmbracoSearchResults(searchResult.TotalItemCount,
            searchResult.Select(x => new UmbracoSearchResult(x.Id, x.Score, x.Values)));

    }
    public string Name => _examineIndex.Name;

    public UmbracoSearchResults? NativeQuery(string query, int page, int pageSize)
    {
      var searchResult=  _examineIndex.CreateQuery().NativeQuery(query).Execute(QueryOptions.SkipTake(pageSize * page, pageSize));
      return new UmbracoSearchResults(searchResult.TotalItemCount,
          searchResult.Select(x => new UmbracoSearchResult(x.Id, x.Score, x.Values)));
    }
}

using Examine;
using Examine.Search;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using Umbraco.Search.Examine.TBD;

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
        var searchResult = _examineIndex.CreateQuery().ManagedQuery(term)
            .Execute(QueryOptions.SkipTake(pageSize * page, pageSize));
        return new UmbracoSearchResults(searchResult.TotalItemCount,
            searchResult.Select(x => new UmbracoSearchResult(x.Id, x.Score, x.Values)));
    }

    public string Name => _examineIndex.Name;

    public UmbracoSearchResults? NativeQuery(string query, int page, int pageSize)
    {
        var searchResult = _examineIndex.CreateQuery().NativeQuery(query)
            .Execute(QueryOptions.SkipTake(pageSize * page, pageSize));
        return new UmbracoSearchResults(searchResult.TotalItemCount,
            searchResult.Select(x => new UmbracoSearchResult(x.Id, x.Score, x.Values)));
    }

    public IEnumerable<PublishedSearchResult> SearchDescendants(
        IPublishedContent content,
        IUmbracoContextAccessor umbracoContextAccessor,
        string term)
    {
        // var t = term.Escape().Value;
        // var luceneQuery = "+__Path:(" + content.Path.Replace("-", "\\-") + "*) +" + t;
        IBooleanOperation? query = _examineIndex.CreateQuery()
            .Field(UmbracoSearchFieldNames.IndexPathFieldName, (content.Path + ",").MultipleCharacterWildcard())
            .And()
            .ManagedQuery(term);
        IUmbracoContext umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();
        return query.Execute().ToPublishedSearchResults(umbracoContext.Content);
    }

    public IEnumerable<PublishedSearchResult> SearchChildren(
        IPublishedContent content,
        IUmbracoContextAccessor umbracoContextAccessor,
        string term)
    {
        IBooleanOperation? query = _examineIndex.CreateQuery()
            .Field("parentID", content.Id)
            .And()
            .ManagedQuery(term);
        IUmbracoContext umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();

        return query.Execute().ToPublishedSearchResults(umbracoContext.Content);
    }

    public IUmbracoSearchResults Search(string[] fields, string[] values, int page, int pageSize,
        LogicOperator logicOperator = LogicOperator.OR) =>
        throw new NotImplementedException();
}

using Examine;
using Examine.Search;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using Umbraco.Search.Examine.TBD;
using Umbraco.Search.Models;

namespace Umbraco.Search.Examine;

public class UmbracoExamineSearcher<T> : IUmbracoSearcher<T>
{
    private readonly ISearcher _examineIndex;
    private readonly IUmbracoContextFactory _umbracoContextFactory;

    public UmbracoExamineSearcher(ISearcher examineIndex, IUmbracoContextFactory umbracoContextFactory)
    {
        _examineIndex = examineIndex;
        _umbracoContextFactory = umbracoContextFactory;
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
        string term)
    {
        // var t = term.Escape().Value;
        // var luceneQuery = "+__Path:(" + content.Path.Replace("-", "\\-") + "*) +" + t;
        IBooleanOperation? query = _examineIndex.CreateQuery()
            .Field(UmbracoSearchFieldNames.IndexPathFieldName, (content.Path + ",").MultipleCharacterWildcard())
            .And()
            .ManagedQuery(term);
        using (var contextReference = _umbracoContextFactory.EnsureUmbracoContext())
        {
            IUmbracoContext umbracoContext = contextReference.UmbracoContext;
            return query.Execute().ToPublishedSearchResults(umbracoContext.Content);
        }
    }

    public IEnumerable<PublishedSearchResult> SearchChildren(
        IPublishedContent content,
        string term)
    {
        IBooleanOperation? query = _examineIndex.CreateQuery()
            .Field("parentID", content.Id)
            .And()
            .ManagedQuery(term);
        using (var contextReference = _umbracoContextFactory.EnsureUmbracoContext())
        {
            return query.Execute().ToPublishedSearchResults(contextReference.UmbracoContext.Content);
        }
    }

    public IUmbracoSearchResults Search(ISearchRequest searchRequest)
    {
        var query = _examineIndex.CreateQuery();
        IBooleanOperation? booleanOperation = null;
        if (!string.IsNullOrWhiteSpace(searchRequest.Term))
        {
            booleanOperation = query.ManagedQuery(searchRequest.Term);
        }
        else
        {
            booleanOperation = query.Field("", "");
        }

        /*foreach (var filter in searchRequest.Filters)
        {
            switch (searchRequest.FiltersLogicOperator)
            {
                case LogicOperator.OR:
                    booleanOperation = booleanOperation.Or(x =>
                    {
                        if (filter.Values.Any())
                        {
                            if (filter.Values.Count == 1)
                            {
                                return x.Field(filter.FieldName, filter.Values.First());
                            }

                            return x.GroupedOr(new[] { filter.FieldName }, filter.Values.ToArray());
                        }

                        if (filter.SubFilters.Any())
                        {
                            switch (filter.LogicOperator)
                            {
                                case LogicOperator.OR:
                                    f
                            }
                        }
                    })
                    break;
            }

            (searchRequest.FiltersLogicOperator)
        }*/
        using (var contextReference = _umbracoContextFactory.EnsureUmbracoContext())
        {
            IUmbracoContext umbracoContext = contextReference.UmbracoContext;
            var searchResult = booleanOperation.Execute();
            return new UmbracoSearchResults(searchResult.TotalItemCount,
                searchResult.Select(x => new UmbracoSearchResult(x.Id, x.Score, x.Values)));
        }
    }

    public ISearchRequest CreateSearchRequest()
    {
        return new DefaultSearchRequest(string.Empty, new List<ISearchFilter>(), LogicOperator.OR);
    }
}

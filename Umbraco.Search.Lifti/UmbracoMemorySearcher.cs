using Examine;
using Examine.Search;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Web;
using Umbraco.Search.Lifti.Extensions;
using Umbraco.Search.Models;

namespace Umbraco.Search.Lifti;

public class UmbracoMemorySearcher<T> : IUmbracoSearcher<T>
{
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    private readonly ILiftiIndex? _liftiIndex;

    public UmbracoMemorySearcher(IUmbracoContextFactory umbracoContextFactory, ILiftiIndex? liftiIndex, string name)
    {
        _umbracoContextFactory = umbracoContextFactory;
        _liftiIndex = liftiIndex;
        Name = name;
    }

    public UmbracoSearchResults Search(string term, int page, int pageSize)
    {
        var allResult = _liftiIndex?.LiftiIndex.Search(term);
        if (allResult != null)
        {
            var results =allResult.Skip(pageSize * page).Take(pageSize).ToUmbracoResults();
            return new UmbracoSearchResults(allResult.Count(), pageSize,results);
        }
        return new UmbracoSearchResults(0, pageSize,new List<IUmbracoSearchResult>());
    }
    public string Name { get; }

    public UmbracoSearchResults? NativeQuery(string query, int page, int pageSize)
    {
        var allResult = _liftiIndex?.LiftiIndex.Search(query);
        if (allResult != null)
        {
            var results =allResult.Skip(pageSize * page).Take(pageSize).ToUmbracoResults();
            return new UmbracoSearchResults(allResult.Count(), pageSize,results);
        }
        return new UmbracoSearchResults(0, pageSize,new List<IUmbracoSearchResult>());
    }

    public IEnumerable<PublishedSearchResult> SearchDescendants(IPublishedContent content, string term)
    {
        var query = $"{UmbracoSearchFieldNames.IndexPathFieldName}={content.Path}* &{term}";
        var allResult = _liftiIndex?.LiftiIndex.Search(query);


        using (var contextReference = _umbracoContextFactory.EnsureUmbracoContext())
        {
            IUmbracoContext umbracoContext = contextReference.UmbracoContext;
            if (allResult != null)
            {
                return allResult.ToPublishedSearchResults(umbracoContext.Content);
            }
        }
        return new List<PublishedSearchResult>();
    }

    public IEnumerable<PublishedSearchResult> SearchChildren(IPublishedContent content, string term)
    {
        var query = $"{UmbracoSearchFieldNames.ParentID}={content.Id} &{term}";
        var allResult = _liftiIndex?.LiftiIndex.Search(query);


        using (var contextReference = _umbracoContextFactory.EnsureUmbracoContext())
        {
            IUmbracoContext umbracoContext = contextReference.UmbracoContext;
            if (allResult != null)
            {
                return allResult.ToPublishedSearchResults(umbracoContext.Content);
            }
        }
        return new List<PublishedSearchResult>();
    }

    public IUmbracoSearchResults Search(ISearchRequest searchRequest) => throw new NotImplementedException();

    public ISearchRequest CreateSearchRequest()
    {
        return new DefaultSearchRequest(string.Empty, new List<ISearchFilter>(), LogicOperator.OR);

    }

    public IEnumerable<PublishedSearchResult> GetAll()
    {
        var allResult = _liftiIndex?.LiftiIndex.Search("*");


        using (var contextReference = _umbracoContextFactory.EnsureUmbracoContext())
        {
            IUmbracoContext umbracoContext = contextReference.UmbracoContext;
            if (allResult != null)
            {
                return allResult.ToPublishedSearchResults(umbracoContext.Content);
            }
        }
        return new List<PublishedSearchResult>();
    }
}

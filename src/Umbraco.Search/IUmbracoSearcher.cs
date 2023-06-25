using System.Runtime.CompilerServices;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Search;
public interface IUmbracoSearcher<T> : IUmbracoSearcher
{
}

public interface IUmbracoSearcher
{
    public UmbracoSearchResults Search(string term, int page, int pageSize);

    string Name { get;  }
    UmbracoSearchResults? NativeQuery(string query, int page, int pageSize);

    IEnumerable<PublishedSearchResult> SearchDescendants(
        IPublishedContent content,
        IUmbracoContextAccessor umbracoContextAccessor,
        string term);

    IEnumerable<PublishedSearchResult> SearchChildren(IPublishedContent content, IUmbracoContextAccessor umbracoContextAccessor, string term);
    IUmbracoSearchResults Search(string[] fields, string[] values, LogicOperator logicOperator = LogicOperator.OR);
}

public enum LogicOperator
{
    OR,
    And
}

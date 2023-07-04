using System.Runtime.CompilerServices;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Cms.Core.Web;
using Umbraco.Search.Models;

namespace Umbraco.Search;

public interface IUmbracoSearcher<T> : IUmbracoSearcher
{
}

public interface IUmbracoSearcher
{
    public UmbracoSearchResults Search(string term, int page, int pageSize);

    string Name { get; }
    UmbracoSearchResults? NativeQuery(string query, int page, int pageSize);

    IEnumerable<PublishedSearchResult> SearchDescendants(
        IPublishedContent content,
        string term);

    IEnumerable<PublishedSearchResult> SearchChildren(IPublishedContent content, string term);

    IUmbracoSearchResults Search(ISearchRequest searchRequest);
    public ISearchRequest CreateSearchRequest();

}

public interface ISearchRequest
{
    string Term { get; set; }

    IList<ISearchFilter> Filters { get; set; }
    LogicOperator FiltersLogicOperator { get; set; }
    int Page { get; set; }
    int PageSize { get; set; }
    ISearchRequest CreateFilter(string name, IList<string> values, LogicOperator logicOperator);
    void SortBy(string sortFieldName, SortType sortType);
}

public interface ISearchFilter
{
    IList<ISearchFilter> SubFilters { get; set; }
    string FieldName { get; set; }
    IList<string> Values { get; set; }
    LogicOperator LogicOperator { get; set; }
    ISearchFilter CreateSubFilter(string fieldName, List<string> values, LogicOperator or);
}

public enum LogicOperator
{
    OR,
    And,
    Not
}

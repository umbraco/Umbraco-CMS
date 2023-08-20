using Umbraco.Search.Models;

namespace Umbraco.Search;

/// <summary>
///    A Marker interface for defining an Umbraco search request
/// </summary>
public interface ISearchRequest
{
    string Term { get; set; }

    IList<ISearchFilter> Filters { get; set; }
    LogicOperator FiltersLogicOperator { get; set; }
    int Page { get; set; }
    int PageSize { get; set; }
    bool Preview { get; set; }
    ISearchRequest CreateFilter(string name, IList<string> values, LogicOperator logicOperator);
    void SortBy(string sortFieldName, SortType sortType, bool descanding = false);
}

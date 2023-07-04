namespace Umbraco.Search.Models;

public class DefaultSearchRequest : ISearchRequest
{
    public DefaultSearchRequest(string term, IList<ISearchFilter> filters, LogicOperator filtersLogicOperator)
    {
        Term = term;
        Filters = filters;
        FiltersLogicOperator = filtersLogicOperator;
    }

    public string Term { get; set; }
    public IList<ISearchFilter> Filters { get; set; }
    public LogicOperator FiltersLogicOperator { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool Preview { get; set; }
    public ISearchRequest CreateFilter(string name, IList<string> values, LogicOperator logicOperator) => throw new NotImplementedException();

    public void SortBy(string sortFieldName, SortType sortType) => throw new NotImplementedException();
}

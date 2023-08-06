namespace Umbraco.Search;

public interface ISearchFilter
{
    IList<ISearchFilter> SubFilters { get; set; }
    string FieldName { get; set; }
    IList<string> Values { get; set; }
    LogicOperator LogicOperator { get; set; }
    ISearchFilter CreateSubFilter(string fieldName, List<string> values, LogicOperator or);
}

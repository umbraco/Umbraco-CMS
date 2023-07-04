namespace Umbraco.Search.Models;

public class DefaultSearchFilter : ISearchFilter
{
    public DefaultSearchFilter(string fieldName, IList<string> values, LogicOperator logicOperator, IList<ISearchFilter> subFilters)
    {
        FieldName = fieldName;
        Values = values;
        LogicOperator = logicOperator;
        SubFilters = subFilters;
    }
    public IList<ISearchFilter> SubFilters { get; set; }
    public string FieldName { get; set; }
    public IList<string> Values { get; set; }
    public LogicOperator LogicOperator { get; set; }
    public ISearchFilter CreateSubFilter(string fieldName, List<string> values, LogicOperator or)
    {
        SubFilters.Add(new DefaultSearchFilter(fieldName, values, or, new List<ISearchFilter>()));
        return this;
    }
}

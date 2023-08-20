namespace Umbraco.Search.Models;

public class DefaultSearchSort : ISearchSort
{
    public DefaultSearchSort(string fieldName)
    {
        FieldName = fieldName;
    }

    public string FieldName { get; set; }
    public SortType SortType { get; set; }
    public bool Descending { get; set; }
}

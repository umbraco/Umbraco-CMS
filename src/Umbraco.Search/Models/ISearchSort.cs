namespace Umbraco.Search.Models;

public interface ISearchSort
{
    string FieldName { get; set; }
    SortType SortType { get; set; }
    bool Descending { get; set; }
}

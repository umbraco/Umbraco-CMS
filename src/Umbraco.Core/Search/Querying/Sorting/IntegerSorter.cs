namespace Umbraco.Cms.Core.Search.Querying.Sorting;

/// <summary>
/// Sorts search results by an integer field.
/// </summary>
/// <param name="FieldName">The name of the field to sort by.</param>
/// <param name="Direction">The sort direction.</param>
public record IntegerSorter(string FieldName, Direction Direction)
    : Sorter(FieldName, Direction)
{
}

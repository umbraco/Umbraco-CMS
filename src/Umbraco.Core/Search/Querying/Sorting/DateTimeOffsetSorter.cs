namespace Umbraco.Cms.Core.Search.Querying.Sorting;

/// <summary>
/// Sorts search results by a <c>DateTimeOffset</c> field.
/// </summary>
/// <param name="FieldName">The name of the field to sort by.</param>
/// <param name="Direction">The sort direction.</param>
public record DateTimeOffsetSorter(string FieldName, Direction Direction)
    : Sorter(FieldName, Direction)
{
}

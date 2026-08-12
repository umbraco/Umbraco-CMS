using Umbraco.Cms.Core;

namespace Umbraco.Cms.Search.Core.Models.Searching.Sorting;

/// <summary>
/// Sorts search results by a decimal field.
/// </summary>
/// <param name="FieldName">The name of the field to sort by.</param>
/// <param name="Direction">The sort direction.</param>
public record DecimalSorter(string FieldName, Direction Direction)
    : Sorter(FieldName, Direction)
{
}

using Umbraco.Cms.Core;

namespace Umbraco.Cms.Core.Search.Querying.Sorting;

/// <summary>
/// Represents a request to sort search results by a field, passed to <see cref="Umbraco.Cms.Core.Search.ISearcher.SearchAsync"/>. When multiple sorters are supplied, the first is primary.
/// </summary>
/// <param name="FieldName">The name of the field to sort by.</param>
/// <param name="Direction">The sort direction.</param>
public abstract record Sorter(string FieldName, Direction Direction)
{
}

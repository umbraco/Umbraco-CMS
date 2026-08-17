namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

/// <summary>
/// Base type for one bucket in a facet result.
/// </summary>
/// <param name="Count">The number of matching documents in this bucket.</param>
public abstract record FacetValue(long Count)
{
}

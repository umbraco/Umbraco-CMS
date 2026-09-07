namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

/// <summary>
/// Base type for one exact-value bucket in an exact facet's result.
/// </summary>
/// <typeparam name="T">The type of the bucketed value.</typeparam>
/// <param name="Key">The exact value this bucket represents.</param>
/// <param name="Count">The number of matching documents with this value.</param>
public abstract record ExactFacetValue<T>(T Key, long Count)
    : FacetValue(Count)
{
}

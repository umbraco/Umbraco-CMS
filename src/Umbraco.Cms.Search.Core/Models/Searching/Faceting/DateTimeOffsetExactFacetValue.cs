namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

/// <summary>
/// One date-value bucket in a <see cref="DateTimeOffsetExactFacet"/> result.
/// </summary>
/// <param name="Key">The exact date value this bucket represents.</param>
/// <param name="Count">The number of matching documents with this value.</param>
public record DateTimeOffsetExactFacetValue(DateTimeOffset Key, long Count)
    : ExactFacetValue<DateTimeOffset>(Key, Count)
{
}

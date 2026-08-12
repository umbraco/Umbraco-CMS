namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

/// <summary>
/// One integer-value bucket in an <see cref="IntegerExactFacet"/> result.
/// </summary>
/// <param name="Key">The exact integer value this bucket represents.</param>
/// <param name="Count">The number of matching documents with this value.</param>
public record IntegerExactFacetValue(int Key, long Count)
    : ExactFacetValue<int>(Key, Count)
{
}

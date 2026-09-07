namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

/// <summary>
/// One decimal-value bucket in a <see cref="DecimalExactFacet"/> result.
/// </summary>
/// <param name="Key">The exact decimal value this bucket represents.</param>
/// <param name="Count">The number of matching documents with this value.</param>
public record DecimalExactFacetValue(decimal Key, long Count)
    : ExactFacetValue<decimal>(Key, Count)
{
}

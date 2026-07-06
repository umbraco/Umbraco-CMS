namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public record DecimalExactFacetValue(decimal Key, long Count)
    : ExactFacetValue<decimal>(Key, Count)
{
}

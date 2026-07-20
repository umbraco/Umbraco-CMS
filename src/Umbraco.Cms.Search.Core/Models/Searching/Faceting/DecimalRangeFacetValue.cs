namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public record DecimalRangeFacetValue(string Key, decimal? Min, decimal? Max, long Count)
    : RangeFacetValue<decimal?>(Key, Min, Max, Count)
{
}

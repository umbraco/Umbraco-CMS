namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public record DecimalRangeFacetRange(string Key, decimal? MinValue, decimal? MaxValue)
    : RangeFacetRange<decimal?>(Key, MinValue, MaxValue)
{
}

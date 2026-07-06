namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

public record DecimalRangeFilterRange(decimal? MinValue, decimal? MaxValue)
    : RangeFilterRange<decimal?>(MinValue, MaxValue)
{
}

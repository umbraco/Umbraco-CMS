namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public record DateTimeOffsetRangeFacetRange(string Key, DateTimeOffset? MinValue, DateTimeOffset? MaxValue)
    : RangeFacetRange<DateTimeOffset?>(Key, MinValue, MaxValue)
{
}

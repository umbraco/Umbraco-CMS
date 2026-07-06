namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public record DateTimeOffsetRangeFacetValue(string Key, DateTimeOffset? Min, DateTimeOffset? Max, long Count)
    : RangeFacetValue<DateTimeOffset?>(Key, Min, Max, Count)
{
}

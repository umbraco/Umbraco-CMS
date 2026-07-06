namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

public record DateTimeOffsetRangeFilterRange(DateTimeOffset? MinValue, DateTimeOffset? MaxValue)
    : RangeFilterRange<DateTimeOffset?>(MinValue, MaxValue)
{
}

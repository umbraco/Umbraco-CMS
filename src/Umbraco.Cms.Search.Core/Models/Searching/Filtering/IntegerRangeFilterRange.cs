namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

public record IntegerRangeFilterRange(int? MinValue, int? MaxValue)
    : RangeFilterRange<int?>(MinValue, MaxValue)
{
}

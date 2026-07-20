namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public record IntegerRangeFacetRange(string Key, int? MinValue, int? MaxValue)
    : RangeFacetRange<int?>(Key, MinValue, MaxValue)
{
}

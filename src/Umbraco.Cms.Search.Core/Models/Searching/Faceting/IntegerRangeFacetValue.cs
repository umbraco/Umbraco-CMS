namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public record IntegerRangeFacetValue(string Key, int? Min, int? Max, long Count)
    : RangeFacetValue<int?>(Key, Min, Max, Count)
{
}

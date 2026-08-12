namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

/// <summary>
/// One range bucket in an <see cref="IntegerRangeFacet"/> result, with an inclusive lower bound and an exclusive upper bound.
/// </summary>
/// <param name="Key">The identifier of the requested range this bucket corresponds to.</param>
/// <param name="Min">The inclusive lower boundary of the bucket, or null for an open-ended lower bound.</param>
/// <param name="Max">The exclusive upper boundary of the bucket, or null for an open-ended upper bound.</param>
/// <param name="Count">The number of matching documents within the range.</param>
public record IntegerRangeFacetValue(string Key, int? Min, int? Max, long Count)
    : RangeFacetValue<int?>(Key, Min, Max, Count)
{
}

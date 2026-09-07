namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

/// <summary>
/// One requested bucket for a <see cref="DateTimeOffsetRangeFacet"/>, with an inclusive lower bound and an exclusive upper bound.
/// </summary>
/// <param name="Key">An identifier for this bucket, echoed back on the matching result value.</param>
/// <param name="MinValue">The inclusive lower boundary of the bucket, or null for an open-ended lower bound.</param>
/// <param name="MaxValue">The exclusive upper boundary of the bucket, or null for an open-ended upper bound.</param>
public record DateTimeOffsetRangeFacetRange(string Key, DateTimeOffset? MinValue, DateTimeOffset? MaxValue)
    : RangeFacetRange<DateTimeOffset?>(Key, MinValue, MaxValue)
{
}

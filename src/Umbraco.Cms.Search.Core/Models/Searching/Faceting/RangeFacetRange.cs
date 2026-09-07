namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

/// <summary>
/// Base type describing one requested bucket for a range facet, with an inclusive lower bound and an exclusive upper bound.
/// </summary>
/// <typeparam name="T">The type of the range boundary values.</typeparam>
/// <param name="Key">An identifier for this bucket, echoed back on the matching result value.</param>
/// <param name="MinValue">The inclusive lower boundary of the bucket, or null for an open-ended lower bound.</param>
/// <param name="MaxValue">The exclusive upper boundary of the bucket, or null for an open-ended upper bound.</param>
public abstract record RangeFacetRange<T>(string Key, T MinValue, T MaxValue)
{
}

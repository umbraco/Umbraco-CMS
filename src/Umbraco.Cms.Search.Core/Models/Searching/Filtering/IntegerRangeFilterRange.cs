namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

/// <summary>
/// A single range for <see cref="IntegerRangeFilter"/>, with an inclusive lower bound and an exclusive upper bound.
/// </summary>
/// <param name="MinValue">The inclusive lower bound, or null for no lower bound.</param>
/// <param name="MaxValue">The exclusive upper bound, or null for no upper bound.</param>
public record IntegerRangeFilterRange(int? MinValue, int? MaxValue)
    : RangeFilterRange<int?>(MinValue, MaxValue)
{
}

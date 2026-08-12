namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

/// <summary>
/// Base type describing a single range within a range filter, with an inclusive lower bound and an exclusive upper bound.
/// </summary>
/// <typeparam name="T">The type of the range bounds.</typeparam>
/// <param name="MinValue">The inclusive lower bound, or null for no lower bound.</param>
/// <param name="MaxValue">The exclusive upper bound, or null for no upper bound.</param>
public abstract record RangeFilterRange<T>(T MinValue, T MaxValue)
{
}

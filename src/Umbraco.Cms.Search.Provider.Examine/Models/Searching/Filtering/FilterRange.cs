namespace Umbraco.Cms.Search.Provider.Examine.Models.Searching.Filtering;

/// <summary>
/// Provider-internal range bound passed to Examine's range query API, with an inclusive lower bound and an exclusive upper bound.
/// </summary>
/// <typeparam name="T">The type of the range bounds.</typeparam>
/// <param name="MinValue">The inclusive lower bound.</param>
/// <param name="MaxValue">The exclusive upper bound.</param>
internal record FilterRange<T>(T MinValue, T MaxValue)
    where T : struct
{
}

namespace Umbraco.Cms.Search.Provider.Examine.Models.Searching.Filtering;

internal record FilterRange<T>(T MinValue, T MaxValue)
    where T : struct
{
}

namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public abstract record RangeFacetRange<T>(string Key, T MinValue, T MaxValue)
{
}

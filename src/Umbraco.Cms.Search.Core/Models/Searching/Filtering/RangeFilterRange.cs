namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

public abstract record RangeFilterRange<T>(T MinValue, T MaxValue)
{
}

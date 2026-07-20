namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public abstract record RangeFacetValue<T>(string Key, T? Min, T? Max, long Count)
    : FacetValue(Count)
{
}

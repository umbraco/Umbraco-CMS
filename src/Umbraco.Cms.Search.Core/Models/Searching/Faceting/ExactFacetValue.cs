namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public abstract record ExactFacetValue<T>(T Key, long Count)
    : FacetValue(Count)
{
}

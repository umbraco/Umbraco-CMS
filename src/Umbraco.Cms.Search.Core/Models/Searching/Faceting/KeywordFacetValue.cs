namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public record KeywordFacetValue(string Key, long Count)
    : ExactFacetValue<string>(Key, Count)
{
}

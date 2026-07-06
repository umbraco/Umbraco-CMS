namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public record DateTimeOffsetExactFacetValue(DateTimeOffset Key, long Count)
    : ExactFacetValue<DateTimeOffset>(Key, Count)
{
}

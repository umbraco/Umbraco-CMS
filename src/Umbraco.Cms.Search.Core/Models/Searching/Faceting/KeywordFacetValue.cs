namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

/// <summary>
/// One keyword-value bucket in a <see cref="KeywordFacet"/> result.
/// </summary>
/// <param name="Key">The keyword value this bucket represents.</param>
/// <param name="Count">The number of matching documents with this value.</param>
public record KeywordFacetValue(string Key, long Count)
    : ExactFacetValue<string>(Key, Count)
{
}

namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

/// <summary>
/// An exact-value facet on a keyword (string) field.
/// </summary>
/// <param name="FieldName">The name of the index field to facet on.</param>
public record KeywordFacet(string FieldName)
    : ExactFacet(FieldName)
{
}

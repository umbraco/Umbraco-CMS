namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

/// <summary>
/// An exact-value facet on an integer field.
/// </summary>
/// <param name="FieldName">The name of the index field to facet on.</param>
public record IntegerExactFacet(string FieldName)
    : ExactFacet(FieldName)
{
}

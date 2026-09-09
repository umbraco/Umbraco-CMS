namespace Umbraco.Cms.Core.Search.Querying.Faceting;

/// <summary>
/// An exact-value facet on a date field.
/// </summary>
/// <param name="FieldName">The name of the index field to facet on.</param>
public record DateTimeOffsetExactFacet(string FieldName)
    : ExactFacet(FieldName)
{
}

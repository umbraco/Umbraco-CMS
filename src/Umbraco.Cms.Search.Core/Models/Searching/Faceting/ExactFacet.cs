namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

/// <summary>
/// Base type for a facet that buckets results by each exact value found in the field.
/// </summary>
/// <param name="FieldName">The name of the index field to facet on.</param>
public abstract record ExactFacet(string FieldName)
    : Facet(FieldName)
{
}

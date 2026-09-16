namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

/// <summary>
/// Base type for a facet request: computes an aggregation over a single index field.
/// </summary>
/// <param name="FieldName">The name of the index field to facet on.</param>
public abstract record Facet(string FieldName)
{
}

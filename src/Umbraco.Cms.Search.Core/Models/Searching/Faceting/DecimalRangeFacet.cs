namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

/// <summary>
/// A range facet on a decimal field.
/// </summary>
/// <param name="FieldName">The name of the index field to facet on.</param>
/// <param name="Ranges">The requested decimal bucket ranges.</param>
public record DecimalRangeFacet(string FieldName, DecimalRangeFacetRange[] Ranges)
    : RangeFacet<DecimalRangeFacetRange>(FieldName, Ranges)
{
}

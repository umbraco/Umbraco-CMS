namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

/// <summary>
/// A range facet on an integer field.
/// </summary>
/// <param name="FieldName">The name of the index field to facet on.</param>
/// <param name="Ranges">The requested integer bucket ranges.</param>
public record IntegerRangeFacet(string FieldName, IntegerRangeFacetRange[] Ranges)
    : RangeFacet<IntegerRangeFacetRange>(FieldName, Ranges)
{
}

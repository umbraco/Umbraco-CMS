namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

/// <summary>
/// A range facet on a date field.
/// </summary>
/// <param name="FieldName">The name of the index field to facet on.</param>
/// <param name="Ranges">The requested date bucket ranges.</param>
public record DateTimeOffsetRangeFacet(string FieldName, DateTimeOffsetRangeFacetRange[] Ranges)
    : RangeFacet<DateTimeOffsetRangeFacetRange>(FieldName, Ranges)
{
}

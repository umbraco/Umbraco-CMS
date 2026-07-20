namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public record DateTimeOffsetRangeFacet(string FieldName, DateTimeOffsetRangeFacetRange[] Ranges)
    : RangeFacet<DateTimeOffsetRangeFacetRange>(FieldName, Ranges)
{
}

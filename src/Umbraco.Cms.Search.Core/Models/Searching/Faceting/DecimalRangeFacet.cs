namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public record DecimalRangeFacet(string FieldName, DecimalRangeFacetRange[] Ranges)
    : RangeFacet<DecimalRangeFacetRange>(FieldName, Ranges)
{
}

namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public record IntegerRangeFacet(string FieldName, IntegerRangeFacetRange[] Ranges)
    : RangeFacet<IntegerRangeFacetRange>(FieldName, Ranges)
{
}

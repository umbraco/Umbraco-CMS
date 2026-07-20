namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public abstract record RangeFacet<TRange>(string FieldName, TRange[] Ranges)
    : Facet(FieldName)
{
}

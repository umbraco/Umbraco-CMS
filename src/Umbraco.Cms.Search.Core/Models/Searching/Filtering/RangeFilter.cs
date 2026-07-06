namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

public abstract record RangeFilter<TRange>(string FieldName, TRange[] Ranges, bool Negate)
    : Filter(FieldName, Negate), IRangeFilter
{
}

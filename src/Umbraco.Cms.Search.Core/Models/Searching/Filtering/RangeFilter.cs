namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

/// <summary>
/// Base type for filters that match documents whose field value falls within one or more ranges.
/// </summary>
/// <typeparam name="TRange">The type describing a single range.</typeparam>
/// <param name="FieldName">The name of the field to filter on.</param>
/// <param name="Ranges">The ranges to match. A document matches if its field value falls within any one of these.</param>
/// <param name="Negate">If true, matches documents whose field value falls outside all of <paramref name="Ranges"/>.</param>
public abstract record RangeFilter<TRange>(string FieldName, TRange[] Ranges, bool Negate)
    : Filter(FieldName, Negate), IRangeFilter
{
}

namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

/// <summary>
/// Base type for a facet that buckets results into a set of requested numeric or date ranges.
/// </summary>
/// <typeparam name="TRange">The concrete range type describing each requested bucket.</typeparam>
/// <param name="FieldName">The name of the index field to facet on.</param>
/// <param name="Ranges">The requested bucket ranges.</param>
public abstract record RangeFacet<TRange>(string FieldName, TRange[] Ranges)
    : Facet(FieldName)
{
}

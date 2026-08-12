namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

/// <summary>
/// Base type for filters that restrict search results to documents matching a condition on a field.
/// </summary>
/// <param name="FieldName">The name of the field to filter on.</param>
/// <param name="Negate">If true, matches documents that do NOT satisfy the filter instead of those that do.</param>
public abstract record Filter(string FieldName, bool Negate)
{
}

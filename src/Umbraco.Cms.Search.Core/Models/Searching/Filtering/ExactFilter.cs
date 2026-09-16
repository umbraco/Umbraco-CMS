namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

/// <summary>
/// Base type for filters that match documents whose field value is one of a set of exact values.
/// </summary>
/// <typeparam name="T">The type of the values to match.</typeparam>
/// <param name="FieldName">The name of the field to filter on.</param>
/// <param name="Values">The values to match. A document matches if its field value is any one of these.</param>
/// <param name="Negate">If true, matches documents whose field value is none of <paramref name="Values"/>.</param>
public abstract record ExactFilter<T>(string FieldName, T[] Values, bool Negate)
    : Filter(FieldName, Negate), IExactFilter
{
}

namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

/// <summary>
/// Base type for filters that match documents whose analyzed text field contains (or does not contain) any of a set of values.
/// </summary>
/// <typeparam name="T">The type of the values to match.</typeparam>
/// <param name="FieldName">The name of the field to filter on.</param>
/// <param name="Values">The values to search for within the field's contents.</param>
/// <param name="Negate">If true, matches documents whose field does not contain any of <paramref name="Values"/>.</param>
public abstract record ContainsFilter<T>(string FieldName, T[] Values, bool Negate)
    : Filter(FieldName, Negate)
{
}

namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

/// <summary>
/// Filters on an analyzed full-text field, matching (or excluding) documents whose field contains any of the given values.
/// </summary>
/// <param name="FieldName">The name of the text field to filter on.</param>
/// <param name="Values">The values to search for within the field's text.</param>
/// <param name="Negate">If true, this becomes a "does not contain" filter, matching documents whose field does not contain any of <paramref name="Values"/>.</param>
public record TextFilter(string FieldName, string[] Values, bool Negate)
    : ContainsFilter<string>(FieldName, Values, Negate)
{
}

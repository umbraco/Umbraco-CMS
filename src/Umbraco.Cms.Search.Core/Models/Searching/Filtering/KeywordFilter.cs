namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

/// <summary>
/// Filters on an exact-match keyword field.
/// </summary>
/// <param name="FieldName">The name of the keyword field to filter on.</param>
/// <param name="Values">The keyword values to match.</param>
/// <param name="Negate">If true, matches documents whose field value is none of <paramref name="Values"/>.</param>
public record KeywordFilter(string FieldName, string[] Values, bool Negate)
    : ExactFilter<string>(FieldName, Values, Negate)
{
}

using Umbraco.Cms.Search.Core.Models.Searching.Filtering;

namespace Umbraco.Cms.Search.Provider.Examine.Models.Searching.Filtering;

/// <summary>
/// Provider-internal exact-match filter on a date/time field, using <see cref="DateTime"/> to match Examine's internal date representation.
/// </summary>
/// <param name="FieldName">The name of the field to filter on.</param>
/// <param name="Values">The values to match.</param>
/// <param name="Negate">If true, matches documents whose field value is none of <paramref name="Values"/>.</param>
internal record DateTimeExactFilter(string FieldName, DateTime[] Values, bool Negate)
    : ExactFilter<DateTime>(FieldName, Values, Negate)
{
}

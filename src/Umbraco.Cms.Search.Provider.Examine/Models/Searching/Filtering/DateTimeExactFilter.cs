using Umbraco.Cms.Search.Core.Models.Searching.Filtering;

namespace Umbraco.Cms.Search.Provider.Examine.Models.Searching.Filtering;

internal record DateTimeExactFilter(string FieldName, DateTime[] Values, bool Negate)
    : ExactFilter<DateTime>(FieldName, Values, Negate)
{
}

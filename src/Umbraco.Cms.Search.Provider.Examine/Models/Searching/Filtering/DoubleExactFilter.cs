using Umbraco.Cms.Search.Core.Models.Searching.Filtering;

namespace Umbraco.Cms.Search.Provider.Examine.Models.Searching.Filtering;

internal record DoubleExactFilter(string FieldName, double[] Values, bool Negate)
    : ExactFilter<double>(FieldName, Values, Negate)
{
}

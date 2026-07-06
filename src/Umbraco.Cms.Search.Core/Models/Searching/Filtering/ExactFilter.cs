namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

public abstract record ExactFilter<T>(string FieldName, T[] Values, bool Negate)
    : Filter(FieldName, Negate), IExactFilter
{
}

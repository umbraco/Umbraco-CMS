namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

public abstract record ContainsFilter<T>(string FieldName, T[] Values, bool Negate)
    : Filter(FieldName, Negate)
{
}

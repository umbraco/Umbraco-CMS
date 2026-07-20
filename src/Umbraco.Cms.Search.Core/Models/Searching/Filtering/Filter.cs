namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

public abstract record Filter(string FieldName, bool Negate)
{
}

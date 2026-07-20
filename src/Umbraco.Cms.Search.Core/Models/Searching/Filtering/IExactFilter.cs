namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

// marker interface for exact filters
public interface IExactFilter
{
    string FieldName { get; }
}

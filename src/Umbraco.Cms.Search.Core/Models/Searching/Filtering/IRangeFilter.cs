namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

/// <summary>
/// Marker interface implemented by all range filter types.
/// </summary>
public interface IRangeFilter
{
    /// <summary>
    /// Gets the name of the field the filter targets.
    /// </summary>
    string FieldName { get; }
}

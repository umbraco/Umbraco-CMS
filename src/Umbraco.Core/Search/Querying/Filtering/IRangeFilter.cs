namespace Umbraco.Cms.Core.Search.Querying.Filtering;

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

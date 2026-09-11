namespace Umbraco.Cms.Core.Search.Querying.Filtering;

/// <summary>
/// Marker interface implemented by all exact-match filter types.
/// </summary>
public interface IExactFilter
{
    /// <summary>
    /// Gets the name of the field the filter targets.
    /// </summary>
    string FieldName { get; }
}

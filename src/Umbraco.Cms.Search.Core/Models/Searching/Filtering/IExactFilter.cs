namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

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

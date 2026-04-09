using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

/// <summary>
/// Represents a response model containing the configuration details for a data type.
/// </summary>
public class DatatypeConfigurationResponseModel
{
    /// <summary>
    /// Gets the mode that indicates how or if the data type can be changed.
    /// </summary>
    public required DataTypeChangeMode CanBeChanged { get; init; }

    /// <summary>
    /// Gets or sets the identifier of the document list view.
    /// </summary>
    public required Guid DocumentListViewId { get; init; }

    /// <summary>
    /// Gets or sets the identifier for the media list view.
    /// </summary>
    public required Guid MediaListViewId { get; init; }
}

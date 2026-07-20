namespace Umbraco.Cms.Api.Management.ViewModels.DynamicRoot;

/// <summary>
/// Represents the request model for specifying a single query step within a dynamic root operation.
/// </summary>
public class DynamicRootQueryStepRequestModel
{
    /// <summary>
    /// Gets or sets the alias that identifies this dynamic root query step.
    /// </summary>
    public required string Alias { get; set; }

    /// <summary>
    /// Gets or sets the collection of document type IDs used to filter results in this query step.
    /// </summary>
    public required IEnumerable<Guid> DocumentTypeIds { get; set; }
}

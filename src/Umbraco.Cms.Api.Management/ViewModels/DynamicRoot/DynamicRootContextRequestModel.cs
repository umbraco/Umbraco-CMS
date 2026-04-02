namespace Umbraco.Cms.Api.Management.ViewModels.DynamicRoot;

/// <summary>
/// Represents the request model containing parameters for retrieving the dynamic root context in the Umbraco management API.
/// </summary>
public class DynamicRootContextRequestModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the dynamic root context.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Gets or sets the model representing the parent entity by its identifier.
    /// </summary>
    public required ReferenceByIdModel Parent { get; set; }

    /// <summary>
    /// Gets or sets the culture code (e.g., "en-US") used for the dynamic root context.
    /// </summary>
    public string? Culture { get; set; }

    /// <summary>
    /// Gets or sets the segment associated with the dynamic root context.
    /// </summary>
    public string? Segment { get; set; }
}

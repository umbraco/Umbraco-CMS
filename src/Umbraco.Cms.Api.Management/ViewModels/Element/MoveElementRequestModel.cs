namespace Umbraco.Cms.Api.Management.ViewModels.Element;

/// <summary>
/// Represents the request model used when moving an element to a new location via the Management API.
/// </summary>
public class MoveElementRequestModel
{
    /// <summary>
    /// Gets or sets the target location, specified by ID, to which the element should be moved.
    /// </summary>
    public ReferenceByIdModel? Target { get; set; }
}

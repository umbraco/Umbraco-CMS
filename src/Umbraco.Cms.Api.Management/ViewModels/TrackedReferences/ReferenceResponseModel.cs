namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

/// <summary>
/// Represents the response model containing information about a tracked reference in the system.
/// </summary>
public abstract class ReferenceResponseModel : IReferenceResponseModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the reference.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the reference.
    /// </summary>
    public string? Name { get; set; }
}

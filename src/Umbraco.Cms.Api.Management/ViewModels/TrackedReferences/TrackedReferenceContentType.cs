namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

/// <summary>
/// Represents a content type that is tracked as a reference in the Umbraco CMS Management API.
/// </summary>
public abstract class TrackedReferenceContentType
{
    /// <summary>
    /// Gets or sets the unique identifier of the tracked reference content type.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the icon representing the tracked reference content type.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the alias of the tracked reference content type.
    /// </summary>
    public string? Alias { get; set; }

    /// <summary>
    /// Gets or sets the name of the tracked reference content type.
    /// </summary>
    public string? Name { get; set; }
}

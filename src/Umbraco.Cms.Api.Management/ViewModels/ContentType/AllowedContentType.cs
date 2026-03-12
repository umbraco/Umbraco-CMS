namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

/// <summary>
/// Represents an allowed content type in the content type management API view model.
/// </summary>
public abstract class AllowedContentType
{
    /// <summary>
    /// Gets or sets the unique identifier of the allowed content type.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the allowed content type.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the allowed content type.
    /// </summary>
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the icon associated with the allowed content type.
    /// </summary>
    public string? Icon { get; set; }
}

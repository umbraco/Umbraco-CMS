namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

/// <summary>
/// Serves as the base class for response models representing content type compositions in the API.
/// </summary>
public abstract class ContentTypeCompositionResponseModelBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the content type composition.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the content type composition.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the icon representing the content type composition.
    /// </summary>
    public string Icon { get; set; } = string.Empty;
}

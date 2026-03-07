namespace Umbraco.Cms.Api.Management.ViewModels.Media;

/// <summary>
/// Represents a response model containing configuration settings for media in the Umbraco CMS Management API.
/// </summary>
public class MediaConfigurationResponseModel
{
    /// <summary>Gets or sets a value indicating whether deleting media is disabled when it is referenced.</summary>
    public required bool DisableDeleteWhenReferenced { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether unpublishing is disabled when the media item is referenced.
    /// </summary>
    public required bool DisableUnpublishWhenReferenced { get; set; }
}

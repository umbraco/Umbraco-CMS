using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;

namespace Umbraco.Cms.Api.Management.ViewModels.Media;

/// <summary>
/// Represents a response model for a media entity in the API management layer.
/// </summary>
public class MediaResponseModel : ContentResponseModelBase<MediaValueResponseModel, MediaVariantResponseModel>
{
    /// <summary>
    /// Gets or sets a value indicating whether the media item is trashed.
    /// </summary>
    public bool IsTrashed { get; set; }

    /// <summary>
    /// Gets or sets the media type reference associated with this media item.
    /// </summary>
    public MediaTypeReferenceResponseModel MediaType { get; set; } = new();
}

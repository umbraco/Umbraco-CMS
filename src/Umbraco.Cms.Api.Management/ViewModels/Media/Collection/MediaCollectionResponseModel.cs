using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;

namespace Umbraco.Cms.Api.Management.ViewModels.Media.Collection;

/// <summary>
/// Represents the response model containing information about a collection of media items.
/// </summary>
public class MediaCollectionResponseModel : ContentCollectionResponseModelBase<MediaValueResponseModel, MediaVariantResponseModel>
{
    /// <summary>
    /// Gets or sets a reference to the media type associated with this media collection.
    /// </summary>
    public MediaTypeCollectionReferenceResponseModel MediaType { get; set; } = new();
}

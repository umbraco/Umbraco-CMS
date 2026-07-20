using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;

namespace Umbraco.Cms.Api.Management.ViewModels.Media.Item;

/// <summary>
/// Represents a response model containing details of a media item returned by the Management API.
/// </summary>
public class MediaItemResponseModel : ItemResponseModelBase
{
    /// <summary>
    /// Gets or sets a value indicating whether the media item is trashed.
    /// </summary>
    public bool IsTrashed { get; set; }

    /// <summary>
    /// Gets or sets a reference to the parent media item, if any.
    /// </summary>
    public ReferenceByIdModel? Parent { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this media item has any child items.
    /// </summary>
    public bool HasChildren { get; set; }

    /// <summary>
    /// Gets or sets the reference to the media type of this media item.
    /// </summary>
    public MediaTypeReferenceResponseModel MediaType { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of language or culture variants for the media item.
    /// </summary>
    public IEnumerable<VariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<VariantItemResponseModel>();
}

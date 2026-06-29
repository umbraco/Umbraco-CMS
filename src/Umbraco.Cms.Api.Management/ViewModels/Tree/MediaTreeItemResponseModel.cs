using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;

namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

/// <summary>
/// Represents a single media item within the tree view response model in the Umbraco management API.
/// </summary>
public class MediaTreeItemResponseModel : ContentTreeItemResponseModel
{
    /// <summary>
    /// Gets or sets the reference to the media type for this media tree item.
    /// </summary>
    public MediaTypeReferenceResponseModel MediaType { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of variant items for the media tree item.
    /// </summary>
    public IEnumerable<VariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<VariantItemResponseModel>();
}

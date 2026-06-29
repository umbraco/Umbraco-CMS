using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Api.Management.ViewModels.RecycleBin;

namespace Umbraco.Cms.Api.Management.ViewModels.Media.RecycleBin;

/// <summary>
/// Represents an item in the media recycle bin response model.
/// </summary>
public class MediaRecycleBinItemResponseModel : RecycleBinItemResponseModelBase
{
    /// <summary>
    /// Gets or sets the media type reference associated with the recycle bin item.
    /// </summary>
    public MediaTypeReferenceResponseModel MediaType { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of variant items associated with the media recycle bin item.
    /// </summary>
    public IEnumerable<VariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<VariantItemResponseModel>();
}

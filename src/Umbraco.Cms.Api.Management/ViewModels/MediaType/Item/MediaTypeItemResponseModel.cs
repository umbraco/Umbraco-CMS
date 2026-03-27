using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.MediaType.Item;

/// <summary>
/// Represents a response model containing information about a media type item in the management API.
/// </summary>
public class MediaTypeItemResponseModel : NamedItemResponseModelBase
{
    /// <summary>Gets or sets the icon associated with the media type.</summary>
    public string? Icon { get; set; }
}

using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;

namespace Umbraco.Cms.Api.Management.ViewModels.Media.Item;

public class MediaItemResponseModel : NamedItemResponseModelBase
{
    public MediaTypeReferenceResponseModel MediaType { get; set; } = new();

    public string? Icon { get; set; }

    public bool IsTrashed { get; set; }
}

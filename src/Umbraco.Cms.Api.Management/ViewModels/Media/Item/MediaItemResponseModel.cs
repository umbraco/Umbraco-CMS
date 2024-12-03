using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;

namespace Umbraco.Cms.Api.Management.ViewModels.Media.Item;

public class MediaItemResponseModel : ItemResponseModelBase
{
    public bool IsTrashed { get; set; }

    public ReferenceByIdModel? Parent { get; set; }

    public bool HasChildren { get; set; }

    public MediaTypeReferenceResponseModel MediaType { get; set; } = new();

    public IEnumerable<VariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<VariantItemResponseModel>();
}

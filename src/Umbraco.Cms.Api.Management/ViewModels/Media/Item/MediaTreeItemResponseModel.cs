using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.ViewModels.Media.Item;

public class MediaTreeItemResponseModel : ContentTreeItemResponseModel
{
    public MediaTypeReferenceResponseModel MediaType { get; set; } = new();

    public IEnumerable<VariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<VariantItemResponseModel>();
}

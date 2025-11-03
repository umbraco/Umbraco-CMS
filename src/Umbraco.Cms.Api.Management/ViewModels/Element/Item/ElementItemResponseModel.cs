using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.Element.Item;

public class ElementItemResponseModel : ItemResponseModelBase
{
    public ReferenceByIdModel? Parent { get; set; }

    public bool HasChildren { get; set; }

    public DocumentTypeReferenceResponseModel DocumentType { get; set; } = new();

    public IEnumerable<ElementVariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<ElementVariantItemResponseModel>();
}

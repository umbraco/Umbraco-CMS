using Umbraco.Cms.Api.Management.ViewModels.Element;

namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

public class ElementReferenceResponseModel : ReferenceResponseModel
{
    public bool? Published { get; set; }

    public TrackedReferenceDocumentType DocumentType { get; set; } = new();

    public IEnumerable<ElementVariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<ElementVariantItemResponseModel>();
}
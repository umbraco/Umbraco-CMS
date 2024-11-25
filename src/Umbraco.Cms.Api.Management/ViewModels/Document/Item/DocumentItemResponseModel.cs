using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Item;

namespace Umbraco.Cms.Api.Management.ViewModels.Document.Item;

public class DocumentItemResponseModel : ItemResponseModelBase
{
    public bool IsTrashed { get; set; }

    public bool IsProtected { get; set; }

    public ReferenceByIdModel? Parent { get; set; }

    public bool HasChildren { get; set; }

    public DocumentTypeReferenceResponseModel DocumentType { get; set; } = new();

    public IEnumerable<DocumentVariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<DocumentVariantItemResponseModel>();
}

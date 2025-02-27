using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;

namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class DocumentTreeItemResponseModel : ContentTreeItemResponseModel
{
    public bool IsProtected { get; set; }

    public DocumentTypeReferenceResponseModel DocumentType { get; set; } = new();

    public IEnumerable<DocumentVariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<DocumentVariantItemResponseModel>();
}

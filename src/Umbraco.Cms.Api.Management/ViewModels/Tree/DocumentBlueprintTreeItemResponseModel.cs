using Umbraco.Cms.Api.Management.ViewModels.DocumentType;

namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class DocumentBlueprintTreeItemResponseModel : NamedEntityTreeItemResponseModel
{
    public DocumentTypeReferenceResponseModel DocumentType { get; set; } = new();
}

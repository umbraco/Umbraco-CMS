using Umbraco.Cms.Api.Management.ViewModels.DocumentType;

namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class DocumentBlueprintTreeItemResponseModel : FolderTreeItemResponseModel
{
    public DocumentTypeReferenceResponseModel? DocumentType { get; set; }
}

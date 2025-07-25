using Umbraco.Cms.Api.Management.ViewModels.DocumentType;

namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class ElementTreeItemResponseModel : FolderTreeItemResponseModel
{
    public DocumentTypeReferenceResponseModel? ElementType { get; set; }
}

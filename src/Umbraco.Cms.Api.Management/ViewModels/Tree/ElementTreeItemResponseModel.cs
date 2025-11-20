using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Element;

namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class ElementTreeItemResponseModel : FolderTreeItemResponseModel
{
    public DocumentTypeReferenceResponseModel? DocumentType { get; set; }

    public IEnumerable<ElementVariantItemResponseModel> Variants { get; set; } = [];
}

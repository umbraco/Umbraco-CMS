namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class DocumentTypeTreeItemResponseModel : FolderTreeItemResponseModel
{
    public bool IsElement { get; set; }

    public string Icon { get; set; } = string.Empty;
}

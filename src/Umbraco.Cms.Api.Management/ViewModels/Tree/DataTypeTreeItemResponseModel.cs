namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class DataTypeTreeItemResponseModel : FolderTreeItemResponseModel
{
    public string? EditorUiAlias { get; set; }

    public bool IsDeletable { get; set; }
}

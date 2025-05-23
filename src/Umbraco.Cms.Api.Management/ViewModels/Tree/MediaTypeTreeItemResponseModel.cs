namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class MediaTypeTreeItemResponseModel : FolderTreeItemResponseModel
{
    public string Icon { get; set; } = string.Empty;

    public bool IsDeletable { get; set; }
}

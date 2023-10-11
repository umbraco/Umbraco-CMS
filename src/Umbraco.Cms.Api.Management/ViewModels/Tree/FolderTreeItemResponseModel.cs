namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public abstract class FolderTreeItemResponseModel : EntityTreeItemResponseModel
{
    public bool IsFolder { get; set; }
}

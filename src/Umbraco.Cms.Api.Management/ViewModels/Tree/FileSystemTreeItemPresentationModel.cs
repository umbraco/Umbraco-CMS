namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class FileSystemTreeItemPresentationModel : TreeItemPresentationModel
{
    public string Path { get; set; } = string.Empty;

    public bool IsFolder { get; set; }
}

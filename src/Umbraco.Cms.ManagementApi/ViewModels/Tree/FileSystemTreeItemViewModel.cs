namespace Umbraco.Cms.ManagementApi.ViewModels.Tree;

public class FileSystemTreeItemViewModel : TreeItemViewModel
{
    public string Path { get; set; } = string.Empty;

    public bool IsFolder { get; set; }
}

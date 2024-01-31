using Umbraco.Cms.Api.Management.ViewModels.FileSystem;

namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class FileSystemTreeItemPresentationModel : TreeItemPresentationModel
{
    public string Name { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public FileSystemFolderModel? Parent { get; set; }

    public bool IsFolder { get; set; }
}

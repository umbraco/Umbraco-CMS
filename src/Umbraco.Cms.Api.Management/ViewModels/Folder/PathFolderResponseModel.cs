namespace Umbraco.Cms.Api.Management.ViewModels.Folder;

public class PathFolderResponseModel : FolderModelBase
{
    public string? ParentPath { get; set; }

    public string Path =>
        string.IsNullOrEmpty(ParentPath)
            ? Name
            : System.IO.Path.Combine(ParentPath, Name);
}

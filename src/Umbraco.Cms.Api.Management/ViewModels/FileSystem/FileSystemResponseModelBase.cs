namespace Umbraco.Cms.Api.Management.ViewModels.FileSystem;

public abstract class FileSystemResponseModelBase : FileSystemItemViewModelBase
{
    public required string Name { get; set; }

    public FileSystemFolderModel? Parent { get; set; }
}

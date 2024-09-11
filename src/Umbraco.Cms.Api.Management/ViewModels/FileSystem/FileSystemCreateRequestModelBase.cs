namespace Umbraco.Cms.Api.Management.ViewModels.FileSystem;

public abstract class FileSystemCreateRequestModelBase
{
    public required string Name { get; set; }

    public FileSystemFolderModel? Parent { get; set; }
}

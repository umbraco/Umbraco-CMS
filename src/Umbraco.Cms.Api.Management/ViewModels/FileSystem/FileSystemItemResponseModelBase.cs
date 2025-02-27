namespace Umbraco.Cms.Api.Management.ViewModels.FileSystem;

public abstract class FileSystemItemResponseModelBase : FileSystemResponseModelBase
{
    public required bool IsFolder { get; set; }
}

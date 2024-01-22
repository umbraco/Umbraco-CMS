namespace Umbraco.Cms.Api.Management.ViewModels.FileSystem;

public abstract class FileSystemFileCreateRequestModelBase : FileSystemCreateRequestModelBase
{
    public required string Content { get; set; }
}

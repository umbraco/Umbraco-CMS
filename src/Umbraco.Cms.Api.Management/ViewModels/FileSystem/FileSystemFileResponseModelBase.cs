namespace Umbraco.Cms.Api.Management.ViewModels.FileSystem;

public abstract class FileSystemFileResponseModelBase : FileSystemResponseModelBase
{
    public required string Content { get; set; }
}

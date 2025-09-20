using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.StaticFile.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/static-file")]
[ApiExplorerSettings(GroupName = "Static File")]
public class StaticFileTreeControllerBase : FileSystemTreeControllerBase
{
    private readonly IFileSystemTreeService _fileSystemTreeService;
    private static readonly string[] _allowedRootFolders = { $"{Path.DirectorySeparatorChar}App_Plugins", $"{Path.DirectorySeparatorChar}wwwroot" };

    public StaticFileTreeControllerBase(IPhysicalFileSystem physicalFileSystem, IFileSystemTreeService fileSystemTreeService)
        : base (fileSystemTreeService)
    {
        FileSystem = physicalFileSystem;
        _fileSystemTreeService = fileSystemTreeService;
    }

    [Obsolete("Please use the other constructor. Scheduled for removal in Umbraco 19")]
    public StaticFileTreeControllerBase(IPhysicalFileSystem physicalFileSystem)
        : this(physicalFileSystem, StaticServiceProvider.Instance.GetRequiredService<IFileSystemTreeService>())
    {
    }

    protected override IFileSystem FileSystem { get; }

    protected string[] GetDirectories(string path) =>
        IsTreeRootPath(path)
            ? _allowedRootFolders
            : IsAllowedPath(path)
                ? _fileSystemTreeService.GetDirectories(path)
                : Array.Empty<string>();

    protected string[] GetFiles(string path)
        => IsTreeRootPath(path) || IsAllowedPath(path) == false
            ? Array.Empty<string>()
            : _fileSystemTreeService.GetFiles(path);

    protected FileSystemTreeItemPresentationModel[] GetAncestorModels(string path, bool includeSelf)
        => IsAllowedPath(path)
            ? _fileSystemTreeService.GetAncestorModels(path, includeSelf)
            : Array.Empty<FileSystemTreeItemPresentationModel>();

    private bool IsTreeRootPath(string path) => path == Path.DirectorySeparatorChar.ToString();

    private bool IsAllowedPath(string path) => _allowedRootFolders.Contains(path) || _allowedRootFolders.Any(folder => path.StartsWith($"{folder}{Path.DirectorySeparatorChar}"));
}

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

    /// <summary>
    /// Serves as the base controller for handling operations related to static file trees in the Umbraco CMS Management API.
    /// Provides common functionality for derived controllers managing static files.
    /// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/static-file")]
[ApiExplorerSettings(GroupName = "Static File")]
public class StaticFileTreeControllerBase : FileSystemTreeControllerBase
{
    private readonly IFileSystemTreeService _fileSystemTreeService;
    private static readonly string[] _allowedRootFolders = { $"{Path.DirectorySeparatorChar}App_Plugins", $"{Path.DirectorySeparatorChar}wwwroot" };

    /// <summary>
    /// Initializes a new instance of the <see cref="StaticFileTreeControllerBase"/> class.
    /// </summary>
    /// <param name="physicalFileSystem">An injected <see cref="IPhysicalFileSystem"/> instance representing the physical file system to be used by the controller.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public StaticFileTreeControllerBase(IPhysicalFileSystem physicalFileSystem)
        : base(StaticServiceProvider.Instance.GetRequiredService<IPhysicalFileSystemTreeService>())
    {
        FileSystem = physicalFileSystem;
        _fileSystemTreeService = StaticServiceProvider.Instance.GetRequiredService<IPhysicalFileSystemTreeService>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StaticFileTreeControllerBase"/> class with the specified file system and tree service.
    /// </summary>
    /// <param name="physicalFileSystem">The physical file system used for file operations.</param>
    /// <param name="fileSystemTreeService">The service used to manage the file system tree structure.</param>
    public StaticFileTreeControllerBase(IPhysicalFileSystem physicalFileSystem, IPhysicalFileSystemTreeService fileSystemTreeService)
        : base (fileSystemTreeService)
    {
        FileSystem = physicalFileSystem;
        _fileSystemTreeService = fileSystemTreeService;
    }

    protected override IFileSystem FileSystem { get; }

    protected override string[] GetDirectories(string path) =>
        IsTreeRootPath(path)
            ? _allowedRootFolders
            : IsAllowedPath(path)
                ? _fileSystemTreeService.GetDirectories(path)
                : Array.Empty<string>();

    protected override string[] GetFiles(string path)
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

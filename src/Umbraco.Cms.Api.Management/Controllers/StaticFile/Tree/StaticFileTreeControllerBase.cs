using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.StaticFile.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/static-file")]
[ApiExplorerSettings(GroupName = "Static File")]
public class StaticFileTreeControllerBase : FileSystemTreeControllerBase
{
    private static readonly string[] _allowedRootFolders = { $"{Path.DirectorySeparatorChar}App_Plugins", $"{Path.DirectorySeparatorChar}wwwroot" };

    public StaticFileTreeControllerBase(IPhysicalFileSystem physicalFileSystem)
        => FileSystem = physicalFileSystem;

    protected override IFileSystem FileSystem { get; }

    protected override string[] GetDirectories(string path) =>
        IsTreeRootPath(path)
            ? _allowedRootFolders
            : IsAllowedPath(path)
                ? base.GetDirectories(path)
                : Array.Empty<string>();

    protected override string[] GetFiles(string path)
        => IsTreeRootPath(path) || IsAllowedPath(path) == false
            ? Array.Empty<string>()
            : base.GetFiles(path);

    protected override FileSystemTreeItemPresentationModel[] GetAncestorModels(string path, bool includeSelf)
        => IsAllowedPath(path)
            ? base.GetAncestorModels(path, includeSelf)
            : Array.Empty<FileSystemTreeItemPresentationModel>();

    private bool IsTreeRootPath(string path) => path == Path.DirectorySeparatorChar.ToString();

    private bool IsAllowedPath(string path) => _allowedRootFolders.Contains(path) || _allowedRootFolders.Any(folder => path.StartsWith($"{folder}{Path.DirectorySeparatorChar}"));
}

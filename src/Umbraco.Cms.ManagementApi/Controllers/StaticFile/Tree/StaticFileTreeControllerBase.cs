using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.ManagementApi.Controllers.Tree;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.StaticFile.Tree;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute("static-file/tree")]
[OpenApiTag("StaticFile")]
public class StaticFileTreeControllerBase : FileSystemTreeControllerBase
{
    private static readonly string[] _allowedRootFolders = { "App_Plugins", "wwwroot" };

    public StaticFileTreeControllerBase(IPhysicalFileSystem physicalFileSystem)
        => FileSystem = physicalFileSystem;

    protected override IFileSystem FileSystem { get; }

    protected override string FileIcon(string path) => Constants.Icons.DefaultIcon;

    protected override string ItemType(string path) => "static-file";

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

    private bool IsTreeRootPath(string path) => string.IsNullOrWhiteSpace(path);

    private bool IsAllowedPath(string path) => _allowedRootFolders.Contains(path) || _allowedRootFolders.Any(folder => path.StartsWith($"{folder}/"));
}

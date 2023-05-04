using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Tree;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Stylesheet}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Stylesheet))]
public class StylesheetTreeControllerBase : FileSystemTreeControllerBase
{
    public StylesheetTreeControllerBase(FileSystems fileSystems)
        => FileSystem = fileSystems.StylesheetsFileSystem ??
                        throw new ArgumentException("Missing stylesheets file system", nameof(fileSystems));

    protected override IFileSystem FileSystem { get; }

    protected override string FileIcon(string path) => Constants.Icons.Stylesheet;

    protected override string ItemType(string path) => Constants.UdiEntityType.Stylesheet;
}

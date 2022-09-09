using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.ManagementApi.Controllers.Tree;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Stylesheet.Tree;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Stylesheet}/tree")]
[OpenApiTag(nameof(Constants.UdiEntityType.Stylesheet))]
public class StylesheetTreeControllerBase : FileSystemTreeControllerBase
{
    public StylesheetTreeControllerBase(FileSystems fileSystems)
        => FileSystem = fileSystems.StylesheetsFileSystem ??
                        throw new ArgumentException("Missing stylesheets file system", nameof(fileSystems));

    protected override IFileSystem FileSystem { get; }

    protected override string FileIcon(string path) => Constants.Icons.Stylesheet;

    protected override string ItemType(string path) => Constants.UdiEntityType.Stylesheet;
}

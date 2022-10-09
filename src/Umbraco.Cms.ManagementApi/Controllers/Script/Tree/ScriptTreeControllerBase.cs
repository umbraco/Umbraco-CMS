using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.ManagementApi.Controllers.Tree;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Script.Tree;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Script}/tree")]
[OpenApiTag(nameof(Constants.UdiEntityType.Script))]
public class ScriptTreeControllerBase : FileSystemTreeControllerBase
{
    public ScriptTreeControllerBase(FileSystems fileSystems)
        => FileSystem = fileSystems.ScriptsFileSystem ??
                        throw new ArgumentException("Missing scripts file system", nameof(fileSystems));

    protected override IFileSystem FileSystem { get; }

    protected override string FileIcon(string path) => Constants.Icons.Script;

    protected override string ItemType(string path) => Constants.UdiEntityType.Script;
}

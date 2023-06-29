using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Tree;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Script}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Script))]
public class ScriptTreeControllerBase : FileSystemTreeControllerBase
{
    public ScriptTreeControllerBase(FileSystems fileSystems)
        => FileSystem = fileSystems.ScriptsFileSystem ??
                        throw new ArgumentException("Missing scripts file system", nameof(fileSystems));

    protected override IFileSystem FileSystem { get; }

    protected override string ItemType(string path) => Constants.UdiEntityType.Script;
}

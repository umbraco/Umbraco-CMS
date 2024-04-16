using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Script}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Script))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessScripts)]
public class ScriptTreeControllerBase : FileSystemTreeControllerBase
{
    public ScriptTreeControllerBase(FileSystems fileSystems)
        => FileSystem = fileSystems.ScriptsFileSystem ??
                        throw new ArgumentException("Missing scripts file system", nameof(fileSystems));

    protected override IFileSystem FileSystem { get; }
}

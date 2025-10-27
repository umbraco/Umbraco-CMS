using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Script}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Script))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessScripts)]
public class ScriptTreeControllerBase : FileSystemTreeControllerBase
{
    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    public ScriptTreeControllerBase(IScriptTreeService scriptTreeService)
        : base(scriptTreeService)
    {
        FileSystem = null!;
    }

    // FileSystem is required therefore, we can't remove it without some wizardry. When obsoletion is due, remove this.
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public ScriptTreeControllerBase(IScriptTreeService scriptTreeService, FileSystems fileSystems)
        : base(scriptTreeService)
    {
        FileSystem = fileSystems.ScriptsFileSystem ??
                     throw new ArgumentException("Missing scripts file system", nameof(fileSystems));
    }

    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public ScriptTreeControllerBase(FileSystems fileSystems)
        : base()
        => FileSystem = fileSystems.ScriptsFileSystem ??
                        throw new ArgumentException("Missing scripts file system", nameof(fileSystems));

    [Obsolete("Included in the service class. Scheduled to be removed in Umbraco 19.")]
    protected override IFileSystem FileSystem { get; }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Script}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Script))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessScripts)]
public class ScriptTreeControllerBase : FileSystemTreeControllerBase
{
    private readonly IScriptTreeService _scriptTreeService;

    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    public ScriptTreeControllerBase(IScriptTreeService scriptTreeService)
        : this(scriptTreeService, StaticServiceProvider.Instance.GetRequiredService<FileSystems>()) =>
        _scriptTreeService = scriptTreeService;

    // FileSystem is required therefore, we can't remove it without some wizadry. When obsoletion is due, remove this.
    [ActivatorUtilitiesConstructor]
    [Obsolete("Scheduled for removal in Umbraco 18.")]
    public ScriptTreeControllerBase(IScriptTreeService scriptTreeService, FileSystems fileSystems)
        : base(scriptTreeService)
    {
        _scriptTreeService = scriptTreeService;
        FileSystem = fileSystems.ScriptsFileSystem ??
                     throw new ArgumentException("Missing scripts file system", nameof(fileSystems));
    }

    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 18.")]
    public ScriptTreeControllerBase(FileSystems fileSystems)
        : this(StaticServiceProvider.Instance.GetRequiredService<IScriptTreeService>())
        => FileSystem = fileSystems.ScriptsFileSystem ??
                        throw new ArgumentException("Missing scripts file system", nameof(fileSystems));

    [Obsolete("Included in the service class. Scheduled to be removed in Umbraco 18.")]
    protected override IFileSystem FileSystem { get; }
}

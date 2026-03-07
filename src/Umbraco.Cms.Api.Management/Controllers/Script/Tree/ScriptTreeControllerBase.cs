using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Tree;

    /// <summary>
    /// Serves as the base controller for script tree management in the Umbraco CMS API.
    /// Provides shared functionality for derived script tree controllers.
    /// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Script}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Script))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessScripts)]
public class ScriptTreeControllerBase : FileSystemTreeControllerBase
{
    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptTreeControllerBase"/> class.
    /// </summary>
    /// <param name="scriptTreeService">An instance of <see cref="IScriptTreeService"/> used to manage script tree operations.</param>
    public ScriptTreeControllerBase(IScriptTreeService scriptTreeService)
        : base(scriptTreeService)
    {
        FileSystem = null!;
    }

    // FileSystem is required therefore, we can't remove it without some wizardry. When obsoletion is due, remove this.
    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptTreeControllerBase"/> class.
    /// </summary>
    /// <param name="scriptTreeService">Service used to manage and retrieve script tree data.</param>
    /// <param name="fileSystems">Provides access to the file systems used for script storage and retrieval.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public ScriptTreeControllerBase(IScriptTreeService scriptTreeService, FileSystems fileSystems)
        : base(scriptTreeService)
    {
        FileSystem = fileSystems.ScriptsFileSystem ??
                     throw new ArgumentException("Missing scripts file system", nameof(fileSystems));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptTreeControllerBase"/> class.
    /// </summary>
    /// <param name="fileSystems">
    /// The <see cref="FileSystems"/> instance that provides access to the file systems used by the controller.
    /// </param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public ScriptTreeControllerBase(FileSystems fileSystems)
        : base()
        => FileSystem = fileSystems.ScriptsFileSystem ??
                        throw new ArgumentException("Missing scripts file system", nameof(fileSystems));

    [Obsolete("Included in the service class. Scheduled to be removed in Umbraco 19.")]
    protected override IFileSystem FileSystem { get; }
}

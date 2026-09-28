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
    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptTreeControllerBase"/> class.
    /// </summary>
    /// <param name="scriptTreeService">An instance of <see cref="IScriptTreeService"/> used to manage script tree operations.</param>
    public ScriptTreeControllerBase(IScriptTreeService scriptTreeService)
        : base(scriptTreeService)
    {
    }
}

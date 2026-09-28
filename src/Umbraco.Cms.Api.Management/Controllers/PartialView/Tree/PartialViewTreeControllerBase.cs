using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Tree;

/// <summary>
/// Serves as the base controller for managing partial view trees in the Umbraco CMS API.
/// Provides shared functionality for partial view tree operations.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.PartialView}")]
[ApiExplorerSettings(GroupName = "Partial View")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessPartialViews)]
public class PartialViewTreeControllerBase : FileSystemTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PartialViewTreeControllerBase"/> class with the specified partial view tree service.
    /// </summary>
    /// <param name="partialViewTreeService">An instance of <see cref="IPartialViewTreeService"/> used to manage partial view trees.</param>
    public PartialViewTreeControllerBase(IPartialViewTreeService partialViewTreeService)
        : base(partialViewTreeService)
    {
    }
}

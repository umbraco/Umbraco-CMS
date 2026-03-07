using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
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
    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    /// <summary>
    /// Initializes a new instance of the <see cref="PartialViewTreeControllerBase"/> class with the specified partial view tree service.
    /// </summary>
    /// <param name="partialViewTreeService">An instance of <see cref="IPartialViewTreeService"/> used to manage partial view trees.</param>
    public PartialViewTreeControllerBase(IPartialViewTreeService partialViewTreeService)
        : base(partialViewTreeService)
    {
        FileSystem = null!;
    }

    // FileSystem is required therefore, we can't remove it without some wizardry. When obsoletion is due, remove this.
    /// <summary>
    /// Initializes a new instance of the <see cref="PartialViewTreeControllerBase"/> class with the specified services.
    /// </summary>
    /// <param name="partialViewTreeService">The service used to manage and retrieve partial view tree structures.</param>
    /// <param name="fileSystems">The abstraction for accessing and managing file systems related to partial views.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public PartialViewTreeControllerBase(IPartialViewTreeService partialViewTreeService, FileSystems fileSystems)
        : base(partialViewTreeService)
    {
        FileSystem = fileSystems.PartialViewsFileSystem ??
                     throw new ArgumentException("Missing scripts file system", nameof(fileSystems));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PartialViewTreeControllerBase"/> class.
    /// </summary>
    /// <param name="fileSystems">The <see cref="FileSystems"/> instance to be used by the controller.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public PartialViewTreeControllerBase(FileSystems fileSystems)
        : base()
        => FileSystem = fileSystems.PartialViewsFileSystem ??
                        throw new ArgumentException("Missing scripts file system", nameof(fileSystems));

    [Obsolete("Included in the service class. Scheduled to be removed in Umbraco 19.")]
    protected override IFileSystem FileSystem { get; }
}

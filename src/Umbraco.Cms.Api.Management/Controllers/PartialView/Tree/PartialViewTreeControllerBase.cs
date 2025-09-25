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

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.PartialView}")]
[ApiExplorerSettings(GroupName = "Partial View")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessPartialViews)]
public class PartialViewTreeControllerBase : FileSystemTreeControllerBase
{
    private readonly IPartialViewTreeService _partialViewTreeService;

    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    public PartialViewTreeControllerBase(IPartialViewTreeService partialViewTreeService)
        : this(partialViewTreeService, StaticServiceProvider.Instance.GetRequiredService<FileSystems>()) =>
        _partialViewTreeService = partialViewTreeService;

    // FileSystem is required therefore, we can't remove it without some wizadry. When obsoletion is due, remove this.
    [ActivatorUtilitiesConstructor]
    [Obsolete("Scheduled for removal in Umbraco 18.")]
    public PartialViewTreeControllerBase(IPartialViewTreeService partialViewTreeService, FileSystems fileSystems)
        : base(partialViewTreeService)
    {
        _partialViewTreeService = partialViewTreeService;
        FileSystem = fileSystems.PartialViewsFileSystem ??
                     throw new ArgumentException("Missing scripts file system", nameof(fileSystems));
    }

    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 18.")]
    public PartialViewTreeControllerBase(FileSystems fileSystems)
        : this(StaticServiceProvider.Instance.GetRequiredService<IPartialViewTreeService>())
        => FileSystem = fileSystems.PartialViewsFileSystem ??
                        throw new ArgumentException("Missing scripts file system", nameof(fileSystems));

    [Obsolete("Included in the service class. Scheduled to be removed in Umbraco 18.")]
    protected override IFileSystem FileSystem { get; }
}

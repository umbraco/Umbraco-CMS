using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Tree;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.PartialView}")]
[ApiExplorerSettings(GroupName = "Partial View")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessPartialViews)]
public class PartialViewTreeControllerBase : FileSystemTreeControllerBase
{
    public PartialViewTreeControllerBase(FileSystems fileSystems)
        => FileSystem = fileSystems.PartialViewsFileSystem ??
                        throw new ArgumentException("Missing partial views file system", nameof(fileSystems));

    protected override IFileSystem FileSystem { get; }

    protected override string ItemType(string path) => Constants.UdiEntityType.PartialView;
}

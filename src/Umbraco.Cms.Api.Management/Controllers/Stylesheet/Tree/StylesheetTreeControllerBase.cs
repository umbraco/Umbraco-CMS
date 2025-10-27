using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Stylesheet}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Stylesheet))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessStylesheets)]
public class StylesheetTreeControllerBase : FileSystemTreeControllerBase
{
    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    public StylesheetTreeControllerBase(IStyleSheetTreeService styleSheetTreeService)
        : base(styleSheetTreeService)
    {
        FileSystem = null!;
    }

    // FileSystem is required therefore, we can't remove it without some wizadry. When obsoletion is due, remove this.
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public StylesheetTreeControllerBase(IStyleSheetTreeService styleSheetTreeService, FileSystems fileSystems)
        : base(styleSheetTreeService)
    {
        FileSystem = fileSystems.ScriptsFileSystem ??
                     throw new ArgumentException("Missing scripts file system", nameof(fileSystems));
    }

    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public StylesheetTreeControllerBase(FileSystems fileSystems)
        : base()
        => FileSystem = fileSystems.ScriptsFileSystem ??
                        throw new ArgumentException("Missing scripts file system", nameof(fileSystems));

    [Obsolete("Included in the service class. Scheduled to be removed in Umbraco 19.")]
    protected override IFileSystem FileSystem { get; }
}

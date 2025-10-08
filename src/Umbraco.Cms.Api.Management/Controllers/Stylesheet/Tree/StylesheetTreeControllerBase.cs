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

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Stylesheet}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Stylesheet))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessStylesheets)]
public class StylesheetTreeControllerBase : FileSystemTreeControllerBase
{
    private readonly IStyleSheetTreeService _styleSheetTreeService;

    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    public StylesheetTreeControllerBase(IStyleSheetTreeService styleSheetTreeService)
        : this(styleSheetTreeService, StaticServiceProvider.Instance.GetRequiredService<FileSystems>()) =>
        _styleSheetTreeService = styleSheetTreeService;

    // FileSystem is required therefore, we can't remove it without some wizadry. When obsoletion is due, remove this.
    [ActivatorUtilitiesConstructor]
    [Obsolete("Scheduled for removal in Umbraco 18.")]
    public StylesheetTreeControllerBase(IStyleSheetTreeService styleSheetTreeService, FileSystems fileSystems)
        : base(styleSheetTreeService)
    {
        _styleSheetTreeService = styleSheetTreeService;
        FileSystem = fileSystems.ScriptsFileSystem ??
                     throw new ArgumentException("Missing scripts file system", nameof(fileSystems));
    }

    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 18.")]
    public StylesheetTreeControllerBase(FileSystems fileSystems)
        : this(StaticServiceProvider.Instance.GetRequiredService<IStyleSheetTreeService>())
        => FileSystem = fileSystems.ScriptsFileSystem ??
                        throw new ArgumentException("Missing scripts file system", nameof(fileSystems));

    [Obsolete("Included in the service class. Scheduled to be removed in Umbraco 18.")]
    protected override IFileSystem FileSystem { get; }
}

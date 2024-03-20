using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Stylesheet}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Stylesheet))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessStylesheets)]
public class StylesheetTreeControllerBase : FileSystemTreeControllerBase
{
    public StylesheetTreeControllerBase(FileSystems fileSystems)
        => FileSystem = fileSystems.StylesheetsFileSystem ??
                        throw new ArgumentException("Missing stylesheets file system", nameof(fileSystems));

    protected override IFileSystem FileSystem { get; }
}

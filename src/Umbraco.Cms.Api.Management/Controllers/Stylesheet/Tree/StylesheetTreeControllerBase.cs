using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Tree;

/// <summary>
/// Serves as the base controller for handling operations related to stylesheet trees within the Umbraco CMS Management API.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.Stylesheet}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Stylesheet))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessStylesheets)]
public class StylesheetTreeControllerBase : FileSystemTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StylesheetTreeControllerBase"/> class with the specified stylesheet tree service.
    /// </summary>
    /// <param name="styleSheetTreeService">An instance of <see cref="IStyleSheetTreeService"/> used to manage stylesheet trees.</param>
    public StylesheetTreeControllerBase(IStyleSheetTreeService styleSheetTreeService)
        : base(styleSheetTreeService)
    {
    }

    // FileSystem is required therefore, we can't remove it without some wizadry. When obsoletion is due, remove this.
}

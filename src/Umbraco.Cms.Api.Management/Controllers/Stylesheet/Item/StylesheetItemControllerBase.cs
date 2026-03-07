using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Item;

    /// <summary>
    /// Abstract base controller for managing stylesheet items in the Umbraco CMS Management API.
    /// Intended to be inherited by controllers that implement specific stylesheet item operations.
    /// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.Stylesheet}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Stylesheet))]
public class StylesheetItemControllerBase : ManagementApiControllerBase
{
}

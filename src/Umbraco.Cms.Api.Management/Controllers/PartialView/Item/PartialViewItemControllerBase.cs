using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Item;

/// <summary>
/// Serves as the base controller for operations related to partial view items within the Umbraco CMS Management API.
/// Provides common functionality for derived controllers handling partial view item management.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.PartialView}")]
[ApiExplorerSettings(GroupName = "Partial View")]
public class PartialViewItemControllerBase : ManagementApiControllerBase
{
}

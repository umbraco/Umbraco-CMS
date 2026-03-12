using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.User.Item;

/// <summary>
/// Serves as the base controller for user item-related operations in the Umbraco CMS Management API.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/user")]
[ApiExplorerSettings(GroupName = "User")]
public class UserItemControllerBase : ManagementApiControllerBase
{
}

using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup.Item;

/// <summary>
/// Serves as the base controller for managing user group items in the Umbraco CMS Management API.
/// Provides common functionality for derived user group item controllers.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/user-group")]
[ApiExplorerSettings(GroupName = "User Group")]
public class UserGroupItemControllerBase : ManagementApiControllerBase
{
}

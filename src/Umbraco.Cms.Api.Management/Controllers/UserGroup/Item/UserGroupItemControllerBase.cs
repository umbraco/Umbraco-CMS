using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup.Item;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/user-group")]
[ApiExplorerSettings(GroupName = "User Group")]
public class UserGroupItemControllerBase : ManagementApiControllerBase
{
}

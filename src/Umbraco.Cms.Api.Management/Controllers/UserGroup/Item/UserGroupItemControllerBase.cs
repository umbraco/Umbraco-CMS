using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup.Item;

[ApiController]
[VersionedApiBackOfficeRoute("user-groups")]
[ApiExplorerSettings(GroupName = "User Groups")]
[ApiVersion("1.0")]
public class UserGroupItemControllerBase : ManagementApiControllerBase
{
}

using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroups;

// TODO: This needs to be an authorized controller.

[ApiController]
[VersionedApiBackOfficeRoute("user-groups")]
[ApiExplorerSettings(GroupName = "User Groups")]
[ApiVersion("1.0")]
public class UserGroupsControllerBase : ManagementApiControllerBase
{

}

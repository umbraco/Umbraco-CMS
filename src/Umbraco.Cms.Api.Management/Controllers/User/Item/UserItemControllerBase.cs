using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.User.Item;

[ApiController]
[VersionedApiBackOfficeRoute("user")]
[ApiExplorerSettings(GroupName = "User")]
public class UserItemControllerBase : ManagementApiControllerBase
{
}

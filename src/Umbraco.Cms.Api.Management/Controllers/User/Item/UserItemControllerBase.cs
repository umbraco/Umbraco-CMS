using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.User.Item;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/user")]
[ApiExplorerSettings(GroupName = "User")]
public class UserItemControllerBase : ManagementApiControllerBase
{
}

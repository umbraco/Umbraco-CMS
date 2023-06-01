using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

[ApiController]
[VersionedApiBackOfficeRoute("user/current")]
[ApiExplorerSettings(GroupName = "User")]
public abstract class CurrentUserControllerBase : UserControllerBase
{

}


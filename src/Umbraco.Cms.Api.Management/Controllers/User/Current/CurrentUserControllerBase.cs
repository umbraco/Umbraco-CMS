using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

[ApiController]
[VersionedApiBackOfficeRoute("user/current")]
public abstract class CurrentUserControllerBase : UserOrCurrentUserControllerBase
{

}


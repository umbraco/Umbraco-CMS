using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiController]
[VersionedApiBackOfficeRoute("user")]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessUsers)]
public abstract class UserControllerBase : UserOrCurrentUserControllerBase
{
}

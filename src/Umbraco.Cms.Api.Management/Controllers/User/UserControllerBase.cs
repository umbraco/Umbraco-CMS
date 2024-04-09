using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[VersionedApiBackOfficeRoute("user")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessUsers)]
public abstract class UserControllerBase : UserOrCurrentUserControllerBase
{
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.User.Filter;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Filter}/user")]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessUsers)]
public abstract class UserFilterControllerBase : UserOrCurrentUserControllerBase
{
}

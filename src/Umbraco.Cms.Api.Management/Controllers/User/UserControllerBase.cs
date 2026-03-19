using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.User;

/// <summary>
/// Serves as the base controller for API endpoints that manage user-related operations in the Umbraco CMS.
/// </summary>
[VersionedApiBackOfficeRoute("user")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessUsers)]
public abstract class UserControllerBase : UserOrCurrentUserControllerBase
{
}

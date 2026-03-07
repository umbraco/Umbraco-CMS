using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.User.Filter;

    /// <summary>
    /// Serves as the base API controller for handling user filter-related operations in the management interface.
    /// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Filter}/user")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessUsers)]
public abstract class UserFilterControllerBase : UserOrCurrentUserControllerBase
{
}

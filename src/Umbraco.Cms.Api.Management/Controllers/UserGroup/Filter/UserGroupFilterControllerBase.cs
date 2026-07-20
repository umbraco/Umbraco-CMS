using Umbraco.Cms.Api.Management.Controllers.UserGroup;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.User.Filter;

/// <summary>
/// Serves as the base controller for implementing user group filtering functionality in the management API.
/// Provides common logic and endpoints for derived controllers handling user group filters.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Filter}/user-group")]
public abstract class UserGroupFilterControllerBase : UserGroupControllerBase
{
}

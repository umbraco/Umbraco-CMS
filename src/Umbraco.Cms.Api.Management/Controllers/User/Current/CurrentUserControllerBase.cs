using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

/// <summary>
/// Serves as the base controller for operations related to the current user in the management API.
/// </summary>
[VersionedApiBackOfficeRoute("user/current")]
public abstract class CurrentUserControllerBase : UserOrCurrentUserControllerBase
{
}


using Umbraco.Cms.Api.Management.Controllers.UserGroup;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.User.Filter;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Filter}/user-group")]
public abstract class UserGroupFilterControllerBase : UserGroupControllerBase
{
}

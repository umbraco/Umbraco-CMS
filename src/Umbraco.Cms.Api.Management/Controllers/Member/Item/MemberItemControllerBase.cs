using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Member.Item;

/// <summary>
/// Serves as the base controller for member item operations in the Umbraco CMS Management API.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.Member}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Member))]
public class MemberItemControllerBase : ManagementApiControllerBase
{
}

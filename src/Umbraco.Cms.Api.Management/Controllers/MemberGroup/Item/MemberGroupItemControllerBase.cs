using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup.Item;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.MemberGroup}")]
[ApiExplorerSettings(GroupName = "Member Group")]
public class MemberGroupItemControllerBase : ManagementApiControllerBase
{
}

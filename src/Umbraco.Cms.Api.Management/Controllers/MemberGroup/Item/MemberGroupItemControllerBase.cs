using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup.Item;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.MemberGroup}")]
[ApiExplorerSettings(GroupName = "Member Group")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessMemberGroups)]
public class MemberGroupItemControllerBase : ManagementApiControllerBase
{

}

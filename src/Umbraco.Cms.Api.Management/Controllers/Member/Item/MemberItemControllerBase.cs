using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Member.Item;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Member}")]
[ApiExplorerSettings(GroupName = "Member")]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessForMemberTree)]
public class MemberItemControllerBase : ManagementApiControllerBase
{
}

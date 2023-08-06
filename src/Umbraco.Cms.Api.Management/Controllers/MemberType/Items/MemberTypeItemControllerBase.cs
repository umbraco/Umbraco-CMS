using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Items;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.MemberType}")]
[ApiExplorerSettings(GroupName = "Member Type")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessMemberTypes)]
public class MemberTypeItemControllerBase : ManagementApiControllerBase
{
}

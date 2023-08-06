using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup.Item;

[ApiController]
[VersionedApiBackOfficeRoute("user-group")]
[ApiExplorerSettings(GroupName = "User Group")]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessUsers)]
public class UserGroupItemControllerBase : ManagementApiControllerBase
{
}

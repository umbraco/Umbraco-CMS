using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Member.Item;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Member}")]
[ApiExplorerSettings(GroupName = "Member")]
public class MemberItemControllerBase : ManagementApiControllerBase
{
}

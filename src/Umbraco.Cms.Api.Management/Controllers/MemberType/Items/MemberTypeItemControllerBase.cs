using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Items;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.MemberType}")]
[ApiExplorerSettings(GroupName = "Member Type")]
public class MemberTypeItemControllerBase : ManagementApiControllerBase
{
}

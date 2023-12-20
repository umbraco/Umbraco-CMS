using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Preview;

[ApiController]
[VersionedApiBackOfficeRoute("preview")]
[ApiExplorerSettings(GroupName = "Preview")]
[Authorize(Policy = "New" + AuthorizationPolicies.BackOfficeAccess)]
public class PreviewControllerBase : ManagementApiControllerBase
{
}

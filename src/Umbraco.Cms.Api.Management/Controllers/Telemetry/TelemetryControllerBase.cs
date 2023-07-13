using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Telemetry;

[ApiController]
[VersionedApiBackOfficeRoute("telemetry")]
[ApiExplorerSettings(GroupName = "Telemetry")]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessSettings)]
public abstract class TelemetryControllerBase : ManagementApiControllerBase
{
}

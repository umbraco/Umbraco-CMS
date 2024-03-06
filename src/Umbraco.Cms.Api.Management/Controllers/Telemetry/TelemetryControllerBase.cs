using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Telemetry;

[VersionedApiBackOfficeRoute("telemetry")]
[ApiExplorerSettings(GroupName = "Telemetry")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
public abstract class TelemetryControllerBase : ManagementApiControllerBase
{
}

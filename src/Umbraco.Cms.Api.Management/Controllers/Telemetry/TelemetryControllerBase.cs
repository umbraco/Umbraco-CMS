using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Telemetry;

[ApiController]
[VersionedApiBackOfficeRoute("telemetry")]
[ApiExplorerSettings(GroupName = "Telemetry")]
[ApiVersion("1.0")]
public abstract class TelemetryControllerBase : ManagementApiControllerBase
{
}

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Telemetry;

[ApiController]
[VersionedApiBackOfficeRoute("telemetry")]
[ApiExplorerSettings(GroupName = "Telemetry")]
public abstract class TelemetryControllerBase : ManagementApiControllerBase
{
}

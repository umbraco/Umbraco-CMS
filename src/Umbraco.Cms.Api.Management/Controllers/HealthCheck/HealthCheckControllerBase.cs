using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Api.Management.Controllers.HealthCheck;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.HealthChecks.RoutePath.HealthCheck}")]
[ApiExplorerSettings(GroupName = "Health Check")]
public abstract class HealthCheckControllerBase : ManagementApiControllerBase
{
}

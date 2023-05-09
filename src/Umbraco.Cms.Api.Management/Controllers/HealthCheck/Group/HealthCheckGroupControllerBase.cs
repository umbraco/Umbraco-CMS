using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Api.Management.Controllers.HealthCheck.Group;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.HealthChecks.RoutePath.HealthCheck}-group")]
[ApiExplorerSettings(GroupName = "Health Check")]
public abstract class HealthCheckGroupControllerBase : ManagementApiControllerBase
{
}

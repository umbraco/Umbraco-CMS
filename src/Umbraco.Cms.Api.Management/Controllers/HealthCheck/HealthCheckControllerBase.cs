using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Api.Management.Controllers.HealthCheck;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.HealthChecks.RoutePath.HealthCheck}")]
[ApiExplorerSettings(GroupName = "Health Check")]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessSettings)]
public abstract class HealthCheckControllerBase : ManagementApiControllerBase
{
}

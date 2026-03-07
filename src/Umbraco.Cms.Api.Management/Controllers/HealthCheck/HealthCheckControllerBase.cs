using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Api.Management.Controllers.HealthCheck;

    /// <summary>
    /// Serves as the base controller for API endpoints related to health checks in the Umbraco CMS management API.
    /// </summary>
[VersionedApiBackOfficeRoute($"{Constants.HealthChecks.RoutePath.HealthCheck}")]
[ApiExplorerSettings(GroupName = "Health Check")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
public abstract class HealthCheckControllerBase : ManagementApiControllerBase
{
}

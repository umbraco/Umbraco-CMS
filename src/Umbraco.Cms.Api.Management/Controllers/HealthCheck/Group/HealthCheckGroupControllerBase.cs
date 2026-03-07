using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Api.Management.Controllers.HealthCheck.Group;

    /// <summary>
    /// Serves as the base controller for managing health check groups in the Umbraco CMS Management API.
    /// </summary>
[VersionedApiBackOfficeRoute($"{Constants.HealthChecks.RoutePath.HealthCheck}-group")]
[ApiExplorerSettings(GroupName = "Health Check")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
public abstract class HealthCheckGroupControllerBase : ManagementApiControllerBase
{
    protected IActionResult HealthCheckGroupNotFound() => NotFound(new ProblemDetailsBuilder()
        .WithTitle("The health check group could not be found")
        .Build());
}

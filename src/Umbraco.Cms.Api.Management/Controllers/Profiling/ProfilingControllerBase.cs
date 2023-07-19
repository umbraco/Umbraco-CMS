using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Profiling;

[ApiController]
[VersionedApiBackOfficeRoute("profiling")]
[ApiExplorerSettings(GroupName = "Profiling")]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessSettings)]
public class ProfilingControllerBase : ManagementApiControllerBase
{

      protected IActionResult WebProfilerOperationStatusResult(WebProfilerOperationStatus status) =>
        status switch
        {
            WebProfilerOperationStatus.ExecutingUserNotFound => Unauthorized(new ProblemDetailsBuilder()
                .WithTitle("Executing user not found")
                .WithDetail("Executing this action requires a signed in user.")
                .Build()),

            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown profiling operation status")
        };
}

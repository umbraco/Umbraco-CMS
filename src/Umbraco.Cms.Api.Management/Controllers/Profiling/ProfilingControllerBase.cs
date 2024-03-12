using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Profiling;

[VersionedApiBackOfficeRoute("profiling")]
[ApiExplorerSettings(GroupName = "Profiling")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
public class ProfilingControllerBase : ManagementApiControllerBase
{
    protected IActionResult WebProfilerOperationStatusResult(WebProfilerOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            WebProfilerOperationStatus.ExecutingUserNotFound => Unauthorized(problemDetailsBuilder
                .WithTitle("Executing user not found")
                .WithDetail("Executing this action requires a signed in user.")
                .Build()),

            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown profiling operation status.")
                .Build()),
        });
}

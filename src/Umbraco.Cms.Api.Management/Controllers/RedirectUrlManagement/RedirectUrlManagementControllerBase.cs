using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

/// <summary>
/// Serves as the base controller for managing redirect URLs in the Umbraco CMS Management API.
/// Provides common functionality for redirect URL management endpoints.
/// </summary>
[VersionedApiBackOfficeRoute("redirect-management")]
[ApiExplorerSettings(GroupName = "Redirect Management")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
public class RedirectUrlManagementControllerBase : ManagementApiControllerBase
{
    /// <summary>
    /// Maps a <see cref="RedirectUrlOperationStatus"/> to an appropriate <see cref="IActionResult"/>.
    /// </summary>
    /// <param name="status">The operation status to map.</param>
    /// <returns>An <see cref="IActionResult"/> describing the outcome of the operation.</returns>
    protected IActionResult RedirectUrlOperationStatusResult(RedirectUrlOperationStatus status) =>
        status switch
        {
            RedirectUrlOperationStatus.Success => Ok(),
            RedirectUrlOperationStatus.NotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("The redirect URL could not be found")
                .Build()),
            RedirectUrlOperationStatus.CancelledByNotification => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the redirect URL operation.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown redirect URL operation status.")
                .Build()),
        };
}

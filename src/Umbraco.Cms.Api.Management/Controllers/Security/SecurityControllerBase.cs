using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Security;

[VersionedApiBackOfficeRoute("security")]
[ApiExplorerSettings(GroupName = "Security")]
public abstract class SecurityControllerBase : ManagementApiControllerBase
{
    protected IActionResult UserOperationStatusResult(UserOperationStatus status, ErrorMessageResult? errorMessageResult = null) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            UserOperationStatus.UserNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The user was not found")
                .WithDetail("The specified user was not found.")
                .Build()),
            UserOperationStatus.InvalidPasswordResetToken => BadRequest(problemDetailsBuilder
                .WithTitle("The password reset token was invalid")
                .WithDetail("The specified password reset token was either used already or wrong.")
                .Build()),
            UserOperationStatus.UnknownFailure => BadRequest(problemDetailsBuilder
                .WithTitle("Unknown failure")
                .WithDetail(errorMessageResult?.Error?.ErrorMessage ?? "The error was unknown")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown user operation status.")
                .Build()),
        });
}

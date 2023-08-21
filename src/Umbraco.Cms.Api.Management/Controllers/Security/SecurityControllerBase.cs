using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Security;

[ApiController]
[VersionedApiBackOfficeRoute("security")]
[ApiExplorerSettings(GroupName = "Security")]
public abstract class SecurityControllerBase : ManagementApiControllerBase
{
    protected IActionResult UserOperationStatusResult(UserOperationStatus status, ErrorMessageResult? errorMessageResult = null) =>
        status switch
        {
            UserOperationStatus.Success => Ok(),
            UserOperationStatus.UserNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("The user was not found")
                .WithDetail("The specified user was not found.")
                .Build()),
            UserOperationStatus.InvalidPasswordResetToken => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("The password reset token was invalid")
                .WithDetail("The specified password reset token was either used already or wrong.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown user operation status.")
                .Build()),
        };
}

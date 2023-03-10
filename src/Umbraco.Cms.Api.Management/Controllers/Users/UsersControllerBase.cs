using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Users;

[ApiController]
[VersionedApiBackOfficeRoute("users")]
[ApiExplorerSettings(GroupName = "Users")]
[ApiVersion("1.0")]
public class UsersControllerBase : ManagementApiControllerBase
{
    protected IActionResult UserOperationStatusResult(UserOperationStatus status) =>
        status switch
        {
            UserOperationStatus.Success => Ok(),
            UserOperationStatus.MissingUser => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Missing User")
                .WithDetail("A performing user is required for the operation, but none was found.")
                .Build()),
            UserOperationStatus.MissingUserGroup => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Missing User Group")
                .WithDetail("The specified user group was not found.")
                .Build()),
            UserOperationStatus.UserNameIsNotEmail => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid Username")
                .WithDetail("The username must be the same as the email.")
                .Build()),
            UserOperationStatus.EmailCannotBeChanged => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Email Cannot be changed")
                .WithDetail("Local login is disabled, so the email cannot be changed.")
                .Build()),
            UserOperationStatus.DuplicateUserName => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Duplicate Username")
                .WithDetail("The username is already in use.")
                .Build()),
            UserOperationStatus.DuplicateEmail => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Duplicate Email")
                .WithDetail("The email is already in use.")
                .Build()),
            UserOperationStatus.Unauthorized => Unauthorized(),
            UserOperationStatus.CancelledByNotification => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the user operation.")
                .Build()),
            UserOperationStatus.CannotInvite => StatusCode(StatusCodes.Status500InternalServerError, "Cannot send user invitation."),
            UserOperationStatus.CannotDelete => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cannot delete user")
                .WithDetail("The user cannot be deleted.")
                .Build()),
            UserOperationStatus.CannotDisableSelf => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cannot disable")
                .WithDetail("A user cannot disable itself.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown user group operation status."),
        };

    protected IActionResult FormatErrorMessageResult(IErrorMessageResult errorMessageResult) =>
        BadRequest(new ProblemDetailsBuilder()
            .WithTitle("An error occured.")
            .WithDetail(errorMessageResult.ErrorMessage ?? "The error was unknown")
            .Build());
}

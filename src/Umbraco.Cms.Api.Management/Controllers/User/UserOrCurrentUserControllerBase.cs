using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiController]
[ApiExplorerSettings(GroupName = "User")]
public abstract class UserOrCurrentUserControllerBase : ManagementApiControllerBase
{
    protected IActionResult UserOperationStatusResult(UserOperationStatus status, ErrorMessageResult? errorMessageResult = null) =>
        status switch
        {
            UserOperationStatus.Success => Ok(),
            UserOperationStatus.MissingUser =>
                StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                    .WithTitle("A performing user is required for the operation, but none was found")
                    .Build()),
            UserOperationStatus.MissingUserGroup => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Missing User Group")
                .WithDetail("The specified user group was not found.")
                .Build()),
            UserOperationStatus.NoUserGroup => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("No User Group Specified")
                .WithDetail("A user group must be specified to create a user")
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
            UserOperationStatus.CannotInvite =>
                StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                    .WithTitle("Cannot send user invitation")
                    .Build()),
            UserOperationStatus.CannotDelete => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cannot delete user")
                .WithDetail("The user cannot be deleted.")
                .Build()),
            UserOperationStatus.CannotDisableSelf => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cannot disable")
                .WithDetail("A user cannot disable itself.")
                .Build()),
            UserOperationStatus.CannotDeleteSelf => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cannot delete")
                .WithDetail("A user cannot delete itself.")
                .Build()),
            UserOperationStatus.OldPasswordRequired => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Old password required")
                .WithDetail("The old password is required to change the password of the specified user.")
                .Build()),
            UserOperationStatus.InvalidAvatar => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid avatar")
                .WithDetail("The selected avatar is invalid")
                .Build()),
            UserOperationStatus.InvalidEmail => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid email")
                .WithDetail("The email is invalid")
                .Build()),
            UserOperationStatus.AvatarFileNotFound => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Avatar file not found")
                .WithDetail("The file key did not resolve in to a file")
                .Build()),
            UserOperationStatus.ContentStartNodeNotFound => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Content Start Node not found")
                .WithDetail("Some of the provided content start nodes was not found.")
                .Build()),
            UserOperationStatus.MediaStartNodeNotFound => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Media Start Node not found")
                .WithDetail("Some of the provided media start nodes was not found.")
                .Build()),
            UserOperationStatus.UserNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("The user was not found")
                .WithDetail("The specified user was not found.")
                .Build()),
            UserOperationStatus.CannotDisableInvitedUser => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cannot disable invited user")
                .WithDetail("An invited user cannot be disabled.")
                .Build()),
            UserOperationStatus.UnknownFailure => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Unknown failure")
                .WithDetail(errorMessageResult?.Error?.ErrorMessage ?? "The error was unknown")
                .Build()),
            UserOperationStatus.InvalidIsoCode => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid ISO code")
                .WithDetail("The specified ISO code is invalid.")
                .Build()),
            UserOperationStatus.InvalidInviteToken => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid verification token")
                .WithDetail("The specified verification token is invalid.")
                .Build()),
            UserOperationStatus.MediaNodeNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Media node not found")
                .WithDetail("The specified media node was not found.")
                .Build()),
            UserOperationStatus.ContentNodeNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Content node not found")
                .WithDetail("The specified content node was not found.")
                .Build()),
            UserOperationStatus.NotInInviteState => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid user state")
                .WithDetail("The target user is not in the invite state.")
                .Build()),
            UserOperationStatus.Forbidden => Forbidden(),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown user operation status.")
                .Build()),
        };

    protected IActionResult TwoFactorOperationStatusResult(TwoFactorOperationStatus status) =>
        status switch
        {
            TwoFactorOperationStatus.Success => Ok(),
            TwoFactorOperationStatus.ProviderNameNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Missing 2FA provider")
                .WithDetail("The specified 2fa provider was not found.")
                .Build()),
            TwoFactorOperationStatus.ProviderAlreadySetup => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("2FA provider already configured")
                .WithDetail("The current user already have the provider configured.")
                .Build()),
            TwoFactorOperationStatus.InvalidCode => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid code")
                .WithDetail("The specified 2fa code was invalid in combination with the provider.")
                .Build()),
            TwoFactorOperationStatus.UserNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("User not found")
                .WithDetail("The specified user id was not found.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown two factor operation status.")
                .Build()),
        };
}

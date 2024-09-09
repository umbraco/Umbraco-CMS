using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiExplorerSettings(GroupName = "User")]
public abstract class UserOrCurrentUserControllerBase : ManagementApiControllerBase
{
    protected IActionResult UserOperationStatusResult(UserOperationStatus status, ErrorMessageResult? errorMessageResult = null) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            UserOperationStatus.MissingUser =>
                StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                    .WithTitle("A performing user is required for the operation, but none was found")
                    .Build()),
            UserOperationStatus.MissingUserGroup => NotFound(problemDetailsBuilder
                .WithTitle("Missing User Group")
                .WithDetail("The specified user group was not found.")
                .Build()),
            UserOperationStatus.AdminUserGroupMustNotBeEmpty => BadRequest(problemDetailsBuilder
                .WithTitle("Admin User Group Must Not Be Empty")
                .WithDetail("The admin user group must not be empty.")
                .Build()),
            UserOperationStatus.NoUserGroup => BadRequest(problemDetailsBuilder
                .WithTitle("No User Group Specified")
                .WithDetail("A user group must be specified to create a user")
                .Build()),
            UserOperationStatus.UserNameIsNotEmail => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid Username")
                .WithDetail("The username must be the same as the email.")
                .Build()),
            UserOperationStatus.EmailCannotBeChanged => BadRequest(problemDetailsBuilder
                .WithTitle("Email Cannot be changed")
                .WithDetail("Local login is disabled, so the email cannot be changed.")
                .Build()),
            UserOperationStatus.DuplicateUserName => BadRequest(problemDetailsBuilder
                .WithTitle("Duplicate Username")
                .WithDetail("The username is already in use.")
                .Build()),
            UserOperationStatus.DuplicateEmail => BadRequest(problemDetailsBuilder
                .WithTitle("Duplicate Email")
                .WithDetail("The email is already in use.")
                .Build()),
            UserOperationStatus.Unauthorized => Unauthorized(),
            UserOperationStatus.CancelledByNotification => BadRequest(problemDetailsBuilder
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the user operation.")
                .Build()),
            UserOperationStatus.CannotInvite =>
                StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                    .WithTitle("Cannot send user invitation")
                    .Build()),
            UserOperationStatus.CannotDelete => BadRequest(problemDetailsBuilder
                .WithTitle("Cannot delete user")
                .WithDetail("The user cannot be deleted.")
                .Build()),
            UserOperationStatus.CannotDisableSelf => BadRequest(problemDetailsBuilder
                .WithTitle("Cannot disable")
                .WithDetail("A user cannot disable itself.")
                .Build()),
            UserOperationStatus.CannotDeleteSelf => BadRequest(problemDetailsBuilder
                .WithTitle("Cannot delete")
                .WithDetail("A user cannot delete itself.")
                .Build()),
            UserOperationStatus.SelfOldPasswordRequired => BadRequest(problemDetailsBuilder
                .WithTitle("Old password required")
                .WithDetail("The old password is required to change your own password.")
                .Build()),
            UserOperationStatus.InvalidAvatar => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid avatar")
                .WithDetail("The selected avatar is invalid")
                .Build()),
            UserOperationStatus.InvalidEmail => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid email")
                .WithDetail("The email is invalid")
                .Build()),
            UserOperationStatus.AvatarFileNotFound => BadRequest(problemDetailsBuilder
                .WithTitle("Avatar file not found")
                .WithDetail("The file key did not resolve in to a file")
                .Build()),
            UserOperationStatus.ContentStartNodeNotFound => BadRequest(problemDetailsBuilder
                .WithTitle("Content Start Node not found")
                .WithDetail("Some of the provided content start nodes was not found.")
                .Build()),
            UserOperationStatus.MediaStartNodeNotFound => BadRequest(problemDetailsBuilder
                .WithTitle("Media Start Node not found")
                .WithDetail("Some of the provided media start nodes was not found.")
                .Build()),
            UserOperationStatus.UserNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The user was not found")
                .WithDetail("The specified user was not found.")
                .Build()),
            UserOperationStatus.CannotDisableInvitedUser => BadRequest(problemDetailsBuilder
                .WithTitle("Cannot disable invited user")
                .WithDetail("An invited user cannot be disabled.")
                .Build()),
            UserOperationStatus.UnknownFailure => BadRequest(problemDetailsBuilder
                .WithTitle("Unknown failure")
                .WithDetail(errorMessageResult?.Error?.ErrorMessage ?? "The error was unknown")
                .Build()),
            UserOperationStatus.InvalidIsoCode => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid ISO code")
                .WithDetail("The specified ISO code is invalid.")
                .Build()),
            UserOperationStatus.InvalidInviteToken => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid verification token")
                .WithDetail("The specified verification token is invalid.")
                .Build()),
            UserOperationStatus.MediaNodeNotFound => NotFound(problemDetailsBuilder
                .WithTitle("Media node not found")
                .WithDetail("The specified media node was not found.")
                .Build()),
            UserOperationStatus.ContentNodeNotFound => NotFound(problemDetailsBuilder
                .WithTitle("Content node not found")
                .WithDetail("The specified content node was not found.")
                .Build()),
            UserOperationStatus.NodeNotFound => NotFound(problemDetailsBuilder
                .WithTitle("Node not found")
                .WithDetail("The specified node was not found.")
                .Build()),
            UserOperationStatus.NotInInviteState => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid user state")
                .WithDetail("The target user is not in the invite state.")
                .Build()),
            UserOperationStatus.SelfPasswordResetNotAllowed => BadRequest(problemDetailsBuilder
                .WithTitle("Self password reset not allowed")
                .WithDetail("It is not allowed to reset the password for the account you are logged in to.")
                .Build()),
            UserOperationStatus.InvalidUserType => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid user type")
                .WithDetail("The target user type does not support this operation.")
                .Build()),
            UserOperationStatus.Forbidden => Forbidden(),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown user operation status.")
                .Build()),
        });

    protected IActionResult TwoFactorOperationStatusResult(TwoFactorOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            TwoFactorOperationStatus.ProviderNameNotFound => NotFound(problemDetailsBuilder
                .WithTitle("Missing 2FA provider")
                .WithDetail("The specified 2fa provider was not found.")
                .Build()),
            TwoFactorOperationStatus.ProviderAlreadySetup => BadRequest(problemDetailsBuilder
                .WithTitle("2FA provider already configured")
                .WithDetail("The current user already have the provider configured.")
                .Build()),
            TwoFactorOperationStatus.InvalidCode => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid code")
                .WithDetail("The specified 2fa code was invalid in combination with the provider.")
                .Build()),
            TwoFactorOperationStatus.UserNotFound => NotFound(problemDetailsBuilder
                .WithTitle("User not found")
                .WithDetail("The specified user id was not found.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown two factor operation status.")
                .Build()),
        });

    protected IActionResult ExternalLoginOperationStatusResult(ExternalLoginOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            ExternalLoginOperationStatus.UserNotFound => NotFound(problemDetailsBuilder
                .WithTitle("User not found")
                .WithDetail("The specified user id was not found.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown two factor operation status.")
                .Build()),
        });
}

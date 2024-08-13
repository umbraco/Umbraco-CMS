using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup;

[VersionedApiBackOfficeRoute("user-group")]
[ApiExplorerSettings(GroupName = "User Group")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessUsers)]
public class UserGroupControllerBase : ManagementApiControllerBase
{
    protected IActionResult UserGroupOperationStatusResult(UserGroupOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            UserGroupOperationStatus.NotFound => UserGroupNotFound(problemDetailsBuilder),
            UserGroupOperationStatus.UserNotFound => NotFound(problemDetailsBuilder
                .WithTitle("User key not found")
                .WithDetail("The provided user key do not exist.")
                .Build()),
            UserGroupOperationStatus.AlreadyExists => Conflict(problemDetailsBuilder
                .WithTitle("User group already exists")
                .WithDetail("The user group exists already.")
                .Build()),
            UserGroupOperationStatus.DuplicateAlias => Conflict(problemDetailsBuilder
                .WithTitle("Duplicate alias")
                .WithDetail("A user group already exists with the attempted alias.")
                .Build()),
            UserGroupOperationStatus.CanNotUpdateAliasIsSystemUserGroup => BadRequest(problemDetailsBuilder
                .WithTitle("System user group")
                .WithDetail("Changing the alias is not allowed on a system user group.")
                .Build()),
            UserGroupOperationStatus.MissingUser => Unauthorized(problemDetailsBuilder
                .WithTitle("Missing user")
                .WithDetail("A performing user was not found when attempting the operation.")
                .Build()),
            UserGroupOperationStatus.CanNotDeleteIsSystemUserGroup => BadRequest(problemDetailsBuilder
                .WithTitle("System user group")
                .WithDetail("The operation is not allowed on a system user group.")
                .Build()),
            UserGroupOperationStatus.CancelledByNotification => BadRequest(problemDetailsBuilder
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the language operation.")
                .Build()),
            UserGroupOperationStatus.DocumentStartNodeKeyNotFound => NotFound(problemDetailsBuilder
                .WithTitle("Document start node key not found")
                .WithDetail("The assigned document start node does not exists.")
                .Build()),
            UserGroupOperationStatus.MediaStartNodeKeyNotFound => NotFound(problemDetailsBuilder
                .WithTitle("Media start node key not found")
                .WithDetail("The assigned media start node does not exists.")
                .Build()),
            UserGroupOperationStatus.DocumentPermissionKeyNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("A document permission key not found")
                .WithDetail("A assigned document permission not exists.")
                .Build()),
            UserGroupOperationStatus.LanguageNotFound => NotFound(problemDetailsBuilder
                .WithTitle("Language not found")
                .WithDetail("The specified language cannot be found.")
                .Build()),
            UserGroupOperationStatus.NameTooLong => BadRequest(problemDetailsBuilder
                .WithTitle("Name too long")
                .WithDetail("User Group name is too long.")
                .Build()),
            UserGroupOperationStatus.AliasTooLong => BadRequest(problemDetailsBuilder
                .WithTitle("Alias too long")
                .WithDetail("The user group alias is too long.")
                .Build()),
            UserGroupOperationStatus.MissingName => BadRequest(problemDetailsBuilder
                .WithTitle("Missing user group name.")
                .WithDetail("The user group name is required, and cannot be an empty string.")
                .Build()),
            UserGroupOperationStatus.AdminGroupCannotBeEmpty => BadRequest(problemDetailsBuilder
                .WithTitle("Admin group cannot be empty")
                .WithDetail("The admin group cannot be empty.")
                .Build()),
            UserGroupOperationStatus.UserNotInGroup => BadRequest(problemDetailsBuilder
                .WithTitle("User not in group")
                .WithDetail("The user is not in the group.")),
            UserGroupOperationStatus.Unauthorized => Unauthorized(problemDetailsBuilder
                .WithTitle("Unauthorized access")
                .WithDetail("The performing user does not have the necessary access to perform this operation. Check the log for details.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown user group operation status.")
                .Build()),
        });

    protected IActionResult UserGroupNotFound() => OperationStatusResult(UserGroupOperationStatus.NotFound, UserGroupNotFound);

    protected IActionResult UserGroupNotFound(ProblemDetailsBuilder problemDetailsBuilder) => NotFound(problemDetailsBuilder
        .WithTitle("The user group could not be found")
        .Build());
}

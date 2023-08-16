using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup;

[ApiController]
[VersionedApiBackOfficeRoute("user-group")]
[ApiExplorerSettings(GroupName = "User Group")]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessUsers)]
public class UserGroupControllerBase : ManagementApiControllerBase
{
    protected IActionResult UserGroupOperationStatusResult(UserGroupOperationStatus status) =>
        status switch
        {
            UserGroupOperationStatus.NotFound => UserGroupNotFound(),
            UserGroupOperationStatus.AlreadyExists => Conflict(new ProblemDetailsBuilder()
                .WithTitle("User group already exists")
                .WithDetail("The user group exists already.")
                .Build()),
            UserGroupOperationStatus.DuplicateAlias => Conflict(new ProblemDetailsBuilder()
                .WithTitle("Duplicate alias")
                .WithDetail("A user group already exists with the attempted alias.")
                .Build()),
            UserGroupOperationStatus.MissingUser => Unauthorized(new ProblemDetailsBuilder()
                .WithTitle("Missing user")
                .WithDetail("A performing user was not found when attempting to create the user group.")
                .Build()),
            UserGroupOperationStatus.IsSystemUserGroup => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("System user group")
                .WithDetail("The operation is not allowed on a system user group.")
                .Build()),
            UserGroupOperationStatus.UnauthorizedMissingUserSection => Unauthorized(new ProblemDetailsBuilder()
                .WithTitle("Unauthorized")
                .WithDetail("The performing user does not have access to the required section")
                .Build()),
            UserGroupOperationStatus.UnauthorizedMissingSections => Unauthorized(new ProblemDetailsBuilder()
                .WithTitle("Unauthorized section")
                .WithDetail("The specified allowed section contained a section the performing user doesn't have access to.")
                .Build()),
            UserGroupOperationStatus.UnauthorizedStartNodes => Unauthorized(new ProblemDetailsBuilder()
                .WithTitle("Unauthorized start node")
                .WithDetail("The specified start nodes contained a start node the performing user doesn't have access to.")
                .Build()),
            UserGroupOperationStatus.UnauthorizedMissingUserGroup => Unauthorized(new ProblemDetailsBuilder()
                .WithTitle("User not in user group")
                .WithDetail("The current user is not in the user group")
                .Build()),
            UserGroupOperationStatus.CancelledByNotification => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the language operation.")
                .Build()),
            UserGroupOperationStatus.DocumentStartNodeKeyNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Document start node key not found")
                .WithDetail("The assigned document start node does not exists.")
                .Build()),
            UserGroupOperationStatus.MediaStartNodeKeyNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Media start node key not found")
                .WithDetail("The assigned media start node does not exists.")
                .Build()),
            UserGroupOperationStatus.LanguageNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Language not found")
                .WithDetail("The specified language cannot be found.")
                .Build()),
            UserGroupOperationStatus.NameTooLong => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Name too long")
                .WithDetail("User Group name is too long.")
                .Build()),
            UserGroupOperationStatus.AliasTooLong => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Alias too long")
                .WithDetail("The user group alias is too long.")
                .Build()),
            UserGroupOperationStatus.MissingName => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Missing user group name.")
                .WithDetail("The user group name is required, and cannot be an empty string.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown user group operation status.")
                .Build()),
        };

    protected IActionResult UserGroupNotFound() => NotFound(new ProblemDetailsBuilder()
        .WithTitle("The user group could not be found")
        .Build());

}

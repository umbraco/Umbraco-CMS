﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroups;

// TODO: This needs to be an authorized controller.

[ApiController]
[VersionedApiBackOfficeRoute("user-groups")]
[ApiExplorerSettings(GroupName = "User Groups")]
[ApiVersion("1.0")]
public class UserGroupsControllerBase : ManagementApiControllerBase
{
    protected IActionResult UserGroupOperationStatusResult(UserGroupOperationStatus status) =>
        status switch
        {
            UserGroupOperationStatus.NotFound => NotFound(),
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
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown dictionary status."),
        };
}

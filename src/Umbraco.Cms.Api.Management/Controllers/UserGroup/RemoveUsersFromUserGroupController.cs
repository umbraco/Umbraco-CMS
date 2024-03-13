﻿using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup;

[ApiVersion("1.0")]
public class RemoveUsersFromUserGroupController : UserGroupControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IAuthorizationService _authorizationService;

    public RemoveUsersFromUserGroupController(
        IUserGroupService userGroupService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IAuthorizationService authorizationService)
    {
        _userGroupService = userGroupService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _authorizationService = authorizationService;
    }

    [HttpDelete("{id:guid}/users")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, Guid[] userIds)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserGroupPermissionResource.WithKeys(id),
            AuthorizationPolicies.UserBelongsToUserGroupInRequest);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        UserGroupOperationStatus result = await _userGroupService.RemoveUsersFromUserGroupAsync(
            new UsersToUserGroupManipulationModel(id, userIds), CurrentUserKey(_backOfficeSecurityAccessor));

        return result == UserGroupOperationStatus.Success
            ? Ok()
            : UserGroupOperationStatusResult(result);
    }
}

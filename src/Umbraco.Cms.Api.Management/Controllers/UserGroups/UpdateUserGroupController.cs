﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.UserGroups;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroups;

public class UpdateUserGroupController : UserGroupsControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IUserGroupPresentationFactory _userGroupPresentationFactory;

    public UpdateUserGroupController(
        IUserGroupService userGroupService,
        IUserGroupPresentationFactory userGroupPresentationFactory)
    {
        _userGroupService = userGroupService;
        _userGroupPresentationFactory = userGroupPresentationFactory;
    }

    [HttpPut("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid key, UpdateUserGroupRequestModel dataTypeRequestModel)
    {
        IUserGroup? existingUserGroup = await _userGroupService.GetAsync(key);

        if (existingUserGroup is null)
        {
            return UserGroupOperationStatusResult(UserGroupOperationStatus.NotFound);
        }

        Attempt<IUserGroup, UserGroupOperationStatus> userGroupUpdateAttempt = await _userGroupPresentationFactory.UpdateAsync(existingUserGroup, dataTypeRequestModel);
        if (userGroupUpdateAttempt.Success is false)
        {
            return UserGroupOperationStatusResult(userGroupUpdateAttempt.Status);
        }

        IUserGroup userGroup = userGroupUpdateAttempt.Result;
        Attempt<IUserGroup, UserGroupOperationStatus> result = await _userGroupService.UpdateAsync(userGroup, -1);

        return result.Success
            ? Ok()
            : UserGroupOperationStatusResult(result.Status);
    }
}

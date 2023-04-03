﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Users;

public class InviteUsersController : UsersControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserPresentationFactory _userPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public InviteUsersController(
        IUserService userService,
        IUserPresentationFactory userPresentationFactory, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _userService = userService;
        _userPresentationFactory = userPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }


    [HttpPost("invite")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Invite(InviteUserRequestModel model)
    {
        UserInviteModel userInvite = await _userPresentationFactory.CreateInviteModelAsync(model);

        Attempt<UserInvitationResult, UserOperationStatus> result = await _userService.InviteAsync(CurrentUserKey(_backOfficeSecurityAccessor), userInvite);

        return result.Success
            ? Ok()
            : UserOperationStatusResult(result.Status, result.Result);
    }
}

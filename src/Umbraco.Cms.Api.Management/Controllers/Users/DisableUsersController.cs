﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Users;

public class DisableUsersController : UsersControllerBase
{
    private readonly IUserService _userService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public DisableUsersController(IUserService userService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _userService = userService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost("disable")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DisableUsers(DisableUserRequestModel model)
    {
        UserOperationStatus result = await _userService.DisableAsync(CurrentUserKey(_backOfficeSecurityAccessor), model.UserIds);

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}

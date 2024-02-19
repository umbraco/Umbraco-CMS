﻿using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Security;

[ApiVersion("1.0")]
// FIXME: Add requiring password reset token policy when its implemented
public class ResetPasswordController : SecurityControllerBase
{
    private readonly IUserService _userService;

    public ResetPasswordController(IUserService userService) => _userService = userService;


    [HttpPost("forgot-password")]
    [MapToApiVersion("1.0")]
    [AllowAnonymous] // This is handled implicitly by the NewDenyLocalLoginIfConfigured policy on the <see cref="SecurityControllerBase" />. Keep it here for now and check FIXME in <see cref="DenyLocalLoginHandler" />.
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [UserPasswordEnsureMinimumResponseTime]
    public async Task<IActionResult> RequestPasswordReset(ResetPasswordRequestModel model)
    {
        Attempt<UserOperationStatus> result = await _userService.SendResetPasswordEmailAsync(model.Email);

        // If this feature is switched off in configuration, the UI will be amended to not make the request to reset password available.
        // So this is just a server-side secondary check.
        // Regardless of other status values, it will just return Ok, so you can't use this endpoint to determine whether the email exists in the system.
        return result.Result == UserOperationStatus.CannotPasswordReset
            ? BadRequest()
            : Ok();
    }
}

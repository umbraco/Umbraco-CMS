using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Security;

[ApiVersion("1.0")]
public class VerifyResetPasswordTokenController : SecurityControllerBase
{
    private readonly IUserService _userService;

    public VerifyResetPasswordTokenController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("forgot-password/verify")]
    [MapToApiVersion("1.0")]
    [AllowAnonymous]
    [ProducesResponseType( StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetailsBuilder), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetailsBuilder), StatusCodes.Status404NotFound)]
    [UserPasswordEnsureMinimumResponseTime]
    public async Task<IActionResult> VerifyResetPasswordToken(VerifyResetPasswordTokenRequestModel model)
    {
        Attempt<UserOperationStatus> result = await _userService.VerifyPasswordResetAsync(model.UserId, model.ResetCode);

        return result.Success
            ? NoContent()
            : UserOperationStatusResult(result.Result);
    }
}

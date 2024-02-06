using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Security;

[ApiVersion("1.0")]
public class ResetPasswordTokenController : SecurityControllerBase
{
    private readonly IUserService _userService;

    public ResetPasswordTokenController(IUserService userService) => _userService = userService;

    [HttpPost("forgot-password/reset")]
    [MapToApiVersion("1.0")]
    // [AllowAnonymous] // This is handled implicitly by the NewDenyLocalLoginIfConfigured policy on the <see cref="SecurityControllerBase" />. Keep it here for now and check FIXME in <see cref="DenyLocalLoginHandler" />.
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetailsBuilder), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetailsBuilder), StatusCodes.Status404NotFound)]
    [UserPasswordEnsureMinimumResponseTime]
    public async Task<IActionResult> ResetPasswordToken(ResetPasswordTokenRequestModel model)
    {
        Attempt<PasswordChangedModel, UserOperationStatus> result = await _userService.ResetPasswordAsync(model.User.Id, model.ResetCode, model.Password);

        return result.Success
            ? NoContent()
            : UserOperationStatusResult(result.Status, result.Result);
    }
}

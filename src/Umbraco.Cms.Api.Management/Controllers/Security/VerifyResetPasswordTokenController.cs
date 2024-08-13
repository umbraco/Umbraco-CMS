using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Factories;
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
    private readonly IPasswordConfigurationPresentationFactory _passwordConfigurationPresentationFactory;

    public VerifyResetPasswordTokenController(IUserService userService, IPasswordConfigurationPresentationFactory passwordConfigurationPresentationFactory)
    {
        _userService = userService;
        _passwordConfigurationPresentationFactory = passwordConfigurationPresentationFactory;
    }

    [AllowAnonymous]
    [HttpPost("forgot-password/verify")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(VerifyResetPasswordResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetailsBuilder), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetailsBuilder), StatusCodes.Status404NotFound)]
    [UserPasswordEnsureMinimumResponseTime]
    public async Task<IActionResult> VerifyResetPasswordToken(
        CancellationToken cancellationToken,
        VerifyResetPasswordTokenRequestModel model)
    {
        Attempt<UserOperationStatus> result = await _userService.VerifyPasswordResetAsync(model.User.Id, model.ResetCode);

        return result.Success
            ? Ok(new VerifyResetPasswordResponseModel()
            {
                PasswordConfiguration = _passwordConfigurationPresentationFactory.CreatePasswordConfigurationResponseModel(),
            })
            : UserOperationStatusResult(result.Result);
    }
}

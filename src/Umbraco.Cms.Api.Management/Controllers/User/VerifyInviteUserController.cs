using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiVersion("1.0")]
public class VerifyInviteUserController : UserControllerBase
{
    private readonly IUserService _userService;
    private readonly IPasswordConfigurationPresentationFactory _passwordConfigurationPresentationFactory;

    public VerifyInviteUserController(IUserService userService, IPasswordConfigurationPresentationFactory passwordConfigurationPresentationFactory)
    {
        _userService = userService;
        _passwordConfigurationPresentationFactory = passwordConfigurationPresentationFactory;
    }

    [AllowAnonymous]
    [HttpPost("invite/verify")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(VerifyInviteUserResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Invite(CancellationToken cancellationToken, VerifyInviteUserRequestModel model)
    {
        Attempt<UserOperationStatus> result = await _userService.VerifyInviteAsync(model.User.Id, model.Token);

        return result.Success
            ? Ok(new VerifyInviteUserResponseModel()
            {
                PasswordConfiguration = _passwordConfigurationPresentationFactory.CreatePasswordConfigurationResponseModel(),
            })
            : UserOperationStatusResult(result.Result);
    }
}

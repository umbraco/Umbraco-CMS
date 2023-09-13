using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiVersion("1.0")]
[Authorize(Policy = "New" + AuthorizationPolicies.DenyLocalLoginIfConfigured)]
public class VerifyInviteUserController : UserControllerBase
{
    private readonly IUserService _userService;

    public VerifyInviteUserController(IUserService userService) => _userService = userService;

    [AllowAnonymous]
    [HttpPost("invite/verify")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Invite(VerifyInviteUserRequestModel model)
    {
        Attempt<UserOperationStatus> result = await _userService.VerifyInviteAsync(model.UserId, model.Token);

        return result.Success
            ? Ok()
            : UserOperationStatusResult(result.Result);
    }
}

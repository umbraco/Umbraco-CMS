using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiVersion("1.0")]
[Authorize(Policy = "New" + AuthorizationPolicies.BackOfficeAccessWithoutApproval)]
[Authorize(Policy = "New" + AuthorizationPolicies.DenyLocalLoginIfConfigured)]
public class CreateInitialPasswordUserController : UserControllerBase
{
    private readonly IUserService _userService;

    public CreateInitialPasswordUserController(IUserService userService) => _userService = userService;

    //[AllowAnonymous] // This is handled implicitly by the NewDenyLocalLoginIfConfigured policy on the <see cref="SecurityControllerBase" />. Keep it here for now and check FIXME in <see cref="DenyLocalLoginHandler" />.
    [HttpPost("invite/create-password")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateInitialPassword(CreateInitialPasswordUserRequestModel model)
    {
        Attempt<PasswordChangedModel, UserOperationStatus> response = await _userService.CreateInitialPasswordAsync(model.UserId, model.Token, model.Password);

        return response.Success
            ? Ok()
            : UserOperationStatusResult(response.Status, response.Result);
    }
}

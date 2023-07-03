using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiVersion("1.0")]
public class CreateInitialPasswordUserController : UserControllerBase
{
    private readonly IUserService _userService;

    public CreateInitialPasswordUserController(IUserService userService) => _userService = userService;

    [AllowAnonymous]
    [HttpPost("invite/create-password")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> CreateInitialPassword(CreateInitialPasswordUserRequestModel model)
    {
        Attempt<PasswordChangedModel, UserOperationStatus> response = await _userService.CreateInitialPasswordAsync(model.UserId, model.Token, model.Password);

        return response.Success
            ? NoContent()
            : UserOperationStatusResult(response.Status, response.Result);
    }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Users;

public class ChangePasswordUsersController : UsersControllerBase
{
    private readonly IUserService _userService;

    public ChangePasswordUsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("change-password/{id:guid}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> ChangePassword(Guid id, ChangePasswordUserRequestModel model)
    {
        IUser? existingUser = await _userService.GetAsync(id);

        if (existingUser is null)
        {
            return NotFound();
        }

        var passwordModel = new ChangeBackofficeUserPasswordModel
        {
            NewPassword = model.NewPassword,
            OldPassword = model.OldPassword,
            User = existingUser,
        };

        // FIXME: use the actual currently logged in user key
        Attempt<PasswordChangedModel, UserOperationStatus> response = await _userService.ChangePasswordAsync(Constants.Security.SuperUserKey, passwordModel);

        if (response.Success)
        {
            return Ok(new ChangePasswordUserResponseModel { ResetPassword = response.Result.ResetPassword });
        }

        if (response.Result.ChangeError is not null)
        {
            ValidationResult validationError = response.Result.ChangeError;

            return BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Password change failed")
                .WithDetail(validationError.ErrorMessage!)
                .Build());
        }

        return UserOperationStatusResult(response.Status);
    }
}

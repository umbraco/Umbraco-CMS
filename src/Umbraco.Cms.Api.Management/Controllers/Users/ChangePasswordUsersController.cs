using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Users;

public class ChangePasswordUsersController : UsersControllerBase
{
    private readonly IUserService _userService;
    private readonly IUmbracoMapper _mapper;

    public ChangePasswordUsersController(
        IUserService userService,
        IUmbracoMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    [HttpPost("change-password/{id:guid}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> ChangePassword(Guid id, ChangePasswordUserRequestModel model)
    {
        var passwordModel = new ChangeUserPasswordModel
        {
            NewPassword = model.NewPassword,
            OldPassword = model.OldPassword,
            UserKey = id,
        };

        // FIXME: use the actual currently logged in user key
        Attempt<PasswordChangedModel, UserOperationStatus> response = await _userService.ChangePasswordAsync(Constants.Security.SuperUserKey, passwordModel);

        return response.Success
            ? Ok(_mapper.Map<ChangePasswordUserResponseModel>(response.Result))
            : UserOperationStatusResult(response.Status, response.Result);
    }
}

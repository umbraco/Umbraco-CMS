using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiVersion("1.0")]
public class ChangePasswordUserController : UserControllerBase
{
    private readonly IUserService _userService;
    private readonly IUmbracoMapper _mapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public ChangePasswordUserController(
        IUserService userService,
        IUmbracoMapper mapper, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _userService = userService;
        _mapper = mapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
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

        Attempt<PasswordChangedModel, UserOperationStatus> response = await _userService.ChangePasswordAsync(CurrentUserKey(_backOfficeSecurityAccessor), passwordModel);

        return response.Success
            ? Ok(_mapper.Map<ChangePasswordUserResponseModel>(response.Result))
            : UserOperationStatusResult(response.Status, response.Result);
    }
}

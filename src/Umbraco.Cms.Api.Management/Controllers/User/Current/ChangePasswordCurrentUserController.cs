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

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

[ApiVersion("1.0")]
public class ChangePasswordCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserService _userService;
    private readonly IUmbracoMapper _mapper;

    public ChangePasswordCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserService userService,
        IUmbracoMapper mapper)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userService = userService;
        _mapper = mapper;
    }

    [HttpPost("change-password")]
    [MapToApiVersion("1.0")]
    [ProducesErrorResponseType(typeof(ChangePasswordUserResponseModel))]
    public async Task<IActionResult> ChangePassword(ChangePasswordUserRequestModel model)
    {
        Guid userKey = CurrentUserKey(_backOfficeSecurityAccessor);

        var changeModel = new ChangeUserPasswordModel
        {
            NewPassword = model.NewPassword,
            OldPassword = model.OldPassword,
            UserKey = userKey,
        };

        Attempt<PasswordChangedModel, UserOperationStatus> response = await _userService.ChangePasswordAsync(userKey, changeModel);

        return response.Success
            ? Ok(_mapper.Map<ChangePasswordUserResponseModel>(response.Result))
            : UserOperationStatusResult(response.Status, response.Result);
    }
}

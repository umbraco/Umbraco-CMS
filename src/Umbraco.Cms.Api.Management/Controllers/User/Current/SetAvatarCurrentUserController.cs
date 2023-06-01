using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

[ApiVersion("1.0")]
public class SetAvatarCurrentUserController : CurrentUserControllerBase
{
    private readonly IUserService _userService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public SetAvatarCurrentUserController(
        IUserService userService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _userService = userService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [MapToApiVersion("1.0")]
    [HttpPost("avatar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetAvatar(SetAvatarRequestModel model)
    {
        Guid userKey = CurrentUserKey(_backOfficeSecurityAccessor);

        UserOperationStatus result = await _userService.SetAvatarAsync(userKey, model.FileId);

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}

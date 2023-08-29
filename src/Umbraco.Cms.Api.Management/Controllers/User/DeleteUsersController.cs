using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiVersion("1.0")]
public class DeleteUserController : UserControllerBase
{
    public DeleteUserController(IUserService userService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _userService = userService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    private readonly IUserService _userService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    [MapToApiVersion("1.0")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        UserOperationStatus result = await _userService.DeleteAsync(CurrentUserKey(_backOfficeSecurityAccessor), id);

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}

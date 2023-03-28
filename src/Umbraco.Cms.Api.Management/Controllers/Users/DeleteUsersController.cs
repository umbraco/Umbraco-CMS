using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Users;

public class DeleteUsersController : UsersControllerBase
{
    public DeleteUsersController(IUserService userService) => _userService = userService;

    private readonly IUserService _userService;

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        UserOperationStatus result = await _userService.DeleteAsync(id);

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}

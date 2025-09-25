using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.UserData;

[ApiVersion("1.0")]
public class DeleteUserDataController : UserDataControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserDataService _userDataService;

    public DeleteUserDataController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserDataService userDataService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userDataService = userDataService;
    }

    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserDataOperationStatus), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(UserDataOperationStatus), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id)
    {
        IUserData? data = await _userDataService.GetAsync(id);
        if (data is null)
        {
            return NotFound();
        }

        Guid currentUserKey = CurrentUserKey(_backOfficeSecurityAccessor);

        if (data.UserKey != currentUserKey)
        {
            return Unauthorized();
        }

        Attempt<UserDataOperationStatus> attempt = await _userDataService.DeleteAsync(id);

        return attempt.Success
            ? Ok()
            : UserDataOperationStatusResult(attempt.Result);
    }
}

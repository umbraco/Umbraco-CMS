using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

[ApiVersion("1.0")]
public class ListTwoFactorProvidersCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserTwoFactorLoginService _userTwoFactorLoginService;

    public ListTwoFactorProvidersCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserTwoFactorLoginService userUserTwoFactorLoginService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userTwoFactorLoginService = userUserTwoFactorLoginService;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("2fa")]
    [ProducesResponseType(typeof(IEnumerable<UserTwoFactorProviderModel>),StatusCodes.Status200OK)]
    public async Task<IActionResult> ListTwoFactorProvidersForCurrentUser(CancellationToken cancellationToken)
    {
        Guid userKey = CurrentUserKey(_backOfficeSecurityAccessor);

        Attempt<IEnumerable<UserTwoFactorProviderModel>, TwoFactorOperationStatus> result = await _userTwoFactorLoginService.GetProviderNamesAsync(userKey);

        return result.Success
            ? Ok(result.Result)
            : TwoFactorOperationStatusResult(result.Status);
    }
}

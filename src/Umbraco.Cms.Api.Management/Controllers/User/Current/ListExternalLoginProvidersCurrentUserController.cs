using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

[ApiVersion("1.0")]
public class ListExternalLoginProvidersCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IBackOfficeExternalLoginService _backOfficeExternalLoginService;

    public ListExternalLoginProvidersCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IBackOfficeExternalLoginService backOfficeExternalLoginService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _backOfficeExternalLoginService = backOfficeExternalLoginService;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("login-providers")]
    [ProducesResponseType(typeof(IEnumerable<UserExternalLoginProviderModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListTwoFactorProvidersForCurrentUser(CancellationToken cancellationToken)
    {
        Guid userKey = CurrentUserKey(_backOfficeSecurityAccessor);

        Attempt<IEnumerable<UserExternalLoginProviderModel>, ExternalLoginOperationStatus> result =
            await _backOfficeExternalLoginService.ExternalLoginStatusForUser(userKey);

        return result.Success
            ? Ok(result.Result)
            : ExternalLoginOperationStatusResult(result.Status);
    }
}

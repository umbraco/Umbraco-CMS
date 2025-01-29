using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

[ApiVersion("1.0")]
public class EnableTwoFactorProviderCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserTwoFactorLoginService _userTwoFactorLoginService;

    public EnableTwoFactorProviderCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserTwoFactorLoginService userTwoFactorLoginService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userTwoFactorLoginService = userTwoFactorLoginService;
    }

    [MapToApiVersion("1.0")]
    [HttpPost("2fa/{providerName}")]
    [ProducesResponseType(typeof(ISetupTwoFactorModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnableTwoFactorProvider(
        CancellationToken cancellationToken,
        string providerName,
        EnableTwoFactorRequestModel model)
    {
        Guid userKey = CurrentUserKey(_backOfficeSecurityAccessor);

        Attempt<TwoFactorOperationStatus> result = await _userTwoFactorLoginService.ValidateAndSaveAsync(providerName, userKey, model.Secret, model.Code);

        return result.Success
            ? Ok()
            : TwoFactorOperationStatusResult(result.Result);
    }
}


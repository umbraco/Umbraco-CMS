﻿using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

[ApiVersion("1.0")]
public class GetTwoFactorSetupForProviderCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserTwoFactorLoginService _userTwoFactorLoginService;

    public GetTwoFactorSetupForProviderCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserTwoFactorLoginService userTwoFactorLoginService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userTwoFactorLoginService = userTwoFactorLoginService;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("2fa/{providerName}")]
    [ProducesResponseType(typeof(ISetupTwoFactorModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Gets two-factor setup information.")]
    [EndpointDescription("Gets the setup information for configuring a two-factor authentication provider.")]
    public async Task<IActionResult> GetTwoFactorProviderByName(CancellationToken cancellationToken, string providerName)
    {
        Guid userKey = CurrentUserKey(_backOfficeSecurityAccessor);

        Attempt<ISetupTwoFactorModel, TwoFactorOperationStatus> result = await _userTwoFactorLoginService.GetSetupInfoAsync(userKey, providerName);

        return result.Status is TwoFactorOperationStatus.Success
            ? Ok(result.Result)
            : TwoFactorOperationStatusResult(result.Status);
    }
}

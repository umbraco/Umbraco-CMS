using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Security;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.DenyLocalLoginIfConfigured)]
public class ResetPasswordTokenController : SecurityControllerBase
{
    private readonly IUserService _userService;
    private readonly IOpenIddictTokenManager _tokenManager;

    public ResetPasswordTokenController(IUserService userService, IOpenIddictTokenManager tokenManager)
    {
        _userService = userService;
        _tokenManager = tokenManager;
    }

    [HttpPost("forgot-password/reset")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetailsBuilder), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetailsBuilder), StatusCodes.Status404NotFound)]
    [UserPasswordEnsureMinimumResponseTime]
    public async Task<IActionResult> ResetPasswordToken(CancellationToken cancellationToken, ResetPasswordTokenRequestModel model)
    {
        Attempt<PasswordChangedModel, UserOperationStatus> result = await _userService.ResetPasswordAsync(model.User.Id, model.ResetCode, model.Password);

        if (result.Success is false)
        {
            return UserOperationStatusResult(result.Status, result.Result);
        }

        await _tokenManager.RevokeUmbracoUserTokens(model.User.Id);
        return Ok();

    }
}

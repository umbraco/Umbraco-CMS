using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.DenyLocalLoginIfConfigured)]
public class CreateInitialPasswordUserController : UserControllerBase
{
    private readonly IUserService _userService;
    private readonly IOpenIddictTokenManager _tokenManager;

    public CreateInitialPasswordUserController(IUserService userService, IOpenIddictTokenManager tokenManager)
    {
        _userService = userService;
        _tokenManager = tokenManager;
    }

    [AllowAnonymous]
    [HttpPost("invite/create-password")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInitialPassword(
        CancellationToken cancellationToken,
        CreateInitialPasswordUserRequestModel model)
    {
        Attempt<PasswordChangedModel, UserOperationStatus> response = await _userService.CreateInitialPasswordAsync(model.User.Id, model.Token, model.Password);

        if (response.Success is false)
        {
            return UserOperationStatusResult(response.Status, response.Result);
        }

        await _tokenManager.RevokeUmbracoUserTokens(model.User.Id);
        return Ok();
    }
}

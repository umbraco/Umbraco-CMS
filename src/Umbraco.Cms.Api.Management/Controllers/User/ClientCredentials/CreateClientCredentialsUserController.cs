using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.User.ClientCredentials;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Security.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User.ClientCredentials;

[ApiVersion("1.0")]
public class CreateClientCredentialsUserController : ClientCredentialsUserControllerBase
{
    private readonly IBackOfficeUserClientCredentialsManager _backOfficeUserClientCredentialsManager;
    private readonly IAuthorizationService _authorizationService;

    public CreateClientCredentialsUserController(
        IBackOfficeUserClientCredentialsManager backOfficeUserClientCredentialsManager,
        IAuthorizationService authorizationService)
    {
        _backOfficeUserClientCredentialsManager = backOfficeUserClientCredentialsManager;
        _authorizationService = authorizationService;
    }

    [HttpPost("{id:guid}/client-credentials")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken, Guid id, CreateUserClientCredentialsRequestModel model)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(id),
            AuthorizationPolicies.UserPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<BackOfficeUserClientCredentialsOperationStatus> result = await _backOfficeUserClientCredentialsManager.SaveAsync(id, model.ClientId, model.ClientSecret);
        return result.Success
            ? Ok()
            : BackOfficeUserClientCredentialsOperationStatusResult(result.Result);
    }
}

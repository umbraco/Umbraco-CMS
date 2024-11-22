using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User.ClientCredentials;

[ApiVersion("1.0")]
public class GetAllClientCredentialsUserController : ClientCredentialsUserControllerBase
{
    private readonly IBackOfficeUserClientCredentialsManager _backOfficeUserClientCredentialsManager;
    private readonly IAuthorizationService _authorizationService;

    public GetAllClientCredentialsUserController(
        IBackOfficeUserClientCredentialsManager backOfficeUserClientCredentialsManager,
        IAuthorizationService authorizationService)
    {
        _backOfficeUserClientCredentialsManager = backOfficeUserClientCredentialsManager;
        _authorizationService = authorizationService;
    }

    [HttpGet("{id:guid}/client-credentials")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(id),
            AuthorizationPolicies.UserPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        IEnumerable<string> clientIds = await _backOfficeUserClientCredentialsManager.GetClientIdsAsync(id);
        return Ok(clientIds);
    }
}

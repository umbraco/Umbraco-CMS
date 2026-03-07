using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User.ClientCredentials;

    /// <summary>
    /// Controller responsible for retrieving all users authenticated via client credentials.
    /// </summary>
[ApiVersion("1.0")]
public class GetAllClientCredentialsUserController : ClientCredentialsUserControllerBase
{
    private readonly IBackOfficeUserClientCredentialsManager _backOfficeUserClientCredentialsManager;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllClientCredentialsUserController"/> class, responsible for managing user client credentials via the API.
    /// </summary>
    /// <param name="backOfficeUserClientCredentialsManager">Service for managing back office user client credentials.</param>
    /// <param name="authorizationService">Service used to authorize access to controller actions.</param>
    public GetAllClientCredentialsUserController(
        IBackOfficeUserClientCredentialsManager backOfficeUserClientCredentialsManager,
        IAuthorizationService authorizationService)
    {
        _backOfficeUserClientCredentialsManager = backOfficeUserClientCredentialsManager;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Retrieves all OAuth client credential identifiers associated with the specified user.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the user whose client credentials are to be retrieved.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a collection of strings, each representing a client credential identifier associated with the user.
    /// Returns 200 OK with the collection if successful; otherwise, returns 403 Forbidden if the user is not authorized.
    /// </returns>
    [HttpGet("{id:guid}/client-credentials")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets all client credentials for a user.")]
    [EndpointDescription("Gets a collection of OAuth client credentials for the user identified by the provided Id.")]
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

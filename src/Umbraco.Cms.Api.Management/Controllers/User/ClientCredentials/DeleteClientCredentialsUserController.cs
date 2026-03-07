using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Security.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User.ClientCredentials;

    /// <summary>
    /// Controller responsible for handling the deletion of client credentials associated with a user.
    /// </summary>
[ApiVersion("1.0")]
public class DeleteClientCredentialsUserController : ClientCredentialsUserControllerBase
{
    private readonly IBackOfficeUserClientCredentialsManager _backOfficeUserClientCredentialsManager;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteClientCredentialsUserController"/> class.
    /// </summary>
    /// <param name="backOfficeUserClientCredentialsManager">Manages client credentials for back office users.</param>
    /// <param name="authorizationService">Performs authorization and permission checks.</param>
    public DeleteClientCredentialsUserController(
        IBackOfficeUserClientCredentialsManager backOfficeUserClientCredentialsManager,
        IAuthorizationService authorizationService)
    {
        _backOfficeUserClientCredentialsManager = backOfficeUserClientCredentialsManager;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Deletes the client credentials associated with the specified client ID for a given user.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the user whose client credentials are to be deleted.</param>
    /// <param name="clientId">The client ID of the credentials to delete.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the delete operation.</returns>
    [HttpDelete("{id:guid}/client-credentials/{clientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Deletes client credentials for a user.")]
    [EndpointDescription("Deletes client credentials identified by the provided client Id for a user.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id, string clientId)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(id),
            AuthorizationPolicies.UserPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<BackOfficeUserClientCredentialsOperationStatus> result = await _backOfficeUserClientCredentialsManager.DeleteAsync(id, clientId);
        return result.Success
            ? Ok()
            : BackOfficeUserClientCredentialsOperationStatusResult(result.Result);
    }
}

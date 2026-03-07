using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.User;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User;

    /// <summary>
    /// API controller responsible for handling bulk deletion operations for users in the management section.
    /// </summary>
[ApiVersion("1.0")]
public class BulkDeleteUserController : UserControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserService _userService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="BulkDeleteUserController"/> class.
    /// </summary>
    /// <param name="authorizationService">An instance of <see cref="IAuthorizationService"/> used to authorize user actions.</param>
    /// <param name="userService">An instance of <see cref="IUserService"/> for managing user-related operations.</param>
    /// <param name="backOfficeSecurityAccessor">An accessor for <see cref="IBackOfficeSecurityAccessor"/> providing back office security context.</param>
    public BulkDeleteUserController(
        IAuthorizationService authorizationService,
        IUserService userService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _userService = userService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Deletes multiple users specified by their unique identifiers.
    /// This operation is irreversible.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="model">The request model containing the collection of user IDs to delete.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the outcome of the delete operation: <c>Ok</c> if successful, or an error result if the operation fails.
    /// </returns>
    [HttpDelete]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Deletes multiple users.")]
    [EndpointDescription("Deletes multiple users identified by the provided Ids. This operation cannot be undone.")]
    public async Task<IActionResult> DeleteUsers(CancellationToken cancellationToken, DeleteUsersRequestModel model)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(model.UserIds.Select(x => x.Id)),
            AuthorizationPolicies.UserPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        UserOperationStatus result = await _userService.DeleteAsync(CurrentUserKey(_backOfficeSecurityAccessor), model.UserIds.Select(x => x.Id).ToHashSet());

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}

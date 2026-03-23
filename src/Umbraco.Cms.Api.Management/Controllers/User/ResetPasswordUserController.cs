using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User;

/// <summary>
/// Provides API endpoints for managing user password reset operations in the Umbraco backoffice.
/// </summary>
[ApiVersion("1.0")]
public class ResetPasswordUserController : UserControllerBase
{
    private readonly IUserService _userService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUmbracoMapper _mapper;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResetPasswordUserController"/> class, which handles user password reset operations in the management API.
    /// </summary>
    /// <param name="userService">Service for managing user-related operations.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="mapper">The Umbraco object mapper used for model transformations.</param>
    /// <param name="authorizationService">Service for handling authorization checks.</param>
    public ResetPasswordUserController(
        IUserService userService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUmbracoMapper mapper,
        IAuthorizationService authorizationService)
    {
        _userService = userService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _mapper = mapper;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Handles an HTTP POST request to reset the password for the user with the specified unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the user whose password is to be reset.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that returns:
    /// <list type="bullet">
    /// <item><description><see cref="ResetPasswordUserResponseModel"/> with status 200 (OK) if successful.</description></item>
    /// <item><description><see cref="ProblemDetails"/> with status 400 (Bad Request) or 404 (Not Found) if the operation fails.</description></item>
    /// </list>
    /// </returns>
    [HttpPost("{id:guid}/reset-password")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ResetPasswordUserResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Resets a user's password.")]
    [EndpointDescription("Resets the password for the user using the provided reset token.")]
    public async Task<IActionResult> ResetPassword(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(id),
            AuthorizationPolicies.UserPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<PasswordChangedModel, UserOperationStatus> response = await _userService.ResetPasswordAsync(CurrentUserKey(_backOfficeSecurityAccessor), id);

        return response.Success
            ? Ok(_mapper.Map<ResetPasswordUserResponseModel>(response.Result))
            : UserOperationStatusResult(response.Status, response.Result);
    }
}

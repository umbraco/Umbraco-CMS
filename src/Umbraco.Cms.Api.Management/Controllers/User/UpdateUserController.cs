using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User;

/// <summary>
/// API controller responsible for handling operations related to updating user information in the management system.
/// </summary>
[ApiVersion("1.0")]
public class UpdateUserController : UserControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserPresentationFactory _userPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserController"/> class, which manages user update operations in the Umbraco backoffice API.
    /// </summary>
    /// <param name="userService">Service for managing user data and operations.</param>
    /// <param name="userPresentationFactory">Factory for creating user presentation models.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="authorizationService">Service for handling authorization checks.</param>
    public UpdateUserController(
        IUserService userService,
        IUserPresentationFactory userPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IAuthorizationService authorizationService)
    {
        _userService = userService;
        _userPresentationFactory = userPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Updates the specified user with new details provided in the request model.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the user to update.</param>
    /// <param name="model">The request model containing updated user information.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the update operation.</returns>
    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates a user.")]
    [EndpointDescription("Updates a user identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(CancellationToken cancellationToken, Guid id, UpdateUserRequestModel model)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(id),
            AuthorizationPolicies.UserPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        // We have to use an intermediate save model, and cannot map it directly to an IUserModel
        // This is because we need to compare the updated values with what the user already has, for audit purposes.
        UserUpdateModel updateModel = await _userPresentationFactory.CreateUpdateModelAsync(id, model);

        Attempt<IUser?, UserOperationStatus> result = await _userService.UpdateAsync(CurrentUserKey(_backOfficeSecurityAccessor), updateModel);

        return result.Success
            ? Ok()
            : UserOperationStatusResult(result.Status);
    }
}

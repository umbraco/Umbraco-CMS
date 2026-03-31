using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

/// <summary>
/// Controller responsible for update information about the currently authenticated user.
/// </summary>
[ApiVersion("1.0")]
public class UpdateCurrentUserProfileController : CurrentUserControllerBase
{
    private readonly IUserService _userService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserPresentationFactory _userPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCurrentUserProfileController"/> class, which manages user update operations in the Umbraco backoffice API.
    /// </summary>
    /// <param name="userService">Service for managing user data and operations.</param>
    /// <param name="userPresentationFactory">Factory for creating user presentation models.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public UpdateCurrentUserProfileController(
        IUserService userService,
        IUserPresentationFactory userPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _userService = userService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userPresentationFactory = userPresentationFactory;
    }

    /// <summary>
    /// Updates the current user with new details provided in the request model.
    /// </summary>
    /// <param name="model">The request model containing updated current user information.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the update operation.</returns>
    [HttpPut("profile")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates current user profile.")]
    [EndpointDescription("Updates current user profile with the details from the request model.")]
    public async Task<IActionResult> UpdateCurrentUser(UpdateCurrentUserRequestModel model)
    {
        Guid userKey = CurrentUserKey(_backOfficeSecurityAccessor);

        UserUpdateProfileModel updateModel = await _userPresentationFactory.CreateUpdateCurrentUserModelAsync(model);
        Attempt<IUser?, UserOperationStatus> result = await _userService.UpdateProfileAsync(userKey, updateModel);

        return result.Success
            ? Ok()
            : UserOperationStatusResult(result.Status);
    }
}

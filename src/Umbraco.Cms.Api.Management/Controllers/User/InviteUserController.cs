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

namespace Umbraco.Cms.Api.Management.Controllers.User;

/// <summary>
/// Provides API endpoints for inviting new users to the system.
/// Handles operations related to user invitations.
/// </summary>
[ApiVersion("1.0")]
public class InviteUserController : UserControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserPresentationFactory _userPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.User.InviteUserController"/> class.
    /// </summary>
    /// <param name="userService">Service for managing user-related operations.</param>
    /// <param name="userPresentationFactory">Factory for creating user presentation models.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public InviteUserController(
        IUserService userService,
        IUserPresentationFactory userPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _userService = userService;
        _userPresentationFactory = userPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }


    /// <summary>
    /// Sends an invitation email to a new user, allowing them to create an account with the specified details.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="model">The request model containing details of the user to invite.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the invitation operation.</returns>
    [HttpPost("invite")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Invites new users.")]
    [EndpointDescription("Sends invitation emails to create new user accounts with the specified details.")]
    public async Task<IActionResult> Invite(CancellationToken cancellationToken, InviteUserRequestModel model)
    {
        UserInviteModel userInvite = await _userPresentationFactory.CreateInviteModelAsync(model);

        Attempt<UserInvitationResult, UserOperationStatus> result = await _userService.InviteAsync(CurrentUserKey(_backOfficeSecurityAccessor), userInvite);

        return result.Success
            ? CreatedAtId<ByKeyUserController>(controller => nameof(controller.ByKey), result.Result.InvitedUser!.Key)
            : UserOperationStatusResult(result.Status, result.Result);
    }
}

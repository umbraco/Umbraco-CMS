using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup;

/// <summary>
/// API controller responsible for handling requests to update user groups in the system.
/// </summary>
[ApiVersion("1.0")]
public class UpdateUserGroupController : UserGroupControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IUserGroupPresentationFactory _userGroupPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserGroupController"/> class, which manages update operations for user groups.
    /// </summary>
    /// <param name="userGroupService">Service for managing user group data and operations.</param>
    /// <param name="userGroupPresentationFactory">Factory for creating user group presentation models.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context and operations.</param>
    public UpdateUserGroupController(
        IUserGroupService userGroupService,
        IUserGroupPresentationFactory userGroupPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _userGroupService = userGroupService;
        _userGroupPresentationFactory = userGroupPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Updates the specified user group with new details.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the user group to update.</param>
    /// <param name="updateUserGroupRequestModel">The model containing the updated user group details.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates a user group.")]
    [EndpointDescription("Updates a user group identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        UpdateUserGroupRequestModel updateUserGroupRequestModel)
    {
        IUserGroup? existingUserGroup = await _userGroupService.GetAsync(id);

        if (existingUserGroup is null)
        {
            return UserGroupOperationStatusResult(UserGroupOperationStatus.NotFound);
        }

        Attempt<IUserGroup, UserGroupOperationStatus> userGroupUpdateAttempt = await _userGroupPresentationFactory.UpdateAsync(existingUserGroup, updateUserGroupRequestModel);
        if (userGroupUpdateAttempt.Success is false)
        {
            return UserGroupOperationStatusResult(userGroupUpdateAttempt.Status);
        }

        IUserGroup userGroup = userGroupUpdateAttempt.Result;
        Attempt<IUserGroup, UserGroupOperationStatus> result = await _userGroupService.UpdateAsync(userGroup, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : UserGroupOperationStatusResult(result.Status);
    }
}

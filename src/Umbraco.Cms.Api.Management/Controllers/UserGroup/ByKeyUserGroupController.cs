using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup;

/// <summary>
/// Controller for managing user groups identified by their unique key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyUserGroupController : UserGroupControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserGroupService _userGroupService;
    private readonly IUserGroupPresentationFactory _userGroupPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyUserGroupController"/> class with the specified services.
    /// </summary>
    /// <param name="authorizationService">Service used to perform authorization and permission checks.</param>
    /// <param name="userGroupService">Service for managing user groups.</param>
    /// <param name="userGroupPresentationFactory">Factory for creating user group presentation models.</param>
    public ByKeyUserGroupController(
        IAuthorizationService authorizationService,
        IUserGroupService userGroupService,
        IUserGroupPresentationFactory userGroupPresentationFactory)
    {
        _authorizationService = authorizationService;
        _userGroupService = userGroupService;
        _userGroupPresentationFactory = userGroupPresentationFactory;
    }

    /// <summary>
    /// Retrieves a user group by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the user group to retrieve.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="UserGroupResponseModel"/> with status 200 (OK) if found;
    /// status 404 (Not Found) if the user group does not exist; or status 403 (Forbidden) if the user is not authorized.
    /// </returns>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UserGroupResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a user group.")]
    [EndpointDescription("Gets a user group identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserGroupPermissionResource.WithKeys(id),
            AuthorizationPolicies.UserBelongsToUserGroupInRequest);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        IUserGroup? userGroup = await _userGroupService.GetAsync(id);

        if (userGroup is null)
        {
            return UserGroupNotFound();
        }

        return Ok(await _userGroupPresentationFactory.CreateAsync(userGroup));
    }
}

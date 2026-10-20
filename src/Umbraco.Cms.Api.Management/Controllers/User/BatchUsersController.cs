using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User;

/// <summary>
/// Provides an API controller for retrieving the full details for multiple users by key.
/// </summary>
[ApiVersion("1.0")]
public class BatchUsersController : UserControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserService _userService;
    private readonly IUserPresentationFactory _userPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchUsersController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize user management actions.</param>
    /// <param name="userService">Service for performing user-related operations.</param>
    /// <param name="userPresentationFactory">Factory for creating user presentation models.</param>
    public BatchUsersController(
        IAuthorizationService authorizationService,
        IUserService userService,
        IUserPresentationFactory userPresentationFactory)
    {
        _authorizationService = authorizationService;
        _userService = userService;
        _userPresentationFactory = userPresentationFactory;
    }

    [HttpGet("batch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(BatchResponseModel<UserResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets multiple users.")]
    [EndpointDescription("Gets multiple users identified by the provided Ids.")]
    public async Task<IActionResult> Batch(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        Guid[] requestedIds = [.. ids];

        if (requestedIds.Length == 0)
        {
            return Ok(new BatchResponseModel<UserResponseModel>());
        }

        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(requestedIds),
            AuthorizationPolicies.UserPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        IEnumerable<IUser> users = await _userService.GetAsync(requestedIds);

        List<IUser> ordered = OrderByRequestedIds(users, requestedIds);

        var responseModels = ordered.Select(_userPresentationFactory.CreateResponseModel).ToList();

        return Ok(new BatchResponseModel<UserResponseModel>
        {
            Total = responseModels.Count,
            Items = responseModels,
        });
    }
}

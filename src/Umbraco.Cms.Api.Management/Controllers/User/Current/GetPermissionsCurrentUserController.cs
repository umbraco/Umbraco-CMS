using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

/// <summary>
/// Controller for retrieving the permissions of the current user.
/// </summary>
[ApiVersion("1.0")]
public class GetPermissionsCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserService _userService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPermissionsCurrentUserController"/> class, used to manage and retrieve the current user's permissions.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features.</param>
    /// <param name="userService">Service for managing and retrieving user information.</param>
    /// <param name="mapper">The Umbraco object mapper used for mapping entities to models.</param>
    public GetPermissionsCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserService userService,
        IUmbracoMapper mapper)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userService = userService;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves the permissions for the currently authenticated user for the specified set of entity IDs.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="ids">A set of entity IDs (as <see cref="Guid"/>) for which to retrieve permissions.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="UserPermissionsResponseModel"/> with the user's permissions for the requested entities, or an error result if the operation fails.
    /// </returns>
    [MapToApiVersion("1.0")]
    [HttpGet("permissions")]
    [ProducesResponseType(typeof(UserPermissionsResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets permissions for the current user.")]
    [EndpointDescription("Gets the permissions for the currently authenticated user.")]
    public async Task<IActionResult> GetPermissions(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        Attempt<IEnumerable<NodePermissions>, UserOperationStatus> permissionsAttempt = await _userService.GetPermissionsAsync(CurrentUserKey(_backOfficeSecurityAccessor), ids.ToArray());

        if (permissionsAttempt.Success is false)
        {
            return UserOperationStatusResult(permissionsAttempt.Status);
        }

        List<UserPermissionViewModel> viewmodels = _mapper.MapEnumerable<NodePermissions, UserPermissionViewModel>(permissionsAttempt.Result);

        return Ok(new UserPermissionsResponseModel { Permissions = viewmodels });
    }
}

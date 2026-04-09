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
/// Provides an API controller for retrieving the current user's media permissions.
/// </summary>
[ApiVersion("1.0")]
public class GetMediaPermissionsCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserService _userService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMediaPermissionsCurrentUserController"/> class, which handles requests for retrieving media permissions for the current user.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security information for the current user.</param>
    /// <param name="userService">Service used to manage and retrieve user information.</param>
    /// <param name="mapper">The Umbraco object mapper used for mapping between domain and API models.</param>
    public GetMediaPermissionsCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserService userService,
        IUmbracoMapper mapper)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userService = userService;
        _mapper = mapper;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("permissions/media")]
    [ProducesResponseType(typeof(UserPermissionsResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets media permissions for the current user.")]
    [EndpointDescription("Gets the media permissions for the currently authenticated user.")]
    public async Task<IActionResult> GetPermissions(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        Attempt<IEnumerable<NodePermissions>, UserOperationStatus> permissionsAttempt = await _userService.GetMediaPermissionsAsync(CurrentUserKey(_backOfficeSecurityAccessor), ids);

        if (permissionsAttempt.Success is false)
        {
            return UserOperationStatusResult(permissionsAttempt.Status);
        }

        List<UserPermissionViewModel> viewModels = _mapper.MapEnumerable<NodePermissions, UserPermissionViewModel>(permissionsAttempt.Result);

        return Ok(new UserPermissionsResponseModel { Permissions = viewModels });
    }
}

using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

/// <summary>
/// Provides endpoints to retrieve element permissions for the current user.
/// </summary>
[ApiVersion("1.0")]
public class GetElementPermissionsCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUmbracoMapper _mapper;
    private readonly IElementPermissionService _elementPermissionService;

    // TODO (V20): Remove the IUserService parameter from the constructor as it is not used in the current implementation.

    /// <summary>
    /// Initializes a new instance of the <see cref="GetElementPermissionsCurrentUserController"/> class, which handles requests related to retrieving element permissions for the current user.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security information for the current user.</param>
    /// <param name="userService">Service for managing and retrieving user information.</param>
    /// <param name="mapper">The Umbraco object mapper used for mapping between models.</param>
    /// <param name="elementPermissionService">Service for managing element permissions.</param>
    [ActivatorUtilitiesConstructor]
    public GetElementPermissionsCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
#pragma warning disable IDE0060 // Remove unused parameter
        IUserService userService,
#pragma warning restore IDE0060 // Remove unused parameter
        IUmbracoMapper mapper,
        IElementPermissionService elementPermissionService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _mapper = mapper;
        _elementPermissionService = elementPermissionService;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GetElementPermissionsCurrentUserController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security information for the current user.</param>
    /// <param name="userService">Service for managing and retrieving user information.</param>
    /// <param name="mapper">The Umbraco object mapper used for mapping between models.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 20.")]
    public GetElementPermissionsCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserService userService,
        IUmbracoMapper mapper)
        : this(
            backOfficeSecurityAccessor,
            userService,
            mapper,
            StaticServiceProvider.Instance.GetRequiredService<IElementPermissionService>())
    {
    }

    /// <summary>
    /// Retrieves the element permissions for the currently authenticated user for the specified element IDs.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="ids">A set of element IDs for which to retrieve permissions.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="UserPermissionsResponseModel"/> with the permissions for each requested element.</returns>
    [MapToApiVersion("1.0")]
    [HttpGet("permissions/element")]
    [ProducesResponseType(typeof(UserPermissionsResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets element permissions for the current user.")]
    [EndpointDescription("Gets the element permissions for the currently authenticated user.")]
    public async Task<IActionResult> GetPermissions(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        IUser currentUser = CurrentUser(_backOfficeSecurityAccessor);

        // Resolve permissions through IElementPermissionService so custom implementations are respected.
        NodePermissions[] permissions = (await _elementPermissionService.GetPermissionsAsync(currentUser, ids)).ToArray();

        // Preserve 404 behavior: if any requested ID was not found, return ElementNodeNotFound.
        if (ids.Count > 0 && permissions.Length < ids.Count)
        {
            return UserOperationStatusResult(UserOperationStatus.ElementNodeNotFound);
        }

        List<UserPermissionViewModel> viewModels = _mapper.MapEnumerable<NodePermissions, UserPermissionViewModel>(permissions);

        return Ok(new UserPermissionsResponseModel { Permissions = viewModels });
    }
}

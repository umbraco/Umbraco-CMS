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
/// Provides endpoints to retrieve document permissions for the current user.
/// </summary>
[ApiVersion("1.0")]
public class GetDocumentPermissionsCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUmbracoMapper _mapper;
    private readonly IContentPermissionService _contentPermissionService;

    // TODO (V19): Remove the IUserService parameter from the constructor as it is not used in the current implementation.

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDocumentPermissionsCurrentUserController"/> class, which handles requests related to retrieving document permissions for the current user.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security information for the current user.</param>
    /// <param name="mapper">The Umbraco object mapper used for mapping between models.</param>
    /// <param name="contentPermissionService">Service for managing content permissions.</param>
    [ActivatorUtilitiesConstructor]
    public GetDocumentPermissionsCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserService userService,
        IUmbracoMapper mapper,
        IContentPermissionService contentPermissionService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _mapper = mapper;
        _contentPermissionService = contentPermissionService;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDocumentPermissionsCurrentUserController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security information for the current user.</param>
    /// <param name="userService">Service for managing and retrieving user information.</param>
    /// <param name="mapper">The Umbraco object mapper used for mapping between models.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public GetDocumentPermissionsCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserService userService,
        IUmbracoMapper mapper)
        : this(
            backOfficeSecurityAccessor,
            userService,
            mapper,
            StaticServiceProvider.Instance.GetRequiredService<IContentPermissionService>())
    {
    }

    /// <summary>
    /// Retrieves the document permissions for the currently authenticated user for the specified document IDs.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="ids">A set of document IDs for which to retrieve permissions.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="UserPermissionsResponseModel"/> with the permissions for each requested document.</returns>
    [MapToApiVersion("1.0")]
    [HttpGet("permissions/document")]
    [ProducesResponseType(typeof(UserPermissionsResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets document permissions for the current user.")]
    [EndpointDescription("Gets the document permissions for the currently authenticated user.")]
    public async Task<IActionResult> GetPermissions(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        IUser currentUser = CurrentUser(_backOfficeSecurityAccessor);
        NodePermissions[] permissions = (await _contentPermissionService.GetPermissionsAsync(currentUser, ids)).ToArray();

        // Preserve 404 behavior: if any requested ID was not found, return ContentNodeNotFound.
        if (ids.Count > 0 && permissions.Length < ids.Count)
        {
            return UserOperationStatusResult(UserOperationStatus.ContentNodeNotFound);
        }

        List<UserPermissionViewModel> viewModels = _mapper.MapEnumerable<NodePermissions, UserPermissionViewModel>(permissions);

        return Ok(new UserPermissionsResponseModel { Permissions = viewModels });
    }
}

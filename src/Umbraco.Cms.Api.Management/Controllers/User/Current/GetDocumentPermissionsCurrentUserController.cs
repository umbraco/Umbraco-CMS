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
/// Provides endpoints to retrieve document permissions for the current user.
/// </summary>
[ApiVersion("1.0")]
public class GetDocumentPermissionsCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserService _userService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDocumentPermissionsCurrentUserController"/> class, which handles requests related to retrieving document permissions for the current user.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security information for the current user.</param>
    /// <param name="userService">Service for managing and retrieving user information.</param>
    /// <param name="mapper">The Umbraco object mapper used for mapping between models.</param>
    public GetDocumentPermissionsCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserService userService,
        IUmbracoMapper mapper)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userService = userService;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves the document permissions for the currently authenticated user for the specified document IDs.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="ids">A set of document IDs for which to retrieve permissions.</param>
    /// <returns>An <see cref="IActionResult"/> containing a <see cref="UserPermissionsResponseModel"/> with the permissions for each requested document, or a <see cref="ProblemDetails"/> if not found.</returns>
    [MapToApiVersion("1.0")]
    [HttpGet("permissions/document")]
    [ProducesResponseType(typeof(IEnumerable<UserPermissionsResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets document permissions for the current user.")]
    [EndpointDescription("Gets the document permissions for the currently authenticated user.")]
    public async Task<IActionResult> GetPermissions(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        Attempt<IEnumerable<NodePermissions>, UserOperationStatus> permissionsAttempt = await _userService.GetDocumentPermissionsAsync(CurrentUserKey(_backOfficeSecurityAccessor), ids);

        if (permissionsAttempt.Success is false)
        {
            return UserOperationStatusResult(permissionsAttempt.Status);
        }

        List<UserPermissionViewModel> viewModels = _mapper.MapEnumerable<NodePermissions, UserPermissionViewModel>(permissionsAttempt.Result);

        return Ok(new UserPermissionsResponseModel { Permissions = viewModels });
    }
}

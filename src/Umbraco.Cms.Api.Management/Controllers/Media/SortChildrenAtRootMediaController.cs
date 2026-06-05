using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Sorting;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

/// <summary>
/// Provides an API endpoint for sorting the root-level media items by a system field.
/// </summary>
[ApiVersion("1.0")]
public class SortChildrenAtRootMediaController : MediaControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediaEditingService _mediaEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="SortChildrenAtRootMediaController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize user actions.</param>
    /// <param name="mediaEditingService">Service responsible for editing and sorting media items.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for backoffice security context.</param>
    public SortChildrenAtRootMediaController(
        IAuthorizationService authorizationService,
        IMediaEditingService mediaEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _mediaEditingService = mediaEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Sorts the root-level media items by a system field.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="requestModel">The field to sort by and the sort direction.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the outcome of the operation:
    /// returns <c>200 OK</c> if sorting succeeds or <c>400 Bad Request</c> if the field is not recognised.
    /// </returns>
    [HttpPut("root/sort-children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Sorts the root-level media items by a field.")]
    [EndpointDescription("Sorts the root-level media items by a system field in the given direction.")]
    public async Task<IActionResult> SortChildren(CancellationToken cancellationToken, SortMediaChildrenByFieldRequestModel requestModel)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            MediaPermissionResource.Root(),
            AuthorizationPolicies.MediaPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        ContentEditingOperationStatus result = await _mediaEditingService.SortByFieldAsync(
            null,
            requestModel.Field,
            requestModel.Direction,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result == ContentEditingOperationStatus.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result);
    }
}

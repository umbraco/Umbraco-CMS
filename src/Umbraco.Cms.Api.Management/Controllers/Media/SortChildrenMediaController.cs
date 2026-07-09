using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization;
using Umbraco.Cms.Api.Management.ViewModels.Sorting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

/// <summary>
/// Provides an API endpoint for sorting the children of a media item by a system field.
/// </summary>
[ApiVersion("1.0")]
public class SortChildrenMediaController : MediaControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediaEditingService _mediaEditingService;
    private readonly IEntityService _entityService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="SortChildrenMediaController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize user actions.</param>
    /// <param name="mediaEditingService">Service responsible for editing and sorting media items.</param>
    /// <param name="entityService">Service used to resolve the children to authorize.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for backoffice security context.</param>
    public SortChildrenMediaController(
        IAuthorizationService authorizationService,
        IMediaEditingService mediaEditingService,
        IEntityService entityService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _mediaEditingService = mediaEditingService;
        _entityService = entityService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Sorts the child media items of the specified parent media item by a system field.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="id">The unique identifier of the parent media item whose children should be sorted.</param>
    /// <param name="requestModel">The field to sort by and the sort direction.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the outcome of the operation:
    /// returns <c>200 OK</c> if sorting succeeds, <c>400 Bad Request</c> if the field is not recognised, or <c>404 Not Found</c> if the parent media item does not exist.
    /// </returns>
    [HttpPut("{id:guid}/sort-children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Sorts the children of a media item by a field.")]
    [EndpointDescription("Sorts the children of the specified parent media item by a system field in the given direction.")]
    public async Task<IActionResult> SortChildren(CancellationToken cancellationToken, Guid id, SortMediaChildrenByFieldRequestModel requestModel)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            MediaPermissionResource.WithKeys(id),
            AuthorizationPolicies.MediaPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        var childrenAuthorized = await AllChildrenAuthorizer.IsAuthorizedForChildrenAsync(
            _authorizationService,
            _entityService,
            User,
            id,
            UmbracoObjectTypes.Media,
            childKeys => MediaPermissionResource.WithKeys(childKeys),
            AuthorizationPolicies.MediaPermissionByResource);

        if (!childrenAuthorized)
        {
            return Forbidden();
        }

        ContentEditingOperationStatus result = await _mediaEditingService.SortByFieldAsync(
            id,
            requestModel.Field,
            requestModel.Direction,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result == ContentEditingOperationStatus.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result);
    }
}

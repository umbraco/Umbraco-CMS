using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization;
using Umbraco.Cms.Api.Management.ViewModels.Sorting;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Provides an API endpoint for sorting the children of a document by a system field.
/// </summary>
[ApiVersion("1.0")]
public class SortChildrenDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentEditingService _contentEditingService;
    private readonly IEntityService _entityService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="SortChildrenDocumentController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize user actions.</param>
    /// <param name="contentEditingService">Service for editing and managing content.</param>
    /// <param name="entityService">Service used to resolve the children to authorize.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public SortChildrenDocumentController(
        IAuthorizationService authorizationService,
        IContentEditingService contentEditingService,
        IEntityService entityService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _contentEditingService = contentEditingService;
        _entityService = entityService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Sorts the child documents of the specified parent document by a system field.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="id">The unique identifier of the parent document whose children should be sorted.</param>
    /// <param name="requestModel">The field to sort by and the sort direction.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the outcome of the operation:
    /// returns <c>200 OK</c> if sorting succeeds, <c>400 Bad Request</c> if the field is not recognised, or <c>404 Not Found</c> if the parent document does not exist.
    /// </returns>
    [HttpPut("{id:guid}/sort-children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Sorts the children of a document by a field.")]
    [EndpointDescription("Sorts the children of the specified parent document by a system field in the given direction. When sorting by name, an optional culture selects the variant name; the culture is not validated, so children that do not vary by it (or an unrecognised culture) fall back to the invariant name.")]
    public async Task<IActionResult> SortChildren(CancellationToken cancellationToken, Guid id, SortDocumentChildrenByFieldRequestModel requestModel)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionSort.ActionLetter, id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        var childrenAuthorized = await SortChildrenAuthorizer.IsAuthorizedForChildrenAsync(
            _authorizationService,
            _entityService,
            User,
            id,
            UmbracoObjectTypes.Document,
            childKeys => ContentPermissionResource.WithKeys(ActionSort.ActionLetter, childKeys),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!childrenAuthorized)
        {
            return Forbidden();
        }

        ContentEditingOperationStatus result = await _contentEditingService.SortByFieldAsync(
            id,
            requestModel.Field,
            requestModel.Direction,
            requestModel.Culture,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result == ContentEditingOperationStatus.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result);
    }
}

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Api.Management.ViewModels.Sorting;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Provides API endpoints for sorting documents within the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class SortDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentEditingService _contentEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="SortDocumentController"/> class, which provides API endpoints for sorting documents in Umbraco.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize user actions.</param>
    /// <param name="contentEditingService">Service for editing and managing content.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public SortDocumentController(
        IAuthorizationService authorizationService,
        IContentEditingService contentEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _contentEditingService = contentEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Sorts the child documents within a specified parent document according to the provided sort order.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="sortingRequestModel">A <see cref="SortingRequestModel"/> containing the parent document identifier and the desired sort order for its children.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the outcome of the operation:
    /// returns <c>200 OK</c> if sorting succeeds, <c>400 Bad Request</c> if the request is invalid, or <c>404 Not Found</c> if the parent document does not exist.
    /// </returns>
    [HttpPut("sort")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Sorts documents.")]
    [EndpointDescription("Sorts documents in the specified parent container according to the provided sort order.")]
    public async Task<IActionResult> Sort(CancellationToken cancellationToken, SortingRequestModel sortingRequestModel)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionSort.ActionLetter,  new List<Guid?>(sortingRequestModel.Sorting.Select(x => x.Id).Cast<Guid?>()) { sortingRequestModel.Parent?.Id }),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        ContentEditingOperationStatus result = await _contentEditingService.SortAsync(
            sortingRequestModel.Parent?.Id,
            sortingRequestModel.Sorting.Select(m => new SortingModel { Key = m.Id, SortOrder = m.SortOrder }),
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result == ContentEditingOperationStatus.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result);
    }
}

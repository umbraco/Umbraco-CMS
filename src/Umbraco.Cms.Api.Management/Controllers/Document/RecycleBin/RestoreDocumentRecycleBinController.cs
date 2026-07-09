using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document.RecycleBin;

/// <summary>
/// Provides API endpoints for restoring documents that have been moved to the recycle bin.
/// </summary>
[ApiVersion("1.0")]
public class RestoreDocumentRecycleBinController : DocumentRecycleBinControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentEditingService _contentEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestoreDocumentRecycleBinController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize user actions.</param>
    /// <param name="contentEditingService">Service for editing and managing content items.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context and user information.</param>
    /// <param name="entityService">Service for managing and retrieving entities.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    public RestoreDocumentRecycleBinController(
        IAuthorizationService authorizationService,
        IContentEditingService contentEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IEntityService entityService,
        IDocumentPresentationFactory documentPresentationFactory)
        : base(entityService,documentPresentationFactory)
    {
        _authorizationService = authorizationService;
        _contentEditingService = contentEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Restores a document from the recycle bin, either to its original location or to a new parent specified in the request.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the document to restore from the recycle bin.</param>
    /// <param name="moveDocumentRequestModel">A model containing information about the target parent location for the restored document. If null or not specified, the document is restored to its original location.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the restore operation:
    /// <list type="bullet">
    /// <item><description><c>200 OK</c> if the document was restored successfully.</description></item>
    /// <item><description><c>404 Not Found</c> if the document does not exist in the recycle bin.</description></item>
    /// <item><description><c>400 Bad Request</c> if the request is invalid.</description></item>
    /// </list>
    /// </returns>
    [HttpPut("{id:guid}/restore")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Restores a document from the recycle bin.")]
    [EndpointDescription("Restores a document from the recycle bin to its original location or a specified parent.")]
    public async Task<IActionResult> Restore(CancellationToken cancellationToken, Guid id, MoveMediaRequestModel moveDocumentRequestModel)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.RecycleBin(ActionMove.ActionLetter),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<IContent?, ContentEditingOperationStatus> result = await _contentEditingService.RestoreAsync(
            id,
            moveDocumentRequestModel.Target?.Id,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result.Status);
    }
}

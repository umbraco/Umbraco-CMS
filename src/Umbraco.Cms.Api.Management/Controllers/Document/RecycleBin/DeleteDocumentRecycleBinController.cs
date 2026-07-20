using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
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
/// Controller responsible for handling requests to delete items from the document recycle bin.
/// Provides endpoints for permanently removing documents that have been moved to the recycle bin.
/// </summary>
[ApiVersion("1.0")]
public class DeleteDocumentRecycleBinController : DocumentRecycleBinControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IContentEditingService _contentEditingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteDocumentRecycleBinController"/> class, which handles operations related to deleting items from the document recycle bin.
    /// </summary>
    /// <param name="entityService">The service used for entity operations.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    /// <param name="authorizationService">Service for handling authorization checks.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="contentEditingService">Service for editing content.</param>
    public DeleteDocumentRecycleBinController(
        IEntityService entityService,
        IDocumentPresentationFactory documentPresentationFactory,
        IAuthorizationService authorizationService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentEditingService contentEditingService)
        : base(entityService, documentPresentationFactory)
    {
        _authorizationService = authorizationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _contentEditingService = contentEditingService;
    }

    /// <summary>
    /// Permanently deletes a document from the recycle bin, identified by the provided ID.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="id">The unique identifier of the document to permanently delete from the recycle bin.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the delete operation.</returns>
    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a document.")]
    [EndpointDescription("Deletes a document identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.RecycleBin(ActionDelete.ActionLetter),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<IContent?, ContentEditingOperationStatus> result = await _contentEditingService.DeleteFromRecycleBinAsync(id, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result.Status);
    }
}

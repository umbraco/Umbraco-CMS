using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document.References;

/// <summary>
/// Controller responsible for handling requests related to documents that reference a specified document.
/// </summary>
[ApiVersion("1.0")]
public class ReferencedByDocumentController : DocumentControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesService;
    private readonly IRelationTypePresentationFactory _relationTypePresentationFactory;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferencedByDocumentController"/> class.
    /// </summary>
    /// <param name="trackedReferencesService">Service for tracking document references.</param>
    /// <param name="relationTypePresentationFactory">Factory for creating relation type presentation models.</param>
    /// <param name="authorizationService">The service used to authorize access to the requested items.</param>
    public ReferencedByDocumentController(
        ITrackedReferencesService trackedReferencesService,
        IRelationTypePresentationFactory relationTypePresentationFactory,
        IAuthorizationService authorizationService)
    {
        _trackedReferencesService = trackedReferencesService;
        _relationTypePresentationFactory = relationTypePresentationFactory;
        _authorizationService = authorizationService;
    }

    /// <summary>
    ///     Gets a paged list of tracked references for the specified document, so you can see where a document is being used.
    /// </summary>
    /// <remarks>
    ///     Used by info tabs on content, media, etc., and for the delete and unpublish operations of single items.
    ///     This essentially finds parent items in relations.
    /// </remarks>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document to retrieve references for.</param>
    /// <param name="skip">The number of items to skip when paginating results.</param>
    /// <param name="take">The maximum number of items to return in the paged result.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains an <see cref="IActionResult"/> with a paged list of items referencing the specified document.</returns>
    [HttpGet("{id:guid}/referenced-by")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<IReferenceResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a collection of items that reference documents.")]
    [EndpointDescription("Gets a paginated collection of items that reference the documents identified by the provided Ids.")]
    public async Task<IActionResult> ReferencedBy(

        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 20)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionBrowse.ActionLetter, id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus> relationItemsAttempt = await _trackedReferencesService.GetPagedRelationsForItemAsync(id, UmbracoObjectTypes.Document, skip, take, true);

        if (relationItemsAttempt.Success is false)
        {
            return GetReferencesOperationStatusResult(relationItemsAttempt.Status);
        }

        var pagedViewModel = new PagedViewModel<IReferenceResponseModel>
        {
            Total = relationItemsAttempt.Result.Total,
            Items = await _relationTypePresentationFactory.CreateReferenceResponseModelsAsync(relationItemsAttempt.Result.Items),
        };

        return Ok(pagedViewModel);
    }
}

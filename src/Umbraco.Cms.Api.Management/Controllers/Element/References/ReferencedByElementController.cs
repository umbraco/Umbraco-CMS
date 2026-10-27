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

namespace Umbraco.Cms.Api.Management.Controllers.Element.References;

/// <summary>
/// API controller responsible for retrieving tracked references for a specific element.
/// </summary>
[ApiVersion("1.0")]
public class ReferencedByElementController : ElementControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesService;
    private readonly IRelationTypePresentationFactory _relationTypePresentationFactory;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferencedByElementController"/> class.
    /// </summary>
    /// <param name="trackedReferencesService">Service for tracking content references.</param>
    /// <param name="relationTypePresentationFactory">Factory for creating relation type presentation models.</param>
    /// <param name="authorizationService">The service used to authorize access to the requested items.</param>
    public ReferencedByElementController(
        ITrackedReferencesService trackedReferencesService,
        IRelationTypePresentationFactory relationTypePresentationFactory,
        IAuthorizationService authorizationService)
    {
        _trackedReferencesService = trackedReferencesService;
        _relationTypePresentationFactory = relationTypePresentationFactory;
        _authorizationService = authorizationService;
    }

    /// <summary>
    ///     Gets a paged list of tracked references for the current item, so you can see where an element is being used.
    /// </summary>
    /// <remarks>
    ///     Used by info tabs on elements and for the delete and unpublish of single items.
    ///     This is basically finding parents of relations.
    /// </remarks>
    [HttpGet("{id:guid}/referenced-by")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<IReferenceResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets items referencing an element.")]
    [EndpointDescription("Gets a paginated collection of items that reference the element identified by the provided Id.")]
    public async Task<IActionResult> ReferencedBy(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 20)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementBrowse.ActionLetter, id),
            AuthorizationPolicies.ElementPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus> relationItemsAttempt =
            await _trackedReferencesService.GetPagedRelationsForItemAsync(
                id,
                UmbracoObjectTypes.Element,
                skip,
                take,
                true);

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

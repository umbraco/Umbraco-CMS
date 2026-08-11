using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.References;
using Umbraco.Cms.Api.Management.ViewModels.Element.References;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document.References;

/// <summary>
/// Controller responsible for retrieving the elements referenced by a document that are not fully published.
/// </summary>
[ApiVersion("1.0")]
public class ReferencedElementsWithPendingChangesDocumentController : DocumentControllerBase
{
    private readonly IReferencedElementsService _referencedElementsService;
    private readonly IElementPresentationFactory _elementPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferencedElementsWithPendingChangesDocumentController"/> class.
    /// </summary>
    /// <param name="referencedElementsService">Service used to look up referenced elements that are not fully published.</param>
    /// <param name="elementPresentationFactory">Factory used to map referenced elements to response models.</param>
    public ReferencedElementsWithPendingChangesDocumentController(
        IReferencedElementsService referencedElementsService,
        IElementPresentationFactory elementPresentationFactory)
    {
        _referencedElementsService = referencedElementsService;
        _elementPresentationFactory = elementPresentationFactory;
    }

    /// <summary>
    ///     Gets a paged list of the elements directly referenced by a document that are not fully published.
    /// </summary>
    /// <remarks>
    ///     Used to warn editors, when publishing or scheduling a document, that an element it references (via an
    ///     Element Picker property, or as embedded reusable block content) is a draft, has unpublished changes, or
    ///     is scheduled for a future publish.
    /// </remarks>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document to find referenced elements for.</param>
    /// <param name="skip">The number of items to skip for paging.</param>
    /// <param name="take">The maximum number of items to return for paging.</param>
    /// <returns>A paged list of the referenced elements that are not fully published.</returns>
    [HttpGet("{id:guid}/referenced-elements-with-pending-changes")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ReferencedElementWithPendingChangesResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets the elements referenced by a document that are not fully published.")]
    [EndpointDescription("Gets a paginated collection of elements directly referenced by the document identified by the provided Id, that are draft, have pending changes, or are scheduled for a future publish.")]
    public async Task<IActionResult> ReferencedElementsWithPendingChanges(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 20)
    {
        Attempt<PagedModel<ReferencedElementWithPendingChanges>, GetReferencesOperationStatus> attempt =
            await _referencedElementsService.GetPagedReferencedElementsWithPendingChangesAsync(
                id, UmbracoObjectTypes.Document, skip, take, cancellationToken);

        if (attempt.Success is false)
        {
            return GetReferencesOperationStatusResult(attempt.Status);
        }

        var pagedViewModel = new PagedViewModel<ReferencedElementWithPendingChangesResponseModel>
        {
            Total = attempt.Result.Total,
            Items = await Task.WhenAll(attempt.Result.Items.Select(
                _elementPresentationFactory.CreateReferencedElementWithPendingChangesResponseModelAsync)),
        };

        return Ok(pagedViewModel);
    }
}

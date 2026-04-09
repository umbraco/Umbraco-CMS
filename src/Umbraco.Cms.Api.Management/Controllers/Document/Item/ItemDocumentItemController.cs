using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Item;

/// <summary>
/// API controller responsible for handling operations related to item documents within the Umbraco CMS management area.
/// Provides endpoints for retrieving, updating, or managing document items.
/// </summary>
[ApiVersion("1.0")]
public class ItemDocumentItemController : DocumentItemControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Document.Item.ItemDocumentItemController"/> class.
    /// This controller is responsible for handling API requests related to individual document items in the Umbraco CMS.
    /// </summary>
    /// <param name="entityService">The service used to manage and retrieve entities within the CMS.</param>
    /// <param name="documentPresentationFactory">The factory responsible for creating document presentation models.</param>
    [ActivatorUtilitiesConstructor]
    public ItemDocumentItemController(
        IEntityService entityService,
        IDocumentPresentationFactory documentPresentationFactory)
    {
        _entityService = entityService;
        _documentPresentationFactory = documentPresentationFactory;
    }

    /// <summary>
    /// Gets a collection of document items identified by the provided Ids.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="ids">The set of document item Ids to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult with the collection of document items.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DocumentItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of document items.")]
    [EndpointDescription("Gets a collection of document items identified by the provided Ids.")]
    public async Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<DocumentItemResponseModel>());
        }

        IEnumerable<IDocumentEntitySlim> documents = _entityService
            .GetAll(UmbracoObjectTypes.Document, ids.ToArray())
            .OfType<IDocumentEntitySlim>();

        IEnumerable<DocumentItemResponseModel> responseModels = documents.Select(_documentPresentationFactory.CreateItemResponseModel);
        return Ok(responseModels);
    }
}

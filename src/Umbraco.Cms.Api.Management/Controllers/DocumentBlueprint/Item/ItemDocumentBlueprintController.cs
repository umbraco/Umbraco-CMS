using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Item;

/// <summary>
/// Provides API endpoints for managing document blueprints related to items.
/// </summary>
[ApiVersion("1.0")]
public class ItemDocumentBlueprintController : DocumentBlueprintItemControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemDocumentBlueprintController"/> class with the specified services.
    /// </summary>
    /// <param name="entityService">An <see cref="IEntityService"/> used for managing entities.</param>
    /// <param name="documentPresentationFactory">An <see cref="IDocumentPresentationFactory"/> used to create document presentations.</param>
    public ItemDocumentBlueprintController(IEntityService entityService, IDocumentPresentationFactory documentPresentationFactory)
    {
        _entityService = entityService;
        _documentPresentationFactory = documentPresentationFactory;
    }

    /// <summary>
    /// Retrieves a collection of document blueprint items matching the specified IDs.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <param name="ids">The unique identifiers of the document blueprint items to retrieve.</param>
    /// <returns>An <see cref="IActionResult"/> containing the matching document blueprint items.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DocumentBlueprintItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of document blueprint items.")]
    [EndpointDescription("Gets a collection of document blueprint items identified by the provided Ids.")]
    public Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Task.FromResult<IActionResult>(Ok(Enumerable.Empty<DocumentBlueprintItemResponseModel>()));
        }

        IEnumerable<IDocumentEntitySlim> documents = _entityService
            .GetAll(UmbracoObjectTypes.DocumentBlueprint, ids.ToArray())
            .Select(x => x as IDocumentEntitySlim)
            .WhereNotNull();
        IEnumerable<DocumentBlueprintItemResponseModel> responseModels = documents.Select(x => _documentPresentationFactory.CreateBlueprintItemResponseModel(x));
        return Task.FromResult<IActionResult>(Ok(responseModels));
    }
}

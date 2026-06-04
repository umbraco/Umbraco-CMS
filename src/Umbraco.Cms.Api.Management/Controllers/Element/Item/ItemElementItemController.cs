using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Item;

/// <summary>
/// API controller responsible for retrieving element items by their identifiers.
/// </summary>
[ApiVersion("1.0")]
public class ItemElementItemController : ElementItemControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IElementPresentationFactory _elementPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemElementItemController"/> class.
    /// </summary>
    /// <param name="entityService">Service for retrieving entity data.</param>
    /// <param name="elementPresentationFactory">Factory responsible for creating element presentation models.</param>
    public ItemElementItemController(
        IEntityService entityService,
        IElementPresentationFactory elementPresentationFactory)
    {
        _entityService = entityService;
        _elementPresentationFactory = elementPresentationFactory;
    }

    /// <summary>
    /// Gets a collection of element items identified by the provided identifiers.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="ids">The set of unique identifiers of the element items to retrieve.</param>
    /// <returns>An <see cref="IActionResult"/> containing the collection of element items.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ElementItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of element items.")]
    [EndpointDescription("Gets a collection of element items identified by the provided Ids.")]
    public async Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<ElementItemResponseModel>());
        }

        IEnumerable<IElementEntitySlim> elements = _entityService
            .GetAll(UmbracoObjectTypes.Element, ids.ToArray())
            .OfType<IElementEntitySlim>();

        IEnumerable<Task<ElementItemResponseModel>> tasks = elements.Select(_elementPresentationFactory.CreateItemResponseModelAsync);
        ElementItemResponseModel[] responseModels = await Task.WhenAll(tasks);
        return Ok(responseModels);
    }
}

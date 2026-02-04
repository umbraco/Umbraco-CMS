using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Item;

[ApiVersion("1.0")]
public class ItemElementItemController : ElementItemControllerBase
{
    private readonly IEntityService _entityService;
    private readonly IElementPresentationFactory _elementPresentationFactory;

    public ItemElementItemController(
        IEntityService entityService,
        IElementPresentationFactory elementPresentationFactory)
    {
        _entityService = entityService;
        _elementPresentationFactory = elementPresentationFactory;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ElementItemResponseModel>), StatusCodes.Status200OK)]
    public Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Task.FromResult<IActionResult>(Ok(Enumerable.Empty<ElementItemResponseModel>()));
        }

        IEnumerable<IElementEntitySlim> elements = _entityService
            .GetAll(UmbracoObjectTypes.Element, ids.ToArray())
            .OfType<IElementEntitySlim>();

        IEnumerable<ElementItemResponseModel> responseModels = elements.Select(_elementPresentationFactory.CreateItemResponseModel);
        return Task.FromResult<IActionResult>(Ok(responseModels));
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Api.Delivery.Controllers;

public class ByIdContentApiController : DeliveryApiControllerBase
{
    public ByIdContentApiController(IApiPublishedContentCache apiPublishedContentCache, IApiContentResponseBuilder apiContentResponseBuilderBuilder)
        : base(apiPublishedContentCache, apiContentResponseBuilderBuilder)
    {
    }

    /// <summary>
    ///     Gets a content item by id.
    /// </summary>
    /// <param name="id">The unique identifier of the content item.</param>
    /// <returns>The content item or not found result.</returns>
    [HttpGet("item/{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IApiContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ById(Guid id)
    {
        IPublishedContent? contentItem = ApiPublishedContentCache.GetById(id);

        if (contentItem is null)
        {
            return NotFound();
        }

        return await Task.FromResult(Ok(ApiContentResponseBuilder.Build(contentItem)));
    }
}

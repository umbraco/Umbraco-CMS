using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Controllers.Media;

[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class ByIdMediaApiController : MediaApiControllerBase
{
    public ByIdMediaApiController(
        IPublishedMediaCache publishedMediaCache,
        IApiMediaWithCropsResponseBuilder apiMediaWithCropsResponseBuilder)
        : base(publishedMediaCache, apiMediaWithCropsResponseBuilder)
    {
    }

    [HttpGet("item/{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IApiMediaWithCropsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Obsolete("Please use version 2 of this API. Will be removed in V15.")]
    public Task<IActionResult> ById(Guid id)
        => Task.FromResult(HandleRequest(id));

    /// <summary>
    ///     Gets a media item by id.
    /// </summary>
    /// <param name="id">The unique identifier of the media item.</param>
    /// <returns>The media item or not found result.</returns>
    [HttpGet("item/{id:guid}")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(IApiMediaWithCropsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> ByIdV20(Guid id)
        => Task.FromResult(HandleRequest(id));

    private IActionResult HandleRequest(Guid id)
    {
        IPublishedContent? media = PublishedMediaCache.GetById(id);

        if (media is null)
        {
            return NotFound();
        }

        return Ok(BuildApiMediaWithCrops(media));
    }
}

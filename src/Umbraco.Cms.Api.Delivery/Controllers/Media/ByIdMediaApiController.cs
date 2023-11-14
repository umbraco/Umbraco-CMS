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
    public ByIdMediaApiController(IPublishedSnapshotAccessor publishedSnapshotAccessor, IApiMediaWithCropsResponseBuilder apiMediaWithCropsResponseBuilder)
        : base(publishedSnapshotAccessor, apiMediaWithCropsResponseBuilder)
    {
    }

    [HttpGet("item/{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IApiMediaWithCropsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Obsolete("Please use version 2 of this API. Will be removed in V15.")]
    public async Task<IActionResult> ById(Guid id)
        => await HandleRequest(id);

    /// <summary>
    ///     Gets a media item by id.
    /// </summary>
    /// <param name="id">The unique identifier of the media item.</param>
    /// <returns>The media item or not found result.</returns>
    [HttpGet("item/{id:guid}")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(IApiMediaWithCropsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByIdV20(Guid id)
        => await HandleRequest(id);

    private async Task<IActionResult> HandleRequest(Guid id)
    {
        IPublishedContent? media = PublishedMediaCache.GetById(id);

        if (media is null)
        {
            return await Task.FromResult(NotFound());
        }

        return Ok(BuildApiMediaWithCrops(media));
    }
}

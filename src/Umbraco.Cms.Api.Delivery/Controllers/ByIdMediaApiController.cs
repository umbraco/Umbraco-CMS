using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Controllers;

[ApiVersion("1.0")]
public class ByIdMediaApiController : MediaApiControllerBase
{
    public ByIdMediaApiController(IPublishedSnapshotAccessor publishedSnapshotAccessor, IApiMediaWithCropsBuilder apiMediaWithCropsBuilder)
        : base(publishedSnapshotAccessor, apiMediaWithCropsBuilder)
    {
    }

    /// <summary>
    ///     Gets a media item by id.
    /// </summary>
    /// <param name="id">The unique identifier of the media item.</param>
    /// <returns>The media item or not found result.</returns>
    [HttpGet("item/{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiMediaWithCrops), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ById(Guid id)
    {
        IPublishedContent? media = PublishedMediaCache.GetById(id);
        if (media is null)
        {
            return await Task.FromResult(NotFound());
        }

        return Ok(BuildApiMediaWithCrops(media));
    }
}

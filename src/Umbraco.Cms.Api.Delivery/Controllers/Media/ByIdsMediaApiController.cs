using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.DeliveryApi;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Controllers.Media;

[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class ByIdsMediaApiController : MediaApiControllerBase
{
    public ByIdsMediaApiController(IPublishedMediaCache publishedMediaCache, IApiMediaWithCropsResponseBuilder apiMediaWithCropsResponseBuilder)
        : base(publishedMediaCache, apiMediaWithCropsResponseBuilder)
    {
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<IApiMediaWithCropsResponse>), StatusCodes.Status200OK)]
    [Obsolete("Please use version 2 of this API. Will be removed in V15.")]
    public Task<IActionResult> Item([FromQuery(Name = "id")] HashSet<Guid> ids)
        => Task.FromResult(HandleRequest(ids));

    /// <summary>
    ///     Gets media items by ids.
    /// </summary>
    /// <param name="ids">The unique identifiers of the media items to retrieve.</param>
    /// <returns>The media items.</returns>
    [HttpGet("items")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(IEnumerable<IApiMediaWithCropsResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> ItemsV20([FromQuery(Name = "id")] HashSet<Guid> ids)
        => Task.FromResult(HandleRequest(ids));

    private IActionResult HandleRequest(HashSet<Guid> ids)
    {
        IPublishedContent[] mediaItems = ids
            .Select(PublishedMediaCache.GetById)
            .WhereNotNull()
            .ToArray();

        IApiMediaWithCropsResponse[] apiMediaItems = mediaItems
            .Select(BuildApiMediaWithCrops)
            .ToArray();

        return Ok(apiMediaItems);
    }
}

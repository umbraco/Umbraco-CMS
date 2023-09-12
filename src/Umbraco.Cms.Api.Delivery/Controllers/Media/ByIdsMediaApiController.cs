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
public class ByIdsMediaApiController : MediaApiControllerBase
{
    public ByIdsMediaApiController(IPublishedSnapshotAccessor publishedSnapshotAccessor, IApiMediaWithCropsResponseBuilder apiMediaWithCropsResponseBuilder)
        : base(publishedSnapshotAccessor, apiMediaWithCropsResponseBuilder)
    {
    }

    /// <summary>
    ///     Gets media items by ids.
    /// </summary>
    /// <param name="ids">The unique identifiers of the media items to retrieve.</param>
    /// <returns>The media items.</returns>
    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<IApiMediaWithCropsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Item([FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        IPublishedContent[] mediaItems = ids
            .Select(PublishedMediaCache.GetById)
            .WhereNotNull()
            .ToArray();

        IApiMediaWithCropsResponse[] apiMediaItems = mediaItems
            .Select(BuildApiMediaWithCrops)
            .ToArray();

        return await Task.FromResult(Ok(apiMediaItems));
    }
}

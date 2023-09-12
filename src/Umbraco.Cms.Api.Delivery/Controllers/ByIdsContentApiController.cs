using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Controllers;

[ApiVersion("1.0")]
public class ByIdsContentApiController : ContentApiItemControllerBase
{
    public ByIdsContentApiController(
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentResponseBuilder apiContentResponseBuilder,
        IPublicAccessService publicAccessService)
        : base(apiPublishedContentCache, apiContentResponseBuilder, publicAccessService)
    {
    }

    /// <summary>
    ///     Gets content items by ids.
    /// </summary>
    /// <param name="ids">The unique identifiers of the content items to retrieve.</param>
    /// <returns>The content items.</returns>
    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<IApiContentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item([FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        IEnumerable<IPublishedContent> contentItems = ApiPublishedContentCache.GetByIds(ids);

        IApiContentResponse[] apiContentItems = contentItems
            .Where(contentItem => !IsProtected(contentItem))
            .Select(ApiContentResponseBuilder.Build)
            .WhereNotNull()
            .ToArray();

        return await Task.FromResult(Ok(apiContentItems));
    }
}

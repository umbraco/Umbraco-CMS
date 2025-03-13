using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Controllers.Content;

[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class ByIdsContentApiController : ContentApiItemControllerBase
{
    private readonly IRequestMemberAccessService _requestMemberAccessService;

    public ByIdsContentApiController(
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentResponseBuilder apiContentResponseBuilder,
        IRequestMemberAccessService requestMemberAccessService)
        : base(apiPublishedContentCache, apiContentResponseBuilder)
        => _requestMemberAccessService = requestMemberAccessService;

    /// <summary>
    ///     Gets content items by ids.
    /// </summary>
    /// <param name="ids">The unique identifiers of the content items to retrieve.</param>
    /// <returns>The content items.</returns>
    [HttpGet("items")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(IEnumerable<IApiContentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ItemsV20([FromQuery(Name = "id")] HashSet<Guid> ids)
        => await HandleRequest(ids);

    private async Task<IActionResult> HandleRequest(HashSet<Guid> ids)
    {
        IPublishedContent[] contentItems = (await ApiPublishedContentCache.GetByIdsAsync(ids).ConfigureAwait(false)).ToArray();

        IActionResult? deniedAccessResult = await HandleMemberAccessAsync(contentItems, _requestMemberAccessService);
        if (deniedAccessResult is not null)
        {
            return deniedAccessResult;
        }

        IApiContentResponse[] apiContentItems = contentItems
            .Select(ApiContentResponseBuilder.Build)
            .WhereNotNull()
            .ToArray();

        return Ok(apiContentItems);
    }
}

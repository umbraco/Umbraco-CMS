using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Controllers.Content;

[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class ByIdsContentApiController : ContentApiItemControllerBase
{
    private readonly IRequestMemberAccessService _requestMemberAccessService;

    [Obsolete($"Please use the constructor that does not accept {nameof(IPublicAccessService)}. Will be removed in V14.")]
    public ByIdsContentApiController(
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentResponseBuilder apiContentResponseBuilder,
        IPublicAccessService publicAccessService)
        : this(
            apiPublishedContentCache,
            apiContentResponseBuilder,
            StaticServiceProvider.Instance.GetRequiredService<IRequestMemberAccessService>())
    {
    }

    [Obsolete($"Please use the constructor that does not accept {nameof(IPublicAccessService)}. Will be removed in V14.")]
    public ByIdsContentApiController(
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentResponseBuilder apiContentResponseBuilder,
        IPublicAccessService publicAccessService,
        IRequestMemberAccessService requestMemberAccessService)
        : this(
            apiPublishedContentCache,
            apiContentResponseBuilder,
            requestMemberAccessService)
    {
    }

    [ActivatorUtilitiesConstructor]
    public ByIdsContentApiController(
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentResponseBuilder apiContentResponseBuilder,
        IRequestMemberAccessService requestMemberAccessService)
        : base(apiPublishedContentCache, apiContentResponseBuilder)
        => _requestMemberAccessService = requestMemberAccessService;

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<IApiContentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Obsolete("Please use version 2 of this API. Will be removed in V15.")]
    public async Task<IActionResult> Item([FromQuery(Name = "id")] HashSet<Guid> ids)
        => await HandleRequest(ids);

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
        IPublishedContent[] contentItems = ApiPublishedContentCache.GetByIds(ids).ToArray();

        IActionResult? deniedAccessResult = await HandleMemberAccessAsync(contentItems, _requestMemberAccessService);
        if (deniedAccessResult is not null)
        {
            return deniedAccessResult;
        }

        IApiContentResponse[] apiContentItems = contentItems
            .Select(ApiContentResponseBuilder.Build)
            .WhereNotNull()
            .ToArray();

        return await Task.FromResult(Ok(apiContentItems));
    }
}

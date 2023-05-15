using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Delivery.Controllers;

[ApiVersion("1.0")]
public class ByRouteContentApiController : ContentApiItemControllerBase
{
    private readonly IRequestRoutingService _requestRoutingService;
    private readonly IRequestRedirectService _requestRedirectService;

    public ByRouteContentApiController(
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentResponseBuilder apiContentResponseBuilder,
        IPublicAccessService publicAccessService,
        IRequestRoutingService requestRoutingService,
        IRequestRedirectService requestRedirectService)
        : base(apiPublishedContentCache, apiContentResponseBuilder, publicAccessService)
    {
        _requestRoutingService = requestRoutingService;
        _requestRedirectService = requestRedirectService;
    }

    /// <summary>
    ///     Gets a content item by route.
    /// </summary>
    /// <param name="path">The path to the content item.</param>
    /// <remarks>
    ///     Optional URL segment for the root content item
    ///     can be added through the "start-item" header.
    /// </remarks>
    /// <returns>The content item or not found result.</returns>
    [HttpGet("item/{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IApiContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByRoute(string path = "/")
    {
        var contentRoute = _requestRoutingService.GetContentRoute(path);

        IPublishedContent? contentItem = ApiPublishedContentCache.GetByRoute(contentRoute);
        if (contentItem is not null)
        {
            if (IsProtected(contentItem))
            {
                return Unauthorized();
            }

            return await Task.FromResult(Ok(ApiContentResponseBuilder.Build(contentItem)));
        }

        IApiContentRoute? redirectRoute = _requestRedirectService.GetRedirectRoute(path);
        return redirectRoute != null
            ? RedirectTo(redirectRoute)
            : NotFound();
    }

    private IActionResult RedirectTo(IApiContentRoute redirectRoute)
    {
        Response.Headers.Add("Location-Start-Item-Path", redirectRoute.StartItem.Path);
        Response.Headers.Add("Location-Start-Item-Id", redirectRoute.StartItem.Id.ToString("D"));
        return RedirectPermanent(redirectRoute.Path);
    }
}

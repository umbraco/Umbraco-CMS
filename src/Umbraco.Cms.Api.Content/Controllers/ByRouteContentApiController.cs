using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Api.Content.Controllers;

public class ByRouteContentApiController : ContentApiControllerBase
{
    private readonly IRequestRoutingService _requestRoutingService;
    private readonly IRequestRedirectService _requestRedirectService;

    public ByRouteContentApiController(
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentBuilder apiContentBuilder,
        IRequestRoutingService requestRoutingService,
        IRequestRedirectService requestRedirectService)
        : base(apiPublishedContentCache, apiContentBuilder)
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
    [ProducesResponseType(typeof(IApiContent), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByRoute(string path = "/")
    {
        var contentRoute = _requestRoutingService.GetContentRoute(path);

        IPublishedContent? contentItem = ApiPublishedContentCache.GetByRoute(contentRoute);
        if (contentItem is not null)
        {
            return await Task.FromResult(Ok(ApiContentBuilder.Build(contentItem)));
        }

        var redirectPath = _requestRedirectService.GetRedirectPath(path);
        return redirectPath != null
            ? RedirectPermanent(redirectPath)
            : NotFound();
    }
}

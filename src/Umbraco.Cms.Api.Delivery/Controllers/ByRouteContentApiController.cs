using System.Net;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Controllers;

[ApiVersion("1.0")]
public class ByRouteContentApiController : ContentApiItemControllerBase
{
    private readonly IRequestRoutingService _requestRoutingService;
    private readonly IRequestRedirectService _requestRedirectService;
    private readonly IRequestPreviewService _requestPreviewService;

    public ByRouteContentApiController(
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentResponseBuilder apiContentResponseBuilder,
        IPublicAccessService publicAccessService,
        IRequestRoutingService requestRoutingService,
        IRequestRedirectService requestRedirectService,
        IRequestPreviewService requestPreviewService)
        : base(apiPublishedContentCache, apiContentResponseBuilder, publicAccessService)
    {
        _requestRoutingService = requestRoutingService;
        _requestRedirectService = requestRedirectService;
        _requestPreviewService = requestPreviewService;
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
    public async Task<IActionResult> ByRoute(string path = "")
    {
        // OpenAPI does not allow reserved chars as "in:path" parameters, so clients based on the Swagger JSON will URL
        // encode the path. Normally, ASP.NET Core handles that encoding with an automatic decoding - apparently just not
        // for forward slashes, for whatever reason... so we need to deal with those. Hopefully this will be addressed in
        // an upcoming version of ASP.NET Core.
        // See also https://github.com/dotnet/aspnetcore/issues/11544
        if (path.Contains("%2F", StringComparison.OrdinalIgnoreCase))
        {
            path = WebUtility.UrlDecode(path);
        }

        path = path.TrimStart("/");
        path = path.Length == 0 ? "/" : path;

        IPublishedContent? contentItem = GetContent(path);
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

    private IPublishedContent? GetContent(string path)
        => path.StartsWith(Constants.DeliveryApi.Routing.PreviewContentPathPrefix)
            ? GetPreviewContent(path)
            : GetPublishedContent(path);

    private IPublishedContent? GetPublishedContent(string path)
    {
        var contentRoute = _requestRoutingService.GetContentRoute(path);

        IPublishedContent? contentItem = ApiPublishedContentCache.GetByRoute(contentRoute);
        return contentItem;
    }

    private IPublishedContent? GetPreviewContent(string path)
    {
        if (_requestPreviewService.IsPreview() is false)
        {
            return null;
        }

        if (Guid.TryParse(path.AsSpan(Constants.DeliveryApi.Routing.PreviewContentPathPrefix.Length).TrimEnd("/"), out Guid contentId) is false)
        {
            return null;
        }

        IPublishedContent? contentItem = ApiPublishedContentCache.GetById(contentId);
        return contentItem;
    }

    private IActionResult RedirectTo(IApiContentRoute redirectRoute)
    {
        Response.Headers.Add("Location-Start-Item-Path", redirectRoute.StartItem.Path);
        Response.Headers.Add("Location-Start-Item-Id", redirectRoute.StartItem.Id.ToString("D"));
        return RedirectPermanent(redirectRoute.Path);
    }
}

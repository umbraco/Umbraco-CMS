using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Controllers.Content;

[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class ByRouteContentApiController : ContentApiItemControllerBase
{
    private readonly IRequestRoutingService _requestRoutingService;
    private readonly IRequestRedirectService _requestRedirectService;
    private readonly IRequestPreviewService _requestPreviewService;
    private readonly IRequestMemberAccessService _requestMemberAccessService;

    [Obsolete($"Please use the constructor that does not accept {nameof(IPublicAccessService)}. Will be removed in V14.")]
    public ByRouteContentApiController(
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentResponseBuilder apiContentResponseBuilder,
        IPublicAccessService publicAccessService,
        IRequestRoutingService requestRoutingService,
        IRequestRedirectService requestRedirectService,
        IRequestPreviewService requestPreviewService)
        : this(
            apiPublishedContentCache,
            apiContentResponseBuilder,
            requestRoutingService,
            requestRedirectService,
            requestPreviewService,
            StaticServiceProvider.Instance.GetRequiredService<IRequestMemberAccessService>())
    {
    }

    [Obsolete($"Please use the constructor that does not accept {nameof(IPublicAccessService)}. Will be removed in V14.")]
    public ByRouteContentApiController(
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentResponseBuilder apiContentResponseBuilder,
        IPublicAccessService publicAccessService,
        IRequestRoutingService requestRoutingService,
        IRequestRedirectService requestRedirectService,
        IRequestPreviewService requestPreviewService,
        IRequestMemberAccessService requestMemberAccessService)
        : this(
            apiPublishedContentCache,
            apiContentResponseBuilder,
            requestRoutingService,
            requestRedirectService,
            requestPreviewService,
            requestMemberAccessService)
    {
    }

    [ActivatorUtilitiesConstructor]
    public ByRouteContentApiController(
        IApiPublishedContentCache apiPublishedContentCache,
        IApiContentResponseBuilder apiContentResponseBuilder,
        IRequestRoutingService requestRoutingService,
        IRequestRedirectService requestRedirectService,
        IRequestPreviewService requestPreviewService,
        IRequestMemberAccessService requestMemberAccessService)
        : base(apiPublishedContentCache, apiContentResponseBuilder)
    {
        _requestRoutingService = requestRoutingService;
        _requestRedirectService = requestRedirectService;
        _requestPreviewService = requestPreviewService;
        _requestMemberAccessService = requestMemberAccessService;
    }

    [HttpGet("item/{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IApiContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Obsolete("Please use version 2 of this API. Will be removed in V15.")]
    public async Task<IActionResult> ByRoute(string path = "")
        => await HandleRequest(path);

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
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(IApiContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByRouteV20(string path = "")
        => await HandleRequest(path);

    private async Task<IActionResult> HandleRequest(string path)
    {
        path = DecodePath(path);

        path = path.TrimStart("/");
        path = path.Length == 0 ? "/" : path;

        IPublishedContent? contentItem = GetContent(path);
        if (contentItem is not null)
        {
            IActionResult? deniedAccessResult = await HandleMemberAccessAsync(contentItem, _requestMemberAccessService);
            if (deniedAccessResult is not null)
            {
                return deniedAccessResult;
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

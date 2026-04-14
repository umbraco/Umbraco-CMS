using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Api.Delivery.Caching;

/// <summary>
///     Default implementation of <see cref="IDeliveryApiOutputCacheRequestFilter"/> that prevents caching
///     for preview mode requests and requests without public access.
/// </summary>
public class DefaultDeliveryApiOutputCacheRequestFilter : IDeliveryApiOutputCacheRequestFilter
{
    private readonly IRequestPreviewService _requestPreviewService;
    private readonly IApiAccessService _apiAccessService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultDeliveryApiOutputCacheRequestFilter"/> class.
    /// </summary>
    /// <param name="requestPreviewService">The preview service.</param>
    /// <param name="apiAccessService">The API access service.</param>
    public DefaultDeliveryApiOutputCacheRequestFilter(IRequestPreviewService requestPreviewService, IApiAccessService apiAccessService)
    {
        _requestPreviewService = requestPreviewService;
        _apiAccessService = apiAccessService;
    }

    /// <inheritdoc />
    public virtual bool IsCacheable(HttpContext context)
        => IsPreview() is false && HasPublicAccess();

    /// <inheritdoc />
    public virtual bool IsCacheable(HttpContext context, IPublishedContent content) => true;

    /// <summary>
    ///     Returns <c>true</c> if the current request is a preview request; <c>false</c> if the request
    ///     is not a preview and may be cached.
    /// </summary>
    protected virtual bool IsPreview()
        => _requestPreviewService.IsPreview();

    /// <summary>
    ///     Returns <c>true</c> if the current request has public access; <c>false</c> if the request
    ///     is not publicly accessible and should not be cached.
    /// </summary>
    protected virtual bool HasPublicAccess()
        => _apiAccessService.HasPublicAccess();
}

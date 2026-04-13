using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.DeliveryApi;

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

    /// <summary>
    ///     Gets a value indicating whether the current request is a preview request.
    ///     By default returns <c>true</c> when the Delivery API preview mode is active.
    /// </summary>
    protected virtual bool IsPreview()
        => _requestPreviewService.IsPreview();

    /// <summary>
    ///     Gets a value indicating whether the current request has public access.
    ///     By default returns <c>true</c> when the Delivery API is configured for public access or a valid API key is present.
    /// </summary>
    protected virtual bool HasPublicAccess()
        => _apiAccessService.HasPublicAccess();
}

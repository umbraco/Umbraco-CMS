using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Services;

internal sealed class RequestSegmentService : RequestHeaderHandler, IRequestSegmmentService
{
    public RequestSegmentService(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }

    /// <inheritdoc />
    public string? GetRequestedSegment()
        => GetHeaderValue("Accept-Segment");
}

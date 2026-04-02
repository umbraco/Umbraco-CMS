using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Services;

internal sealed class RequestSegmentService : RequestHeaderHandler, IRequestSegmentService, IRequestSegmmentService
{
    public RequestSegmentService(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }

    /// <inheritdoc />
    public string? GetRequestedSegment()
        => GetHeaderValue(Core.Constants.DeliveryApi.HeaderNames.AcceptSegment);
}

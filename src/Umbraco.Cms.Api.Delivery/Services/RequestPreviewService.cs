using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Services;

internal sealed class RequestPreviewService : RequestHeaderHandler, IRequestPreviewService
{
    public RequestPreviewService(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }

    /// <inheritdoc />
    public bool IsPreview() => string.Equals(GetHeaderValue(Core.Constants.DeliveryApi.HeaderNames.Preview), "true", StringComparison.OrdinalIgnoreCase);
}

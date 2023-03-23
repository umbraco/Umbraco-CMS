using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.ContentApi;

namespace Umbraco.Cms.Api.Content.Services;

internal sealed class RequestPreviewService : RequestHeaderHandler, IRequestPreviewService
{
    public RequestPreviewService(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }

    /// <inheritdoc />
    public bool IsPreview() => GetHeaderValue("Preview") == "true";
}

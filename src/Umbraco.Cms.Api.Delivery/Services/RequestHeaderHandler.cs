using Microsoft.AspNetCore.Http;

namespace Umbraco.Cms.Api.Delivery.Services;

internal abstract class RequestHeaderHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    protected RequestHeaderHandler(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    protected string? GetHeaderValue(string headerName) => _httpContextAccessor.HttpContext?.Request.Headers[headerName];
}

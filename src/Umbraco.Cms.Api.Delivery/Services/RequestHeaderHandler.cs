using Microsoft.AspNetCore.Http;

namespace Umbraco.Cms.Api.Delivery.Services;

internal abstract class RequestHeaderHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    protected RequestHeaderHandler(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    protected string? GetHeaderValue(string headerName)
    {
        HttpContext httpContext = _httpContextAccessor.HttpContext ??
                                  throw new InvalidOperationException("Could not obtain an HTTP context");

        return httpContext.Request.Headers[headerName];
    }
}

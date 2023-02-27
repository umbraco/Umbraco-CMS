using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Content.Services;

public abstract class RequestHeaderService
{
    private readonly HttpContext _httpContext;
    private string? _headerValue;

    protected RequestHeaderService(IHttpContextAccessor httpContextAccessor)
        => _httpContext = httpContextAccessor.HttpContext ?? throw new ArgumentNullException(nameof(IHttpContextAccessor.HttpContext));

    protected abstract string HeaderName { get; }

    protected string? GetHeaderValue()
    {
        // initialize the cached header value on first access
        _headerValue ??= _httpContext.Request.Headers.TryGetValue(HeaderName, out StringValues headerValue)
            ? headerValue.ToString()
            : string.Empty;

        // return null if the cached header value was empty (i.e. no header value present)
        return _headerValue.IfNullOrWhiteSpace(null);
    }
}

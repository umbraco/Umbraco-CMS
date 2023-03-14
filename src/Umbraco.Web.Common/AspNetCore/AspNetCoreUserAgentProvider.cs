using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Net;

namespace Umbraco.Cms.Web.Common.AspNetCore;

public class AspNetCoreUserAgentProvider : IUserAgentProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AspNetCoreUserAgentProvider(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public string? GetUserAgent() => _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
}

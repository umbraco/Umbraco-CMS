using Microsoft.AspNetCore.Http;
using Umbraco.Net;

namespace Umbraco.Web.Common.AspNetCore
{
    public class AspNetIpResolver : IIpResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AspNetIpResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentRequestIpAddress() => _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;
    }
}

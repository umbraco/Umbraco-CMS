using Microsoft.AspNetCore.Http;
using Umbraco.Core.Net;

namespace Umbraco.Web.BackOffice.AspNetCore
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

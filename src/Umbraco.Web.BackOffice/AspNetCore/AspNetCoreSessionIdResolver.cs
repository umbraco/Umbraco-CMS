using Microsoft.AspNetCore.Http;
using Umbraco.Net;

namespace Umbraco.Web.BackOffice.AspNetCore
{
    internal class AspNetCoreSessionIdResolver : ISessionIdResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AspNetCoreSessionIdResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string SessionId => _httpContextAccessor?.HttpContext.Session?.Id;
    }
}

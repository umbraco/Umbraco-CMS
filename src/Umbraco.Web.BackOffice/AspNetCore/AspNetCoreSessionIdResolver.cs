using Microsoft.AspNetCore.Http;
using Umbraco.Net;

namespace Umbraco.Web.BackOffice.AspNetCore
{
    internal class AspNetSessionIdResolver : ISessionIdResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AspNetSessionIdResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string SessionId => _httpContextAccessor?.HttpContext.Session?.Id;
    }
}

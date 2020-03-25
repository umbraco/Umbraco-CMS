using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Umbraco.Core.Net;

namespace Umbraco.Web.BackOffice.AspNetCore
{
    internal class AspNetCoreSessionIdResolver : ISessionIdResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AspNetCoreSessionIdResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        
        public string SessionId
        {
            get
            {
                var httpContext = _httpContextAccessor?.HttpContext;
                // If session isn't enabled this will throw an exception so we check
                var sessionFeature = httpContext?.Features.Get<ISessionFeature>();
                return sessionFeature != null
                    ? httpContext?.Session?.Id
                    : "0";
            }
        }
    }
}

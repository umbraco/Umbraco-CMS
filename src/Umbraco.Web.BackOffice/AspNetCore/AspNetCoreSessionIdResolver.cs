using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
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

        
        public string SessionId
        {
            get
            {
                // If session isn't enabled this will throw an exception so we check
                var sessionFeature = _httpContextAccessor?.HttpContext?.Features.Get<ISessionFeature>();
                return sessionFeature != null
                    ? _httpContextAccessor?.HttpContext?.Session?.Id
                    : "0";
            }
        }
    }
}

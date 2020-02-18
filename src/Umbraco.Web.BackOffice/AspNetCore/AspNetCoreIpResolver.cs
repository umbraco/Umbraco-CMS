using System;
using Microsoft.AspNetCore.Http;
using Umbraco.Net;

namespace Umbraco.Web.BackOffice.AspNetCore
{
    internal class AspNetIpResolver : IIpResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AspNetIpResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentRequestIpAddress() => _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? String.Empty;
    }
}

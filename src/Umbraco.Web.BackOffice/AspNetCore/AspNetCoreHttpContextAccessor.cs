using Microsoft.AspNetCore.Http;

namespace Umbraco.Web.BackOffice.AspNetCore
{
    internal class AspNetCoreHttpContextAccessor //: IHttpContextAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AspNetCoreHttpContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public HttpContext HttpContext => _httpContextAccessor.HttpContext;
    }
}

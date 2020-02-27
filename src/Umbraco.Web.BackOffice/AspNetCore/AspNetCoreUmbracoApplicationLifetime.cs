using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Umbraco.Net;

namespace Umbraco.Web.AspNet
{
    public class AspNetCoreUmbracoApplicationLifetime : IUmbracoApplicationLifetime
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApplicationLifetime _applicationLifetime;

        public AspNetCoreUmbracoApplicationLifetime(IHttpContextAccessor httpContextAccessor, IApplicationLifetime applicationLifetime)
        {
            _httpContextAccessor = httpContextAccessor;
            _applicationLifetime = applicationLifetime;
        }

        public bool IsRestarting { get; set; }
        public void Restart()
        {
            IsRestarting = true;

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                // unload app domain - we must null out all identities otherwise we get serialization errors
                // http://www.zpqrtbnk.net/posts/custom-iidentity-serialization-issue
                httpContext.User = null;
            }

            Thread.CurrentPrincipal = null;
            _applicationLifetime.StopApplication();
        }
    }
}

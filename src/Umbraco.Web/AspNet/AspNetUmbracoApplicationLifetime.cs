using System.Threading;
using System.Web;
using Umbraco.Cms.Core.Hosting;

namespace Umbraco.Web.AspNet
{
    public class AspNetUmbracoApplicationLifetime : IUmbracoApplicationLifetime
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AspNetUmbracoApplicationLifetime(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
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
            HttpRuntime.UnloadAppDomain();
        }
    }
}

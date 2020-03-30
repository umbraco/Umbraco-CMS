using System;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Umbraco.Net;

namespace Umbraco.Web.Common.AspNetCore
{
    public class AspNetCoreUmbracoApplicationLifetime : IUmbracoApplicationLifetime, IUmbracoApplicationLifetimeManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public AspNetCoreUmbracoApplicationLifetime(IHttpContextAccessor httpContextAccessor, IHostApplicationLifetime hostApplicationLifetime)
        {
            _httpContextAccessor = httpContextAccessor;
            _hostApplicationLifetime = hostApplicationLifetime;
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
            _hostApplicationLifetime.StopApplication();
        }

        public void InvokeApplicationInit()
        {
            ApplicationInit?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler ApplicationInit;
    }
}

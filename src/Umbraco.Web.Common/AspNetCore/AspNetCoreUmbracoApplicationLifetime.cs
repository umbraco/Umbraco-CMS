using System;
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
            _hostApplicationLifetime.StopApplication();
        }

        public void InvokeApplicationInit()
        {
            ApplicationInit?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler ApplicationInit;
    }
}

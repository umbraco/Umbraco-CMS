using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Umbraco.Core.Hosting;

namespace Umbraco.Web.Common.AspNetCore
{
    public class AspNetCoreUmbracoApplicationLifetime : IUmbracoApplicationLifetime, IUmbracoApplicationLifetimeManager
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public AspNetCoreUmbracoApplicationLifetime(IHostApplicationLifetime hostApplicationLifetime)
        {
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

        // TODO: Should be killed and replaced with UmbracoApplicationStarting notifications
        public event EventHandler ApplicationInit;
    }
}

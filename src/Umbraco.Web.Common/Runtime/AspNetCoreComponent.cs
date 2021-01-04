using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Hosting;
using Umbraco.Web.Common.Lifetime;

namespace Umbraco.Web.Common.Runtime
{
    public sealed class AspNetCoreComponent : IComponent
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IUmbracoApplicationLifetimeManager _umbracoApplicationLifetimeManager;
        
        public AspNetCoreComponent(
            IHostApplicationLifetime hostApplicationLifetime,
            IUmbracoApplicationLifetimeManager umbracoApplicationLifetimeManager)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _umbracoApplicationLifetimeManager = umbracoApplicationLifetimeManager;
        }

        public void Initialize()
        {
            _hostApplicationLifetime.ApplicationStarted.Register(() => {
                _umbracoApplicationLifetimeManager.InvokeApplicationInit();
            });
        }



        public void Terminate()
        {
        }
    }
}

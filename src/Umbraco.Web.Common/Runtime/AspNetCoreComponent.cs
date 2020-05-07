using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Net;
using Umbraco.Web.Common.Lifetime;

namespace Umbraco.Web.Common.Runtime
{
    public sealed class AspNetCoreComponent : IComponent
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IUmbracoApplicationLifetimeManager _umbracoApplicationLifetimeManager;
        private readonly IUmbracoRequestLifetime _umbracoRequestLifetime;
        private readonly IRuntimeState _runtimeState;

        public AspNetCoreComponent(
            IHostApplicationLifetime hostApplicationLifetime,
            IUmbracoApplicationLifetimeManager umbracoApplicationLifetimeManager,
            IUmbracoRequestLifetime umbracoRequestLifetime,
            IRuntimeState runtimeState)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _umbracoApplicationLifetimeManager = umbracoApplicationLifetimeManager;
            _umbracoRequestLifetime = umbracoRequestLifetime;
            _runtimeState = runtimeState;
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

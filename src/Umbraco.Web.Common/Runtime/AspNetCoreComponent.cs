using Microsoft.Extensions.Hosting;
using Umbraco.Core.Composing;
using Umbraco.Net;

namespace Umbraco.Web.Common.Runtime
{
    public sealed class AspNetCoreComponent : IComponent
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IUmbracoApplicationLifetimeManager _umbracoApplicationLifetimeManager;

        public AspNetCoreComponent(IHostApplicationLifetime hostApplicationLifetime, IUmbracoApplicationLifetimeManager umbracoApplicationLifetimeManager)
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

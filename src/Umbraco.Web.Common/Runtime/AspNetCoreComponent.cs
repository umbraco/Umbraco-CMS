using Microsoft.Extensions.Hosting;
using Umbraco.Core.Composing;
using Umbraco.Net;

namespace Umbraco.Web.Common.Runtime
{
    public sealed class AspNetCoreComponent : IComponent
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IUmbracoApplicationLifetime _umbracoApplicationLifetime;

        public AspNetCoreComponent(IHostApplicationLifetime hostApplicationLifetime, IUmbracoApplicationLifetime umbracoApplicationLifetime)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _umbracoApplicationLifetime = umbracoApplicationLifetime;
        }

        public void Initialize()
        {
            _hostApplicationLifetime.ApplicationStarted.Register(() => {
                _umbracoApplicationLifetime.InvokeApplicationInit();
            });
        }

        public void Terminate()
        {
        }
    }
}

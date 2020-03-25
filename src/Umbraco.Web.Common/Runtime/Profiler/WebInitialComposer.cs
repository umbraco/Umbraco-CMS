using Microsoft.AspNetCore.Http;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Net;
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.Common.Middleware;

namespace Umbraco.Web.Common.Runtime.Profiler
{
    public class WebInitialComposer : ComponentComposer<WebInitialComponent>, ICoreComposer
    {

        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            var umbracoRequestLifetime = new UmbracoRequestLifetime();

            composition.RegisterUnique<IUmbracoRequestLifetimeManager>(factory => umbracoRequestLifetime);
            composition.RegisterUnique<IUmbracoRequestLifetime>(factory => umbracoRequestLifetime);
            composition.RegisterUnique<IUmbracoApplicationLifetime, AspNetCoreUmbracoApplicationLifetime>();
        }
    }
}

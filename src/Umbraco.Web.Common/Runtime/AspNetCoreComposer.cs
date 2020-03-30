using Microsoft.AspNetCore.Http;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Hosting;
using Umbraco.Net;
using Umbraco.Core.Runtime;
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.Common.Lifetime;

namespace Umbraco.Web.Common.Runtime
{
    /// <summary>
    /// Adds/replaces AspNetCore specific services
    /// </summary>
    [ComposeBefore(typeof(ICoreComposer))]
    [ComposeAfter(typeof(CoreInitialComposer))]
    public class AspNetCoreComposer : ComponentComposer<AspNetCoreComponent>, IComposer
    {
        public new void Compose(Composition composition)
        {
            base.Compose(composition);

            // AspNetCore specific services
            composition.RegisterUnique<IHttpContextAccessor, HttpContextAccessor>();

            // Our own netcore implementations
            composition.RegisterUnique<AspNetCoreUmbracoApplicationLifetime>();
            composition.RegisterUnique<IUmbracoApplicationLifetimeManager>(factory => factory.GetInstance<AspNetCoreUmbracoApplicationLifetime>());
            composition.RegisterUnique<IUmbracoApplicationLifetime>(factory => factory.GetInstance<AspNetCoreUmbracoApplicationLifetime>());

            composition.RegisterUnique<IApplicationShutdownRegistry, AspNetCoreApplicationShutdownRegistry>();

            // The umbraco request lifetime
            composition.RegisterUnique<UmbracoRequestLifetime>();
            composition.RegisterUnique<IUmbracoRequestLifetimeManager>(factory => factory.GetInstance<UmbracoRequestLifetime>());
            composition.RegisterUnique<IUmbracoRequestLifetime>(factory => factory.GetInstance<UmbracoRequestLifetime>());
            composition.RegisterUnique<IUmbracoApplicationLifetime, AspNetCoreUmbracoApplicationLifetime>();
        }
    }
}

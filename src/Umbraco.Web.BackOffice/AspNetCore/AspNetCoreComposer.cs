using Microsoft.AspNetCore.Http;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Hosting;
using Umbraco.Net;
using Umbraco.Core.Runtime;
using Umbraco.Web.Common.AspNetCore;

namespace Umbraco.Web.BackOffice.AspNetCore
{
    /// <summary>
    /// Adds/replaces AspNetCore specific services
    /// </summary>
    [ComposeBefore(typeof(ICoreComposer))]
    [ComposeAfter(typeof(CoreInitialComposer))]
    public class AspNetCoreComposer : IComposer
    {
        public void Compose(Composition composition)
        {
            // AspNetCore specific services
            composition.RegisterUnique<IHttpContextAccessor, HttpContextAccessor>();

            // Our own netcore implementations
            composition.RegisterUnique<IUmbracoApplicationLifetime, AspNetCoreUmbracoApplicationLifetime>();
            composition.RegisterUnique<IApplicationShutdownRegistry, AspNetCoreApplicationShutdownRegistry>();
        }
    }
}

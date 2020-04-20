using Microsoft.AspNetCore.Http;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Hosting;
using Umbraco.Net;
using Umbraco.Core.Runtime;
using Umbraco.Core.Security;
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.Common.Formatters;
using Umbraco.Web.Common.Lifetime;
using Umbraco.Web.Common.Macros;
using Umbraco.Web.Composing.CompositionExtensions;
using Umbraco.Web.Macros;

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
            composition.RegisterUnique<IRequestAccessor, AspNetCoreRequestAccessor>();

            // Our own netcore implementations
            composition.RegisterMultipleUnique<IUmbracoApplicationLifetimeManager, IUmbracoApplicationLifetime, AspNetCoreUmbracoApplicationLifetime>();

            composition.RegisterUnique<IApplicationShutdownRegistry, AspNetCoreApplicationShutdownRegistry>();

            // The umbraco request lifetime
            composition.RegisterMultipleUnique<IUmbracoRequestLifetime, IUmbracoRequestLifetimeManager, UmbracoRequestLifetime>();


            //Password hasher
            composition.RegisterUnique<IPasswordHasher, AspNetCorePasswordHasher>();


            composition.RegisterUnique<ICookieManager, AspNetCoreCookieManager>();
            composition.RegisterUnique<IUserAgentProvider, AspNetCoreUserAgentProvider>();

            composition.RegisterMultipleUnique<ISessionIdResolver, ISessionManager, AspNetCoreSessionManager>();

            composition.RegisterUnique<IMacroRenderer, MacroRenderer>();
            composition.RegisterUnique<IMemberUserKeyProvider, MemberUserKeyProvider>();

            composition.RegisterUnique<AngularJsonMediaTypeFormatter>();

            //register the install components
            //NOTE: i tried to not have these registered if we weren't installing or upgrading but post install when the site restarts
            //it still needs to use the install controller so we can't do that
            composition.ComposeInstaller();

        }
    }
}

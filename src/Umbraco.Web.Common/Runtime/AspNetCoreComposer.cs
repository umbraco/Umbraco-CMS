using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Diagnostics;
using Umbraco.Core.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Runtime;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Extensions;
using Umbraco.Net;
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Common.Formatters;
using Umbraco.Web.Common.Install;
using Umbraco.Web.Common.Lifetime;
using Umbraco.Web.Common.Macros;
using Umbraco.Web.Common.Middleware;
using Umbraco.Web.Common.ModelBinding;
using Umbraco.Web.Common.Profiler;
using Umbraco.Web.Common.Routing;
using Umbraco.Web.Common.Security;
using Umbraco.Web.Common.Templates;
using Umbraco.Web.Composing.CompositionExtensions;
using Umbraco.Web.Macros;
using Umbraco.Web.Security;
using Umbraco.Web.Templates;

namespace Umbraco.Web.Common.Runtime
{

    /// <summary>
    /// Adds/replaces AspNetCore specific services
    /// </summary>
    [ComposeBefore(typeof(ICoreComposer))]
    [ComposeAfter(typeof(CoreInitialComposer))]
    public class AspNetCoreComposer : ComponentComposer<AspNetCoreComponent>, IComposer
    {
        public override void Compose(Composition composition)
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
            composition.Services.AddTransient<IIpResolver, AspNetCoreIpResolver>();
            composition.RegisterUnique<IUserAgentProvider, AspNetCoreUserAgentProvider>();

            composition.RegisterMultipleUnique<ISessionIdResolver, ISessionManager, AspNetCoreSessionManager>();

            composition.RegisterUnique<IMarchal, AspNetCoreMarchal>();

            composition.RegisterUnique<IProfilerHtml, WebProfilerHtml>();

            composition.RegisterUnique<IMacroRenderer, MacroRenderer>();
            composition.RegisterUnique<IMemberUserKeyProvider, MemberUserKeyProvider>();


            // register the umbraco context factory
            composition.RegisterUnique<IUmbracoContextFactory, UmbracoContextFactory>();
            composition.RegisterUnique<IBackofficeSecurityFactory, BackofficeSecurityFactory>();
            composition.RegisterUnique<IBackofficeSecurityAccessor, HybridBackofficeSecurityAccessor>();

            //register the install components
            //NOTE: i tried to not have these registered if we weren't installing or upgrading but post install when the site restarts
            //it still needs to use the install controller so we can't do that
            composition.ComposeInstaller();

            var umbracoApiControllerTypes = composition.TypeLoader.GetUmbracoApiControllers().ToList();
            composition.WithCollectionBuilder<UmbracoApiControllerTypeCollectionBuilder>()
                .Add(umbracoApiControllerTypes);


            composition.RegisterUnique<InstallAreaRoutes>();

            composition.RegisterUnique<UmbracoRequestLoggingMiddleware>();
            composition.RegisterUnique<UmbracoRequestMiddleware>();
            composition.RegisterUnique<BootFailedMiddleware>();

            composition.RegisterUnique<UmbracoJsonModelBinder>();

            composition.RegisterUnique<ITemplateRenderer, TemplateRenderer>();
            composition.RegisterUnique<IPublicAccessChecker, PublicAccessChecker>();
            composition.RegisterUnique(factory => new LegacyPasswordSecurity());
        }
    }
}

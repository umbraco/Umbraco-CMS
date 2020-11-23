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
            composition.Services.AddUnique<IHttpContextAccessor, HttpContextAccessor>();
            composition.Services.AddUnique<IRequestAccessor, AspNetCoreRequestAccessor>();

            // Our own netcore implementations
            composition.Services.AddMultipleUnique<IUmbracoApplicationLifetimeManager, IUmbracoApplicationLifetime, AspNetCoreUmbracoApplicationLifetime>();

            composition.Services.AddUnique<IApplicationShutdownRegistry, AspNetCoreApplicationShutdownRegistry>();

            // The umbraco request lifetime
            composition.Services.AddMultipleUnique<IUmbracoRequestLifetime, IUmbracoRequestLifetimeManager, UmbracoRequestLifetime>();

            // Password hasher
            composition.Services.AddUnique<IPasswordHasher, AspNetCorePasswordHasher>();

            composition.Services.AddUnique<ICookieManager, AspNetCoreCookieManager>();
            composition.Services.AddTransient<IIpResolver, AspNetCoreIpResolver>();
            composition.Services.AddUnique<IUserAgentProvider, AspNetCoreUserAgentProvider>();

            composition.Services.AddMultipleUnique<ISessionIdResolver, ISessionManager, AspNetCoreSessionManager>();

            composition.Services.AddUnique<IMarchal, AspNetCoreMarchal>();

            composition.Services.AddUnique<IProfilerHtml, WebProfilerHtml>();

            composition.Services.AddUnique<IMacroRenderer, MacroRenderer>();
            composition.Services.AddUnique<IMemberUserKeyProvider, MemberUserKeyProvider>();

            // register the umbraco context factory
            composition.Services.AddUnique<IUmbracoContextFactory, UmbracoContextFactory>();

            composition.Services.AddUnique<IBackOfficeSecurityFactory, BackOfficeSecurityFactory>();
            composition.Services.AddUnique<IBackOfficeSecurityAccessor, HybridBackofficeSecurityAccessor>();

            composition.Services.AddUnique<IUmbracoWebsiteSecurityAccessor, HybridUmbracoWebsiteSecurityAccessor>();

            //register the install components
            composition.ComposeInstaller();

            var umbracoApiControllerTypes = composition.TypeLoader.GetUmbracoApiControllers().ToList();
            composition.WithCollectionBuilder<UmbracoApiControllerTypeCollectionBuilder>()
                .Add(umbracoApiControllerTypes);


            composition.Services.AddUnique<InstallAreaRoutes>();

            composition.Services.AddUnique<UmbracoRequestLoggingMiddleware>();
            composition.Services.AddUnique<UmbracoRequestMiddleware>();
            composition.Services.AddUnique<BootFailedMiddleware>();

            composition.Services.AddUnique<UmbracoJsonModelBinder>();

            composition.Services.AddUnique<ITemplateRenderer, TemplateRenderer>();
            composition.Services.AddUnique<IPublicAccessChecker, PublicAccessChecker>();
            composition.Services.AddUnique(factory => new LegacyPasswordSecurity());
        }
    }
}

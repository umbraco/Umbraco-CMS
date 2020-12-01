using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Builder;
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
        public override void Compose(IUmbracoBuilder builder)
        {
            base.Compose(builder);

            // AspNetCore specific services
            builder.Services.AddUnique<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddUnique<IRequestAccessor, AspNetCoreRequestAccessor>();

            // Our own netcore implementations
            builder.Services.AddMultipleUnique<IUmbracoApplicationLifetimeManager, IUmbracoApplicationLifetime, AspNetCoreUmbracoApplicationLifetime>();

            builder.Services.AddUnique<IApplicationShutdownRegistry, AspNetCoreApplicationShutdownRegistry>();

            // The umbraco request lifetime
            builder.Services.AddMultipleUnique<IUmbracoRequestLifetime, IUmbracoRequestLifetimeManager, UmbracoRequestLifetime>();

            // Password hasher
            builder.Services.AddUnique<IPasswordHasher, AspNetCorePasswordHasher>();

            builder.Services.AddUnique<ICookieManager, AspNetCoreCookieManager>();
            builder.Services.AddTransient<IIpResolver, AspNetCoreIpResolver>();
            builder.Services.AddUnique<IUserAgentProvider, AspNetCoreUserAgentProvider>();

            builder.Services.AddMultipleUnique<ISessionIdResolver, ISessionManager, AspNetCoreSessionManager>();

            builder.Services.AddUnique<IMarchal, AspNetCoreMarchal>();

            builder.Services.AddUnique<IProfilerHtml, WebProfilerHtml>();

            builder.Services.AddUnique<IMacroRenderer, MacroRenderer>();
            builder.Services.AddUnique<IMemberUserKeyProvider, MemberUserKeyProvider>();

            // register the umbraco context factory

            builder.Services.AddUnique<IUmbracoContextFactory, UmbracoContextFactory>();
            builder.Services.AddUnique<IBackOfficeSecurityFactory, BackOfficeSecurityFactory>();
            builder.Services.AddUnique<IBackOfficeSecurityAccessor, HybridBackofficeSecurityAccessor>();
            builder.Services.AddUnique<IUmbracoWebsiteSecurityAccessor, HybridUmbracoWebsiteSecurityAccessor>();

            //register the install components
            builder.ComposeInstaller();

            var umbracoApiControllerTypes = builder.TypeLoader.GetUmbracoApiControllers().ToList();
            builder.WithCollectionBuilder<UmbracoApiControllerTypeCollectionBuilder>()
                .Add(umbracoApiControllerTypes);


            builder.Services.AddUnique<InstallAreaRoutes>();

            builder.Services.AddUnique<UmbracoRequestLoggingMiddleware>();
            builder.Services.AddUnique<UmbracoRequestMiddleware>();
            builder.Services.AddUnique<BootFailedMiddleware>();

            builder.Services.AddUnique<UmbracoJsonModelBinder>();

            builder.Services.AddUnique<ITemplateRenderer, TemplateRenderer>();
            builder.Services.AddUnique<IPublicAccessChecker, PublicAccessChecker>();            
        }
    }
}

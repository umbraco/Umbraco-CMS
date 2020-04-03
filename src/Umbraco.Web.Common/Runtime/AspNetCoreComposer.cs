using Microsoft.AspNetCore.Http;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Hosting;
using Umbraco.Core.Media;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Net;
using Umbraco.Core.Runtime;
using Umbraco.Core.Security;
using Umbraco.Infrastructure.Media;
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.Common.Lifetime;
using Umbraco.Web.Models.PublishedContent;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.Routing;
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

            composition.RegisterUnique<IPasswordHasher, AspNetCorePasswordHasher>();


            // register the http context and umbraco context accessors
            // we *should* use the HttpContextUmbracoContextAccessor, however there are cases when
            // we have no http context, eg when booting Umbraco or in background threads, so instead
            // let's use an hybrid accessor that can fall back to a ThreadStatic context.
            composition.RegisterUnique<IUmbracoContextAccessor, HybridUmbracoContextAccessor>();

            // register the umbraco context factory
            // composition.RegisterUnique<IUmbracoContextFactory, UmbracoContextFactory>();
            composition.RegisterUnique<IPublishedUrlProvider, UrlProvider>();

            composition.RegisterUnique<HtmlLocalLinkParser>();
            composition.RegisterUnique<HtmlImageSourceParser>();
            composition.RegisterUnique<HtmlUrlParser>();
            composition.RegisterUnique<RichTextEditorPastedImages>();

            composition.UrlProviders()
                .Append<AliasUrlProvider>()
                .Append<DefaultUrlProvider>();

            composition.MediaUrlProviders()
                .Append<DefaultMediaUrlProvider>();

            composition.RegisterUnique<ISiteDomainHelper, SiteDomainHelper>();

            // register properties fallback
            composition.RegisterUnique<IPublishedValueFallback, PublishedValueFallback>();

            composition.RegisterUnique<IImageUrlGenerator, ImageSharpImageUrlGenerator>();
        }
    }
}

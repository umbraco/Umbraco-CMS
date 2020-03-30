using Microsoft.AspNetCore.Http;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Hosting;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Net;
using Umbraco.Core.Runtime;
using Umbraco.Core.Security;
using Umbraco.Web.Models;
using Umbraco.Web.Models.PublishedContent;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.Routing;
using Umbraco.Web.Templates;

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

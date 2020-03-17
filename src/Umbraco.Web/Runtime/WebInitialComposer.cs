using System.Linq;
using System.Web.Security;
using Examine;
using Microsoft.AspNet.SignalR;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Cookie;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Events;
using Umbraco.Core.Hosting;
using Umbraco.Core.Install;
using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Runtime;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Net;
using Umbraco.Web.Actions;
using Umbraco.Web.Cache;
using Umbraco.Web.Composing.CompositionExtensions;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Editors;
using Umbraco.Web.Features;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.Hosting;
using Umbraco.Web.Install;
using Umbraco.Web.Macros;
using Umbraco.Web.Media.EmbedProviders;
using Umbraco.Web.Models.PublishedContent;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Search;
using Umbraco.Web.Sections;
using Umbraco.Web.Security;
using Umbraco.Web.Security.Providers;
using Umbraco.Web.Services;
using Umbraco.Web.SignalR;
using Umbraco.Web.Templates;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;
using Umbraco.Web.PropertyEditors;
using Umbraco.Examine;
using Umbraco.Core.Models;
using Umbraco.Core.Request;
using Umbraco.Core.Session;
using Umbraco.Web.AspNet;
using Umbraco.Web.Models;

namespace Umbraco.Web.Runtime
{
    // web's initial composer composes after core's, and before all core composers
    [ComposeAfter(typeof(CoreInitialComposer))]
    [ComposeBefore(typeof(ICoreComposer))]
    public sealed class WebInitialComposer : ComponentComposer<WebInitialComponent>
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.Register<UmbracoInjectedModule>();
            composition.Register<IIpResolver, AspNetIpResolver>();

            composition.Register<IUserAgentProvider, AspNetUserAgentProvider>();
            composition.Register<AspNetSessionManager>(Lifetime.Singleton);
            composition.Register<ISessionIdResolver>(factory => factory.GetInstance<AspNetSessionManager>(), Lifetime.Singleton);
            composition.Register<ISessionManager>(factory => factory.GetInstance<AspNetSessionManager>(), Lifetime.Singleton);

            composition.Register<IRequestAccessor, AspNetRequestAccessor>(Lifetime.Singleton);

            composition.Register<IHostingEnvironment, AspNetHostingEnvironment>();
            composition.Register<IBackOfficeInfo, AspNetBackOfficeInfo>();
            composition.Register<IUmbracoApplicationLifetime, AspNetUmbracoApplicationLifetime>(Lifetime.Singleton);
            composition.Register<IPasswordHasher, AspNetPasswordHasher>();
            composition.Register<IFilePermissionHelper, FilePermissionHelper>(Lifetime.Singleton);

            composition.RegisterUnique<IHttpContextAccessor, AspNetHttpContextAccessor>(); // required for hybrid accessors
            composition.RegisterUnique<ICookieManager, AspNetCookieManager>();


            composition.ComposeWebMappingProfiles();

            //register the install components
            //NOTE: i tried to not have these registered if we weren't installing or upgrading but post install when the site restarts
            //it still needs to use the install controller so we can't do that
            composition.ComposeInstaller();

            // register membership stuff
            composition.Register(factory => MembershipProviderExtensions.GetMembersMembershipProvider());
            composition.Register(factory => Roles.Enabled ? Roles.Provider : new MembersRoleProvider(factory.GetInstance<IMemberService>()));
            composition.Register<MembershipHelper>(Lifetime.Request);
            composition.Register<IPublishedMemberCache>(factory => factory.GetInstance<IUmbracoContext>().PublishedSnapshot.Members);
            composition.RegisterUnique<IMemberUserKeyProvider, MemberUserKeyProvider>();
            composition.RegisterUnique<IPublicAccessChecker, PublicAccessChecker>();

            // register accessors for cultures
            composition.RegisterUnique<IDefaultCultureAccessor, DefaultCultureAccessor>();


            // register the http context and umbraco context accessors
            // we *should* use the HttpContextUmbracoContextAccessor, however there are cases when
            // we have no http context, eg when booting Umbraco or in background threads, so instead
            // let's use an hybrid accessor that can fall back to a ThreadStatic context.
            composition.RegisterUnique<IUmbracoContextAccessor, HybridUmbracoContextAccessor>();

            // register the umbraco context factory
            composition.RegisterUnique<IUmbracoContextFactory, UmbracoContextFactory>();
            composition.RegisterUnique<IPublishedUrlProvider, UrlProvider>();

            // we should stop injecting UmbracoContext and always inject IUmbracoContextAccessor, however at the moment
            // there are tons of places (controllers...) which require UmbracoContext in their ctor - so let's register
            // a way to inject the UmbracoContext - DO NOT register this as Lifetime.Request since LI will dispose the context
            // in it's own way but we don't want that to happen, we manage its lifetime ourselves.
            composition.Register(factory => factory.GetInstance<IUmbracoContextAccessor>().UmbracoContext);
            composition.RegisterUnique<IUmbracoTreeSearcherFields, UmbracoTreeSearcherFields>();
            composition.Register<IPublishedContentQuery>(factory =>
            {
                var umbCtx = factory.GetInstance<IUmbracoContextAccessor>();
                return new PublishedContentQuery(umbCtx.UmbracoContext.PublishedSnapshot, factory.GetInstance<IVariationContextAccessor>(), factory.GetInstance<IExamineManager>());
            }, Lifetime.Request);
            composition.Register<ITagQuery, TagQuery>(Lifetime.Request);

            composition.RegisterUnique<ITemplateRenderer, TemplateRenderer>();
            composition.RegisterUnique<IMacroRenderer, MacroRenderer>();

            composition.RegisterUnique<IUmbracoComponentRenderer, UmbracoComponentRenderer>();

            composition.RegisterUnique<HtmlLocalLinkParser>();
            composition.RegisterUnique<HtmlUrlParser>();
            composition.RegisterUnique<HtmlImageSourceParser>();
            composition.RegisterUnique<RichTextEditorPastedImages>();

            // register the umbraco helper - this is Transient! very important!
            // also, if not level.Run, we cannot really use the helper (during upgrade...)
            // so inject a "void" helper (not exactly pretty but...)
            if (composition.RuntimeState.Level == RuntimeLevel.Run)
                composition.Register<UmbracoHelper>(factory =>
                {
                    var umbCtx = factory.GetInstance<IUmbracoContext>();
                    return new UmbracoHelper(umbCtx.IsFrontEndUmbracoRequest ? umbCtx.PublishedRequest?.PublishedContent : null, factory.GetInstance<ICultureDictionaryFactory>(),
                        factory.GetInstance<IUmbracoComponentRenderer>(), factory.GetInstance<IPublishedContentQuery>());
                });
            else
                composition.Register(_ => new UmbracoHelper());

            // register distributed cache
            composition.RegisterUnique(f => new DistributedCache(f.GetInstance<IServerMessenger>(), f.GetInstance<CacheRefresherCollection>()));

            composition.RegisterUnique<RoutableDocumentFilter>();

            // replace some services
            composition.RegisterUnique<IEventMessagesFactory, DefaultEventMessagesFactory>();
            composition.RegisterUnique<IEventMessagesAccessor, HybridEventMessagesAccessor>();
            composition.RegisterUnique<ITreeService, TreeService>();
            composition.RegisterUnique<ISectionService, SectionService>();

            composition.RegisterUnique<IExamineManager, ExamineManager>();

            // configure the container for web
            composition.ConfigureForWeb();

            composition
                .ComposeUmbracoControllers(GetType().Assembly)
                .SetDefaultRenderMvcController<RenderMvcController>(); // default controller for template views

            composition.SearchableTrees()
                .Add(() => composition.TypeLoader.GetTypes<ISearchableTree>());

            composition.Register<UmbracoTreeSearcher>(Lifetime.Request);

            composition.EditorValidators()
                .Add(() => composition.TypeLoader.GetTypes<IEditorValidator>());

            composition.TourFilters();

            composition.RegisterUnique<UmbracoFeatures>();

            composition.Actions()
                .Add(() => composition.TypeLoader.GetTypes<IAction>());

            //we need to eagerly scan controller types since they will need to be routed
            composition.WithCollectionBuilder<SurfaceControllerTypeCollectionBuilder>()
                .Add(composition.TypeLoader.GetSurfaceControllers());
            var umbracoApiControllerTypes = composition.TypeLoader.GetUmbracoApiControllers().ToList();
            composition.WithCollectionBuilder<UmbracoApiControllerTypeCollectionBuilder>()
                .Add(umbracoApiControllerTypes);

            // both TinyMceValueConverter (in Core) and RteMacroRenderingValueConverter (in Web) will be
            // discovered when CoreBootManager configures the converters. We HAVE to remove one of them
            // here because there cannot be two converters for one property editor - and we want the full
            // RteMacroRenderingValueConverter that converts macros, etc. So remove TinyMceValueConverter.
            // (the limited one, defined in Core, is there for tests) - same for others
            composition.PropertyValueConverters()
                .Remove<TinyMceValueConverter>()
                .Remove<TextStringValueConverter>()
                .Remove<MarkdownEditorValueConverter>();

            // add all known factories, devs can then modify this list on application
            // startup either by binding to events or in their own global.asax
            composition.FilteredControllerFactory()
                .Append<RenderControllerFactory>();

            composition.UrlProviders()
                .Append<AliasUrlProvider>()
                .Append<DefaultUrlProvider>();

            composition.MediaUrlProviders()
                .Append<DefaultMediaUrlProvider>();

            composition.RegisterUnique<IImageUrlGenerator, ImageProcessorImageUrlGenerator>();

            composition.RegisterUnique<IContentLastChanceFinder, ContentFinderByConfigured404>();

            composition.ContentFinders()
                // all built-in finders in the correct order,
                // devs can then modify this list on application startup
                .Append<ContentFinderByPageIdQuery>()
                .Append<ContentFinderByUrl>()
                .Append<ContentFinderByIdPath>()
                //.Append<ContentFinderByUrlAndTemplate>() // disabled, this is an odd finder
                .Append<ContentFinderByUrlAlias>()
                .Append<ContentFinderByRedirectUrl>();

            composition.RegisterUnique<ISiteDomainHelper, SiteDomainHelper>();

            // register *all* checks, except those marked [HideFromTypeFinder] of course
            composition.HealthChecks()
                .Add(() => composition.TypeLoader.GetTypes<HealthCheck.HealthCheck>());

            composition.WithCollectionBuilder<HealthCheckNotificationMethodCollectionBuilder>()
                .Add(() => composition.TypeLoader.GetTypes<HealthCheck.NotificationMethods.IHealthCheckNotificationMethod>());

            // auto-register views
            composition.RegisterAuto(typeof(UmbracoViewPage<>));

            // register published router
            composition.RegisterUnique<IPublishedRouter, PublishedRouter>();

            // register preview SignalR hub
            composition.RegisterUnique(_ => GlobalHost.ConnectionManager.GetHubContext<PreviewHub>());

            // register properties fallback
            composition.RegisterUnique<IPublishedValueFallback, PublishedValueFallback>();

            // register known content apps
            composition.ContentApps()
                .Append<ListViewContentAppFactory>()
                .Append<ContentEditorContentAppFactory>()
                .Append<ContentInfoContentAppFactory>();

            // register back office sections in the order we want them rendered
            composition.Sections()
                .Append<ContentSection>()
                .Append<MediaSection>()
                .Append<SettingsSection>()
                .Append<PackagesSection>()
                .Append<UsersSection>()
                .Append<MembersSection>()
                .Append<FormsSection>()
                .Append<TranslationSection>();

            // register back office trees
            // the collection builder only accepts types inheriting from TreeControllerBase
            // and will filter out those that are not attributed with TreeAttribute
            composition.Trees()
                .AddTreeControllers(umbracoApiControllerTypes.Where(x => typeof(TreeControllerBase).IsAssignableFrom(x)));

            // register OEmbed providers - no type scanning - all explicit opt-in of adding types
            // note: IEmbedProvider is not IDiscoverable - think about it if going for type scanning
            composition.OEmbedProviders()
                .Append<YouTube>()
                .Append<Instagram>()
                .Append<Twitter>()
                .Append<Vimeo>()
                .Append<DailyMotion>()
                .Append<Flickr>()
                .Append<Slideshare>()
                .Append<Kickstarter>()
                .Append<GettyImages>()
                .Append<Ted>()
                .Append<Soundcloud>()
                .Append<Issuu>()
                .Append<Hulu>()
                .Append<Giphy>();


            // replace with web implementation
            composition.RegisterUnique<IPublishedSnapshotRebuilder, Migrations.PostMigrations.PublishedSnapshotRebuilder>();
        }
    }
}


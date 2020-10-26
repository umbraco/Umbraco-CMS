using System;
using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.CompositionExtensions;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Grid;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Dashboards;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Events;
using Umbraco.Core.Hosting;
using Umbraco.Core.Install;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Core.Media;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Scoping;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;
using Umbraco.Core.Templates;
using Umbraco.Examine;
using Umbraco.Infrastructure.Examine;
using Umbraco.Infrastructure.Media;
using Umbraco.Web;
using Umbraco.Web.Actions;
using Umbraco.Web.Cache;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Editors;
using Umbraco.Web.Features;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.HealthCheck.NotificationMethods;
using Umbraco.Web.Install;
using Umbraco.Web.Media;
using Umbraco.Web.Media.EmbedProviders;
using Umbraco.Web.Migrations.PostMigrations;
using Umbraco.Web.Models.PublishedContent;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.PropertyEditors.ValueConverters;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Search;
using Umbraco.Web.Sections;
using Umbraco.Web.Services;
using Umbraco.Web.Templates;
using Umbraco.Web.Trees;
using IntegerValidator = Umbraco.Core.PropertyEditors.Validators.IntegerValidator;
using TextStringValueConverter = Umbraco.Core.PropertyEditors.ValueConverters.TextStringValueConverter;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.HealthCheck;
using Umbraco.Core.HealthCheck.Checks;

namespace Umbraco.Core.Runtime
{
    // core's initial composer composes before all core composers
    [ComposeBefore(typeof(ICoreComposer))]
    public class CoreInitialComposer : ComponentComposer<CoreInitialComponent>
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            // composers
            composition
                .ComposeRepositories()
                .ComposeServices()
                .ComposeCoreMappingProfiles()
                .ComposeFileSystems();

            // register persistence mappers - required by database factory so needs to be done here
            // means the only place the collection can be modified is in a runtime - afterwards it
            // has been frozen and it is too late
            composition.Mappers().AddCoreMappers();

            // register the scope provider
            composition.RegisterUnique<ScopeProvider>(); // implements both IScopeProvider and IScopeAccessor
            composition.RegisterUnique<IScopeProvider>(f => f.GetInstance<ScopeProvider>());
            composition.RegisterUnique<IScopeAccessor>(f => f.GetInstance<ScopeProvider>());

            composition.RegisterUnique<IJsonSerializer, JsonNetSerializer>();
            composition.RegisterUnique<IMenuItemCollectionFactory, MenuItemCollectionFactory>();
            composition.RegisterUnique<InstallStatusTracker>();

            // register database builder
            // *not* a singleton, don't want to keep it around
            composition.Services.AddTransient<DatabaseBuilder>();

            // register manifest parser, will be injected in collection builders where needed
            composition.RegisterUnique<IManifestParser, ManifestParser>();

            // register our predefined validators
            composition.ManifestValueValidators()
                .Add<RequiredValidator>()
                .Add<RegexValidator>()
                .Add<DelimitedValueValidator>()
                .Add<EmailValidator>()
                .Add<IntegerValidator>()
                .Add<DecimalValidator>();

            // register the manifest filter collection builder (collection is empty by default)
            composition.ManifestFilters();

            // properties and parameters derive from data editors
            composition.DataEditors()
                .Add(() => composition.TypeLoader.GetDataEditors());

            composition.MediaUrlGenerators()
                .Add<FileUploadPropertyEditor>()
                .Add<ImageCropperPropertyEditor>();

            composition.RegisterUnique<PropertyEditorCollection>();
            composition.RegisterUnique<ParameterEditorCollection>();

            // Used to determine if a datatype/editor should be storing/tracking
            // references to media item/s
            composition.DataValueReferenceFactories();

            // register a server registrar, by default it's the db registrar
            composition.RegisterUnique<IServerRegistrar>(f =>
            {
                var globalSettings = f.GetInstance<IOptions<GlobalSettings>>().Value;

                // TODO:  we still register the full IServerMessenger because
                // even on 1 single server we can have 2 concurrent app domains
                var singleServer = globalSettings.DisableElectionForSingleServer;
                return singleServer
                    ? (IServerRegistrar) new SingleServerRegistrar(f.GetInstance<IRequestAccessor>())
                    : new DatabaseServerRegistrar(
                        new Lazy<IServerRegistrationService>(f.GetInstance<IServerRegistrationService>),
                        new DatabaseServerRegistrarOptions());
            });

            // by default we'll use the database server messenger with default options (no callbacks),
            // this will be overridden by the db thing in the corresponding components in the web
            // project
            composition.RegisterUnique<IServerMessenger>(factory
                => new DatabaseServerMessenger(
                    factory.GetInstance<IMainDom>(),
                    factory.GetInstance<IScopeProvider>(),
                    factory.GetInstance<ISqlContext>(),
                    factory.GetInstance<IProfilingLogger>(),
                    factory.GetInstance<ILogger<DatabaseServerMessenger>>(),
                    factory.GetInstance<IServerRegistrar>(),
                    true, new DatabaseServerMessengerOptions(),
                    factory.GetInstance<IHostingEnvironment>(),
                    factory.GetInstance<CacheRefresherCollection>()
                ));

            composition.CacheRefreshers()
                .Add(() => composition.TypeLoader.GetCacheRefreshers());

            composition.PackageActions()
                .Add(() => composition.TypeLoader.GetPackageActions());

            composition.PropertyValueConverters()
                .Append(composition.TypeLoader.GetTypes<IPropertyValueConverter>());

            composition.RegisterUnique<IPublishedContentTypeFactory, PublishedContentTypeFactory>();

            composition.RegisterUnique<IShortStringHelper>(factory
                => new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(factory.GetInstance<IOptions<RequestHandlerSettings>>().Value)));

            composition.UrlSegmentProviders()
                .Append<DefaultUrlSegmentProvider>();

            composition.RegisterUnique<IMigrationBuilder>(factory => new MigrationBuilder(factory));

            // by default, register a noop factory
            composition.RegisterUnique<IPublishedModelFactory, NoopPublishedModelFactory>();

            // by default
            composition.RegisterUnique<IPublishedSnapshotRebuilder, PublishedSnapshotRebuilder>();

            composition.SetCultureDictionaryFactory<DefaultCultureDictionaryFactory>();
            composition.Register(f => f.GetInstance<ICultureDictionaryFactory>().CreateDictionary(), Lifetime.Singleton);
            composition.RegisterUnique<UriUtility>();

            // register the published snapshot accessor - the "current" published snapshot is in the umbraco context
            composition.RegisterUnique<IPublishedSnapshotAccessor, UmbracoContextPublishedSnapshotAccessor>();

            composition.RegisterUnique<IVariationContextAccessor, HybridVariationContextAccessor>();

            composition.RegisterUnique<IDashboardService, DashboardService>();

            // register core CMS dashboards and 3rd party types - will be ordered by weight attribute & merged with package.manifest dashboards
            composition.Dashboards()
                .Add(composition.TypeLoader.GetTypes<IDashboard>());

            // will be injected in controllers when needed to invoke rest endpoints on Our
            composition.RegisterUnique<IInstallationService, InstallationService>();
            composition.RegisterUnique<IUpgradeService, UpgradeService>();

            // Grid config is not a real config file as we know them
            composition.RegisterUnique<IGridConfig, GridConfig>();

            // Config manipulator
            composition.RegisterUnique<IConfigManipulator, JsonConfigManipulator>();
            
            // register the umbraco context factory
            // composition.RegisterUnique<IUmbracoContextFactory, UmbracoContextFactory>();
            composition.RegisterUnique<IPublishedUrlProvider, UrlProvider>();

            composition.RegisterUnique<HtmlLocalLinkParser>();
            composition.RegisterUnique<HtmlImageSourceParser>();
            composition.RegisterUnique<HtmlUrlParser>();
            composition.RegisterUnique<RichTextEditorPastedImages>();
            composition.RegisterUnique<BlockEditorConverter>();

            // both TinyMceValueConverter (in Core) and RteMacroRenderingValueConverter (in Web) will be
            // discovered when CoreBootManager configures the converters. We HAVE to remove one of them
            // here because there cannot be two converters for one property editor - and we want the full
            // RteMacroRenderingValueConverter that converts macros, etc. So remove TinyMceValueConverter.
            // (the limited one, defined in Core, is there for tests) - same for others
            composition.PropertyValueConverters()
                .Remove<TinyMceValueConverter>()
                .Remove<TextStringValueConverter>()
                .Remove<MarkdownEditorValueConverter>();

            composition.UrlProviders()
                .Append<AliasUrlProvider>()
                .Append<DefaultUrlProvider>();

            composition.MediaUrlProviders()
                .Append<DefaultMediaUrlProvider>();

            composition.RegisterUnique<ISiteDomainHelper, SiteDomainHelper>();

            // register properties fallback
            composition.RegisterUnique<IPublishedValueFallback, PublishedValueFallback>();

            composition.RegisterUnique<IImageUrlGenerator, ImageSharpImageUrlGenerator>();

            composition.RegisterUnique<UmbracoFeatures>();

            composition.Actions()
                .Add(() => composition.TypeLoader.GetTypes<IAction>());

            composition.EditorValidators()
                .Add(() => composition.TypeLoader.GetTypes<IEditorValidator>());


            composition.TourFilters();

            // replace with web implementation
            composition.RegisterUnique<IPublishedSnapshotRebuilder, PublishedSnapshotRebuilder>();

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

            // register known content apps
            composition.ContentApps()
                .Append<ListViewContentAppFactory>()
                .Append<ContentEditorContentAppFactory>()
                .Append<ContentInfoContentAppFactory>()
                .Append<ContentTypeDesignContentAppFactory>()
                .Append<ContentTypeListViewContentAppFactory>()
                .Append<ContentTypePermissionsContentAppFactory>()
                .Append<ContentTypeTemplatesContentAppFactory>();

            // register published router
            composition.RegisterUnique<IPublishedRouter, PublishedRouter>();

            // register *all* checks, except those marked [HideFromTypeFinder] of course
            composition.HealthChecks()
                .Add(() => composition.TypeLoader.GetTypes<HealthCheck.HealthCheck>());


            composition.WithCollectionBuilder<HealthCheckNotificationMethodCollectionBuilder>()
                .Add(() => composition.TypeLoader.GetTypes<IHealthCheckNotificationMethod>());

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

            composition.Services.AddScoped<UmbracoTreeSearcher>();

            composition.SearchableTrees()
                .Add(() => composition.TypeLoader.GetTypes<ISearchableTree>());

            // replace some services
            composition.RegisterUnique<IEventMessagesFactory, DefaultEventMessagesFactory>();
            composition.RegisterUnique<IEventMessagesAccessor, HybridEventMessagesAccessor>();
            composition.RegisterUnique<ITreeService, TreeService>();
            composition.RegisterUnique<ISectionService, SectionService>();
            composition.RegisterUnique<IEmailSender, EmailSender>();

            composition.RegisterUnique<IExamineManager, ExamineManager>();

            // register distributed cache
            composition.RegisterUnique(f => new DistributedCache(f.GetInstance<IServerMessenger>(), f.GetInstance<CacheRefresherCollection>()));


            composition.Services.AddScoped<ITagQuery, TagQuery>();

            composition.RegisterUnique<HtmlLocalLinkParser>();
            composition.RegisterUnique<HtmlUrlParser>();
            composition.RegisterUnique<HtmlImageSourceParser>();
            composition.RegisterUnique<RichTextEditorPastedImages>();

            composition.RegisterUnique<IUmbracoTreeSearcherFields, UmbracoTreeSearcherFields>();
            composition.Register<IPublishedContentQuery>(factory =>
            {
                var umbCtx = factory.GetInstance<IUmbracoContextAccessor>();
                return new PublishedContentQuery(umbCtx.UmbracoContext.PublishedSnapshot, factory.GetInstance<IVariationContextAccessor>(), factory.GetInstance<IExamineManager>());
            }, Lifetime.Request);

            composition.RegisterUnique<IPublishedUrlProvider, UrlProvider>();

            // register the http context and umbraco context accessors
            // we *should* use the HttpContextUmbracoContextAccessor, however there are cases when
            // we have no http context, eg when booting Umbraco or in background threads, so instead
            // let's use an hybrid accessor that can fall back to a ThreadStatic context.
            composition.RegisterUnique<IUmbracoContextAccessor, HybridUmbracoContextAccessor>();

            // register accessors for cultures
            composition.RegisterUnique<IDefaultCultureAccessor, DefaultCultureAccessor>();

            composition.Services.AddSingleton<IFilePermissionHelper, FilePermissionHelper>();

            composition.RegisterUnique<IUmbracoComponentRenderer, UmbracoComponentRenderer>();

            // Register noop versions for examine to be overridden by examine
            composition.RegisterUnique<IUmbracoIndexesCreator, NoopUmbracoIndexesCreator>();
            composition.RegisterUnique<IBackOfficeExamineSearcher, NoopBackOfficeExamineSearcher>();

            composition.RegisterUnique<UploadAutoFillProperties>();
        }
    }
}

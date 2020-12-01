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
using Umbraco.Core.Builder;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.HealthCheck;
using Umbraco.Core.HealthCheck.Checks;
using Umbraco.Core.Security;

namespace Umbraco.Core.Runtime
{
    // core's initial composer composes before all core composers
    [ComposeBefore(typeof(ICoreComposer))]
    public class CoreInitialComposer : ComponentComposer<CoreInitialComponent>
    {
        public override void Compose(IUmbracoBuilder builder)
        {
            base.Compose(builder);

            // composers
            builder
                .ComposeRepositories()
                .ComposeServices()
                .ComposeCoreMappingProfiles()
                .ComposeFileSystems();

            // register persistence mappers - required by database factory so needs to be done here
            // means the only place the collection can be modified is in a runtime - afterwards it
            // has been frozen and it is too late
            builder.Mappers().AddCoreMappers();

            // register the scope provider
            builder.Services.AddUnique<ScopeProvider>(); // implements both IScopeProvider and IScopeAccessor
            builder.Services.AddUnique<IScopeProvider>(f => f.GetRequiredService<ScopeProvider>());
            builder.Services.AddUnique<IScopeAccessor>(f => f.GetRequiredService<ScopeProvider>());

            builder.Services.AddUnique<IJsonSerializer, JsonNetSerializer>();
            builder.Services.AddUnique<IConfigurationEditorJsonSerializer, ConfigurationEditorJsonSerializer>();
            builder.Services.AddUnique<IMenuItemCollectionFactory, MenuItemCollectionFactory>();
            builder.Services.AddUnique<InstallStatusTracker>();

            // register database builder
            // *not* a singleton, don't want to keep it around
            builder.Services.AddTransient<DatabaseBuilder>();

            // register manifest parser, will be injected in collection builders where needed
            builder.Services.AddUnique<IManifestParser, ManifestParser>();

            // register our predefined validators
            builder.ManifestValueValidators()
                .Add<RequiredValidator>()
                .Add<RegexValidator>()
                .Add<DelimitedValueValidator>()
                .Add<EmailValidator>()
                .Add<IntegerValidator>()
                .Add<DecimalValidator>();

            // register the manifest filter collection builder (collection is empty by default)
            builder.ManifestFilters();

            // properties and parameters derive from data editors
            builder.DataEditors()
                .Add(() => builder.TypeLoader.GetDataEditors());

            builder.MediaUrlGenerators()
                .Add<FileUploadPropertyEditor>()
                .Add<ImageCropperPropertyEditor>();

            builder.Services.AddUnique<PropertyEditorCollection>();
            builder.Services.AddUnique<ParameterEditorCollection>();

            // Used to determine if a datatype/editor should be storing/tracking
            // references to media item/s
            builder.DataValueReferenceFactories();

            // register a server registrar, by default it's the db registrar
            builder.Services.AddUnique<IServerRegistrar>(f =>
            {
                var globalSettings = f.GetRequiredService<IOptions<GlobalSettings>>().Value;

                // TODO:  we still register the full IServerMessenger because
                // even on 1 single server we can have 2 concurrent app domains
                var singleServer = globalSettings.DisableElectionForSingleServer;
                return singleServer
                    ? (IServerRegistrar) new SingleServerRegistrar(f.GetRequiredService<IRequestAccessor>())
                    : new DatabaseServerRegistrar(
                        new Lazy<IServerRegistrationService>(f.GetRequiredService<IServerRegistrationService>));
            });

            // by default we'll use the database server messenger with default options (no callbacks),
            // this will be overridden by the db thing in the corresponding components in the web
            // project
            builder.Services.AddUnique<IServerMessenger>(factory
                => new DatabaseServerMessenger(
                    factory.GetRequiredService<IMainDom>(),
                    factory.GetRequiredService<IScopeProvider>(),
                    factory.GetRequiredService<IUmbracoDatabaseFactory>(),
                    factory.GetRequiredService<IProfilingLogger>(),
                    factory.GetRequiredService<ILogger<DatabaseServerMessenger>>(),
                    factory.GetRequiredService<IServerRegistrar>(),
                    true,
                    new DatabaseServerMessengerCallbacks(),
                    factory.GetRequiredService<IHostingEnvironment>(),
                    factory.GetRequiredService<CacheRefresherCollection>(),
                    factory.GetRequiredService<IOptions<GlobalSettings>>()
                ));

            builder.CacheRefreshers()
                .Add(() => builder.TypeLoader.GetCacheRefreshers());

            builder.PackageActions()
                .Add(() => builder.TypeLoader.GetPackageActions());

            builder.PropertyValueConverters()
                .Append(builder.TypeLoader.GetTypes<IPropertyValueConverter>());

            builder.Services.AddUnique<IPublishedContentTypeFactory, PublishedContentTypeFactory>();

            builder.Services.AddUnique<IShortStringHelper>(factory
                => new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(factory.GetRequiredService<IOptions<RequestHandlerSettings>>().Value)));

            builder.UrlSegmentProviders()
                .Append<DefaultUrlSegmentProvider>();

            builder.Services.AddUnique<IMigrationBuilder>(factory => new MigrationBuilder(factory));

            // by default, register a noop factory
            builder.Services.AddUnique<IPublishedModelFactory, NoopPublishedModelFactory>();

            // by default
            builder.Services.AddUnique<IPublishedSnapshotRebuilder, PublishedSnapshotRebuilder>();

            builder.SetCultureDictionaryFactory<DefaultCultureDictionaryFactory>();
            builder.Services.AddSingleton(f => f.GetRequiredService<ICultureDictionaryFactory>().CreateDictionary());
            builder.Services.AddUnique<UriUtility>();

            // register the published snapshot accessor - the "current" published snapshot is in the umbraco context
            builder.Services.AddUnique<IPublishedSnapshotAccessor, UmbracoContextPublishedSnapshotAccessor>();

            builder.Services.AddUnique<IVariationContextAccessor, HybridVariationContextAccessor>();

            builder.Services.AddUnique<IDashboardService, DashboardService>();

            // register core CMS dashboards and 3rd party types - will be ordered by weight attribute & merged with package.manifest dashboards
            builder.Dashboards()
                .Add(builder.TypeLoader.GetTypes<IDashboard>());

            // will be injected in controllers when needed to invoke rest endpoints on Our
            builder.Services.AddUnique<IInstallationService, InstallationService>();
            builder.Services.AddUnique<IUpgradeService, UpgradeService>();

            // Grid config is not a real config file as we know them
            builder.Services.AddUnique<IGridConfig, GridConfig>();

            // Config manipulator
            builder.Services.AddUnique<IConfigManipulator, JsonConfigManipulator>();

            // register the umbraco context factory
            // composition.Services.AddUnique<IUmbracoContextFactory, UmbracoContextFactory>();
            builder.Services.AddUnique<IPublishedUrlProvider, UrlProvider>();

            builder.Services.AddUnique<HtmlLocalLinkParser>();
            builder.Services.AddUnique<HtmlImageSourceParser>();
            builder.Services.AddUnique<HtmlUrlParser>();
            builder.Services.AddUnique<RichTextEditorPastedImages>();
            builder.Services.AddUnique<BlockEditorConverter>();

            // both TinyMceValueConverter (in Core) and RteMacroRenderingValueConverter (in Web) will be
            // discovered when CoreBootManager configures the converters. We HAVE to remove one of them
            // here because there cannot be two converters for one property editor - and we want the full
            // RteMacroRenderingValueConverter that converts macros, etc. So remove TinyMceValueConverter.
            // (the limited one, defined in Core, is there for tests) - same for others
            builder.PropertyValueConverters()
                .Remove<TinyMceValueConverter>()
                .Remove<TextStringValueConverter>()
                .Remove<MarkdownEditorValueConverter>();

            builder.UrlProviders()
                .Append<AliasUrlProvider>()
                .Append<DefaultUrlProvider>();

            builder.MediaUrlProviders()
                .Append<DefaultMediaUrlProvider>();

            builder.Services.AddUnique<ISiteDomainHelper, SiteDomainHelper>();

            // register properties fallback
            builder.Services.AddUnique<IPublishedValueFallback, PublishedValueFallback>();

            builder.Services.AddUnique<IImageUrlGenerator, ImageSharpImageUrlGenerator>();

            builder.Services.AddUnique<UmbracoFeatures>();

            builder.Actions()
                .Add(() => builder.TypeLoader.GetTypes<IAction>());

            builder.EditorValidators()
                .Add(() => builder.TypeLoader.GetTypes<IEditorValidator>());


            builder.TourFilters();

            // replace with web implementation
            builder.Services.AddUnique<IPublishedSnapshotRebuilder, PublishedSnapshotRebuilder>();

            // register OEmbed providers - no type scanning - all explicit opt-in of adding types
            // note: IEmbedProvider is not IDiscoverable - think about it if going for type scanning
            builder.OEmbedProviders()
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
            builder.Sections()
                .Append<ContentSection>()
                .Append<MediaSection>()
                .Append<SettingsSection>()
                .Append<PackagesSection>()
                .Append<UsersSection>()
                .Append<MembersSection>()
                .Append<FormsSection>()
                .Append<TranslationSection>();

            // register known content apps
            builder.ContentApps()
                .Append<ListViewContentAppFactory>()
                .Append<ContentEditorContentAppFactory>()
                .Append<ContentInfoContentAppFactory>()
                .Append<ContentTypeDesignContentAppFactory>()
                .Append<ContentTypeListViewContentAppFactory>()
                .Append<ContentTypePermissionsContentAppFactory>()
                .Append<ContentTypeTemplatesContentAppFactory>();

            // register published router
            builder.Services.AddUnique<IPublishedRouter, PublishedRouter>();

            // register *all* checks, except those marked [HideFromTypeFinder] of course
            builder.HealthChecks()
                .Add(() => builder.TypeLoader.GetTypes<HealthCheck.HealthCheck>());

            builder.WithCollectionBuilder<HealthCheckNotificationMethodCollectionBuilder>()
                .Add(() => builder.TypeLoader.GetTypes<IHealthCheckNotificationMethod>());

            builder.Services.AddUnique<IContentLastChanceFinder, ContentFinderByConfigured404>();

            builder.ContentFinders()
                // all built-in finders in the correct order,
                // devs can then modify this list on application startup
                .Append<ContentFinderByPageIdQuery>()
                .Append<ContentFinderByUrl>()
                .Append<ContentFinderByIdPath>()
                //.Append<ContentFinderByUrlAndTemplate>() // disabled, this is an odd finder
                .Append<ContentFinderByUrlAlias>()
                .Append<ContentFinderByRedirectUrl>();

            builder.Services.AddScoped<UmbracoTreeSearcher>();

            builder.SearchableTrees()
                .Add(() => builder.TypeLoader.GetTypes<ISearchableTree>());

            // replace some services
            builder.Services.AddUnique<IEventMessagesFactory, DefaultEventMessagesFactory>();
            builder.Services.AddUnique<IEventMessagesAccessor, HybridEventMessagesAccessor>();
            builder.Services.AddUnique<ITreeService, TreeService>();
            builder.Services.AddUnique<ISectionService, SectionService>();
            builder.Services.AddUnique<IEmailSender, EmailSender>();
            builder.Services.AddUnique<ISmsSender, NotImplementedSmsSender>();

            builder.Services.AddUnique<IExamineManager, ExamineManager>();

            // register distributed cache
            builder.Services.AddUnique(f => new DistributedCache(f.GetRequiredService<IServerMessenger>(), f.GetRequiredService<CacheRefresherCollection>()));


            builder.Services.AddScoped<ITagQuery, TagQuery>();

            builder.Services.AddUnique<HtmlLocalLinkParser>();
            builder.Services.AddUnique<HtmlUrlParser>();
            builder.Services.AddUnique<HtmlImageSourceParser>();
            builder.Services.AddUnique<RichTextEditorPastedImages>();

            builder.Services.AddUnique<IUmbracoTreeSearcherFields, UmbracoTreeSearcherFields>();
            builder.Services.AddScoped<IPublishedContentQuery>(factory =>
            {
                var umbCtx = factory.GetRequiredService<IUmbracoContextAccessor>();
                return new PublishedContentQuery(umbCtx.UmbracoContext.PublishedSnapshot, factory.GetRequiredService<IVariationContextAccessor>(), factory.GetRequiredService<IExamineManager>());
            });

            builder.Services.AddUnique<IPublishedUrlProvider, UrlProvider>();

            // register the http context and umbraco context accessors
            // we *should* use the HttpContextUmbracoContextAccessor, however there are cases when
            // we have no http context, eg when booting Umbraco or in background threads, so instead
            // let's use an hybrid accessor that can fall back to a ThreadStatic context.
            builder.Services.AddUnique<IUmbracoContextAccessor, HybridUmbracoContextAccessor>();

            // register accessors for cultures
            builder.Services.AddUnique<IDefaultCultureAccessor, DefaultCultureAccessor>();

            builder.Services.AddSingleton<IFilePermissionHelper, FilePermissionHelper>();

            builder.Services.AddUnique<IUmbracoComponentRenderer, UmbracoComponentRenderer>();

            // Register noop versions for examine to be overridden by examine
            builder.Services.AddUnique<IUmbracoIndexesCreator, NoopUmbracoIndexesCreator>();
            builder.Services.AddUnique<IBackOfficeExamineSearcher, NoopBackOfficeExamineSearcher>();

            builder.Services.AddUnique<UploadAutoFillProperties>();

            builder.Services.AddUnique<ICronTabParser, NCronTabParser>();

            builder.Services.AddUnique(factory => new LegacyPasswordSecurity());
            builder.Services.AddUnique<UserEditorAuthorizationHelper>();
            builder.Services.AddUnique<ContentPermissions>();
            builder.Services.AddUnique<MediaPermissions>();
        }
    }
}

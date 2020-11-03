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
            composition.Services.AddUnique<ScopeProvider>(); // implements both IScopeProvider and IScopeAccessor
            composition.Services.AddUnique<IScopeProvider>(f => f.GetRequiredService<ScopeProvider>());
            composition.Services.AddUnique<IScopeAccessor>(f => f.GetRequiredService<ScopeProvider>());

            composition.Services.AddUnique<IJsonSerializer, JsonNetSerializer>();
            composition.Services.AddUnique<IMenuItemCollectionFactory, MenuItemCollectionFactory>();
            composition.Services.AddUnique<InstallStatusTracker>();

            // register database builder
            // *not* a singleton, don't want to keep it around
            composition.Services.AddTransient<DatabaseBuilder>();

            // register manifest parser, will be injected in collection builders where needed
            composition.Services.AddUnique<IManifestParser, ManifestParser>();

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

            composition.Services.AddUnique<PropertyEditorCollection>();
            composition.Services.AddUnique<ParameterEditorCollection>();

            // Used to determine if a datatype/editor should be storing/tracking
            // references to media item/s
            composition.DataValueReferenceFactories();

            // register a server registrar, by default it's the db registrar
            composition.Services.AddUnique<IServerRegistrar>(f =>
            {
                var globalSettings = f.GetRequiredService<IOptions<GlobalSettings>>().Value;

                // TODO:  we still register the full IServerMessenger because
                // even on 1 single server we can have 2 concurrent app domains
                var singleServer = globalSettings.DisableElectionForSingleServer;
                return singleServer
                    ? (IServerRegistrar) new SingleServerRegistrar(f.GetRequiredService<IRequestAccessor>())
                    : new DatabaseServerRegistrar(
                        new Lazy<IServerRegistrationService>(f.GetRequiredService<IServerRegistrationService>),
                        new DatabaseServerRegistrarOptions());
            });

            // by default we'll use the database server messenger with default options (no callbacks),
            // this will be overridden by the db thing in the corresponding components in the web
            // project
            composition.Services.AddUnique<IServerMessenger>(factory
                => new DatabaseServerMessenger(
                    factory.GetRequiredService<IMainDom>(),
                    factory.GetRequiredService<IScopeProvider>(),
                    factory.GetRequiredService<ISqlContext>(),
                    factory.GetRequiredService<IProfilingLogger>(),
                    factory.GetRequiredService<ILogger<DatabaseServerMessenger>>(),
                    factory.GetRequiredService<IServerRegistrar>(),
                    true, new DatabaseServerMessengerOptions(),
                    factory.GetRequiredService<IHostingEnvironment>(),
                    factory.GetRequiredService<CacheRefresherCollection>()
                ));

            composition.CacheRefreshers()
                .Add(() => composition.TypeLoader.GetCacheRefreshers());

            composition.PackageActions()
                .Add(() => composition.TypeLoader.GetPackageActions());

            composition.PropertyValueConverters()
                .Append(composition.TypeLoader.GetTypes<IPropertyValueConverter>());

            composition.Services.AddUnique<IPublishedContentTypeFactory, PublishedContentTypeFactory>();

            composition.Services.AddUnique<IShortStringHelper>(factory
                => new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(factory.GetRequiredService<IOptions<RequestHandlerSettings>>().Value)));

            composition.UrlSegmentProviders()
                .Append<DefaultUrlSegmentProvider>();

            composition.Services.AddUnique<IMigrationBuilder>(factory => new MigrationBuilder(factory));

            // by default, register a noop factory
            composition.Services.AddUnique<IPublishedModelFactory, NoopPublishedModelFactory>();

            // by default
            composition.Services.AddUnique<IPublishedSnapshotRebuilder, PublishedSnapshotRebuilder>();

            composition.SetCultureDictionaryFactory<DefaultCultureDictionaryFactory>();
            composition.Services.AddSingleton(f => f.GetRequiredService<ICultureDictionaryFactory>().CreateDictionary());
            composition.Services.AddUnique<UriUtility>();

            // register the published snapshot accessor - the "current" published snapshot is in the umbraco context
            composition.Services.AddUnique<IPublishedSnapshotAccessor, UmbracoContextPublishedSnapshotAccessor>();

            composition.Services.AddUnique<IVariationContextAccessor, HybridVariationContextAccessor>();

            composition.Services.AddUnique<IDashboardService, DashboardService>();

            // register core CMS dashboards and 3rd party types - will be ordered by weight attribute & merged with package.manifest dashboards
            composition.Dashboards()
                .Add(composition.TypeLoader.GetTypes<IDashboard>());

            // will be injected in controllers when needed to invoke rest endpoints on Our
            composition.Services.AddUnique<IInstallationService, InstallationService>();
            composition.Services.AddUnique<IUpgradeService, UpgradeService>();

            // Grid config is not a real config file as we know them
            composition.Services.AddUnique<IGridConfig, GridConfig>();

            // Config manipulator
            composition.Services.AddUnique<IConfigManipulator, JsonConfigManipulator>();

            // register the umbraco context factory
            // composition.Services.AddUnique<IUmbracoContextFactory, UmbracoContextFactory>();
            composition.Services.AddUnique<IPublishedUrlProvider, UrlProvider>();

            composition.Services.AddUnique<HtmlLocalLinkParser>();
            composition.Services.AddUnique<HtmlImageSourceParser>();
            composition.Services.AddUnique<HtmlUrlParser>();
            composition.Services.AddUnique<RichTextEditorPastedImages>();
            composition.Services.AddUnique<BlockEditorConverter>();

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

            composition.Services.AddUnique<ISiteDomainHelper, SiteDomainHelper>();

            // register properties fallback
            composition.Services.AddUnique<IPublishedValueFallback, PublishedValueFallback>();

            composition.Services.AddUnique<IImageUrlGenerator, ImageSharpImageUrlGenerator>();

            composition.Services.AddUnique<UmbracoFeatures>();

            composition.Actions()
                .Add(() => composition.TypeLoader.GetTypes<IAction>());

            composition.EditorValidators()
                .Add(() => composition.TypeLoader.GetTypes<IEditorValidator>());


            composition.TourFilters();

            // replace with web implementation
            composition.Services.AddUnique<IPublishedSnapshotRebuilder, PublishedSnapshotRebuilder>();

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
            composition.Services.AddUnique<IPublishedRouter, PublishedRouter>();

            // register *all* checks, except those marked [HideFromTypeFinder] of course
            composition.HealthChecks()
                .Add(() => composition.TypeLoader.GetTypes<HealthCheck.HealthCheck>());

            composition.WithCollectionBuilder<HealthCheckNotificationMethodCollectionBuilder>()
                .Add(() => composition.TypeLoader.GetTypes<IHealthCheckNotificationMethod>());

            composition.Services.AddUnique<IContentLastChanceFinder, ContentFinderByConfigured404>();

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
            composition.Services.AddUnique<IEventMessagesFactory, DefaultEventMessagesFactory>();
            composition.Services.AddUnique<IEventMessagesAccessor, HybridEventMessagesAccessor>();
            composition.Services.AddUnique<ITreeService, TreeService>();
            composition.Services.AddUnique<ISectionService, SectionService>();
            composition.Services.AddUnique<IEmailSender, EmailSender>();

            composition.Services.AddUnique<IExamineManager, ExamineManager>();

            // register distributed cache
            composition.Services.AddUnique(f => new DistributedCache(f.GetRequiredService<IServerMessenger>(), f.GetRequiredService<CacheRefresherCollection>()));


            composition.Services.AddScoped<ITagQuery, TagQuery>();

            composition.Services.AddUnique<HtmlLocalLinkParser>();
            composition.Services.AddUnique<HtmlUrlParser>();
            composition.Services.AddUnique<HtmlImageSourceParser>();
            composition.Services.AddUnique<RichTextEditorPastedImages>();

            composition.Services.AddUnique<IUmbracoTreeSearcherFields, UmbracoTreeSearcherFields>();
            composition.Services.AddScoped<IPublishedContentQuery>(factory =>
            {
                var umbCtx = factory.GetRequiredService<IUmbracoContextAccessor>();
                return new PublishedContentQuery(umbCtx.UmbracoContext.PublishedSnapshot, factory.GetRequiredService<IVariationContextAccessor>(), factory.GetRequiredService<IExamineManager>());
            });

            composition.Services.AddUnique<IPublishedUrlProvider, UrlProvider>();

            // register the http context and umbraco context accessors
            // we *should* use the HttpContextUmbracoContextAccessor, however there are cases when
            // we have no http context, eg when booting Umbraco or in background threads, so instead
            // let's use an hybrid accessor that can fall back to a ThreadStatic context.
            composition.Services.AddUnique<IUmbracoContextAccessor, HybridUmbracoContextAccessor>();

            // register accessors for cultures
            composition.Services.AddUnique<IDefaultCultureAccessor, DefaultCultureAccessor>();

            composition.Services.AddSingleton<IFilePermissionHelper, FilePermissionHelper>();

            composition.Services.AddUnique<IUmbracoComponentRenderer, UmbracoComponentRenderer>();

            // Register noop versions for examine to be overridden by examine
            composition.Services.AddUnique<IUmbracoIndexesCreator, NoopUmbracoIndexesCreator>();
            composition.Services.AddUnique<IBackOfficeExamineSearcher, NoopBackOfficeExamineSearcher>();

            composition.Services.AddUnique<UploadAutoFillProperties>();

            composition.Services.AddUnique<ICronTabParser, NCronTabParser>();
        }
    }
}

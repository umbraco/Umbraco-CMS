using System.Runtime.InteropServices;
using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Dashboards;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Hosting;
using Umbraco.Core.Install;
using Umbraco.Core.Mail;
using Umbraco.Core.Manifest;
using Umbraco.Core.Media;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Packaging;
using Umbraco.Core.Persistence;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Runtime;
using Umbraco.Core.Scoping;
using Umbraco.Core.Serialization;
using Umbraco.Core.Strings;
using Umbraco.Core.Templates;
using Umbraco.Examine;
using Umbraco.Infrastructure.Examine;
using Umbraco.Infrastructure.Media;
using Umbraco.Infrastructure.Runtime;
using Umbraco.Web;
using Umbraco.Web.Actions;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Editors;
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
using Umbraco.Web.Trees;
using TextStringValueConverter = Umbraco.Core.PropertyEditors.ValueConverters.TextStringValueConverter;

namespace Umbraco.Infrastructure.DependencyInjection
{
    public static partial class UmbracoBuilderExtensions
    {

        /*
         * TODO: Many of these things are not "Core" services and are probably not required to run
         *
         * This should be split up:
         *   - Distributed Cache
         *   - BackOffice
         *     - Manifest
         *     - Property Editors
         *     - Packages
         *     - Dashboards
         *     - OEmbed
         *     - Sections
         *     - Content Apps
         *     - Health Checks
         *     - ETC...
         *   - Installation
         *   - Front End
         */

        /// <summary>
        /// Adds all core Umbraco services required to run which may be replaced later in the pipeline
        /// </summary>
        public static IUmbracoBuilder AddCoreInitialServices(this IUmbracoBuilder builder)
        {
            builder.AddMainDom();

            builder.Services.AddUnique<IUmbracoDatabaseFactory, UmbracoDatabaseFactory>();
            builder.Services.AddUnique(factory => factory.GetRequiredService<IUmbracoDatabaseFactory>().CreateDatabase());
            builder.Services.AddUnique(factory => factory.GetRequiredService<IUmbracoDatabaseFactory>().SqlContext);
            builder.Services.AddUnique<IRuntimeState, RuntimeState>();
            builder.Services.AddUnique<IRuntime, CoreRuntime>();

            // composers
            builder
                .AddRepositories()
                .AddServices()
                .AddCoreMappingProfiles()
                .AddFileSystems();

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

            builder.Services.AddUnique<IPublishedSnapshotRebuilder, PublishedSnapshotRebuilder>();

            // register the published snapshot accessor - the "current" published snapshot is in the umbraco context
            builder.Services.AddUnique<IPublishedSnapshotAccessor, UmbracoContextPublishedSnapshotAccessor>();

            builder.Services.AddUnique<IVariationContextAccessor, HybridVariationContextAccessor>();

            // register core CMS dashboards and 3rd party types - will be ordered by weight attribute & merged with package.manifest dashboards
            builder.Dashboards()
                .Add(builder.TypeLoader.GetTypes<IDashboard>());

            // Config manipulator
            builder.Services.AddUnique<IConfigManipulator, JsonConfigManipulator>();

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

            builder.Services.AddUnique<IImageUrlGenerator, ImageSharpImageUrlGenerator>();

            builder.Actions()
                .Add(() => builder.TypeLoader.GetTypes<IAction>());

            builder.EditorValidators()
                .Add(() => builder.TypeLoader.GetTypes<IEditorValidator>());

            builder.TourFilters();

            builder.Services.AddUnique<IPublishedSnapshotRebuilder, PublishedSnapshotRebuilder>();

            // register OEmbed providers - no type scanning - all explicit opt-in of adding types
            // note: IEmbedProvider is not IDiscoverable - think about it if going for type scanning
            builder.OEmbedProviders()
                .Append<YouTube>()
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

            // register *all* checks, except those marked [HideFromTypeFinder] of course
            builder.Services.AddUnique<IMarkdownToHtmlConverter, MarkdownToHtmlConverter>();
            builder.HealthChecks()
                .Add(() => builder.TypeLoader.GetTypes<Core.HealthCheck.HealthCheck>());

            builder.WithCollectionBuilder<HealthCheckNotificationMethodCollectionBuilder>()
                .Add(() => builder.TypeLoader.GetTypes<IHealthCheckNotificationMethod>());

            builder.Services.AddUnique<IContentLastChanceFinder, ContentFinderByConfigured404>();

            builder.ContentFinders()

                // all built-in finders in the correct order,
                // devs can then modify this list on application startup
                .Append<ContentFinderByPageIdQuery>()
                .Append<ContentFinderByUrl>()
                .Append<ContentFinderByIdPath>()

                // .Append<ContentFinderByUrlAndTemplate>() // disabled, this is an odd finder
                .Append<ContentFinderByUrlAlias>()
                .Append<ContentFinderByRedirectUrl>();

            builder.Services.AddScoped<UmbracoTreeSearcher>();

            builder.SearchableTrees()
                .Add(() => builder.TypeLoader.GetTypes<ISearchableTree>());

            // replace
            builder.Services.AddUnique<IEmailSender, EmailSender>();

            builder.Services.AddUnique<IExamineManager, ExamineManager>();

            builder.Services.AddScoped<ITagQuery, TagQuery>();

            builder.Services.AddUnique<IUmbracoTreeSearcherFields, UmbracoTreeSearcherFields>();
            builder.Services.AddScoped<IPublishedContentQuery>(factory =>
            {
                var umbCtx = factory.GetRequiredService<IUmbracoContextAccessor>();
                return new PublishedContentQuery(umbCtx.UmbracoContext.PublishedSnapshot, factory.GetRequiredService<IVariationContextAccessor>(), factory.GetRequiredService<IExamineManager>());
            });

            // register accessors for cultures
            builder.Services.AddUnique<IDefaultCultureAccessor, DefaultCultureAccessor>();

            builder.Services.AddSingleton<IFilePermissionHelper, FilePermissionHelper>();

            builder.Services.AddUnique<IUmbracoComponentRenderer, UmbracoComponentRenderer>();

            // Register noop versions for examine to be overridden by examine
            builder.Services.AddUnique<IUmbracoIndexesCreator, NoopUmbracoIndexesCreator>();
            builder.Services.AddUnique<IBackOfficeExamineSearcher, NoopBackOfficeExamineSearcher>();

            builder.Services.AddUnique<UploadAutoFillProperties>();

            builder.Services.AddUnique<ICronTabParser, NCronTabParser>();

            builder.Services.AddUnique<IImageDimensionExtractor, ImageDimensionExtractor>();

            builder.Services.AddUnique<PackageDataInstallation>();

            return builder;
        }

        private static IUmbracoBuilder AddMainDom(this IUmbracoBuilder builder)
        {
            builder.Services.AddUnique<IMainDom, MainDom>();

            builder.Services.AddUnique<IMainDomLock>(factory =>
            {
                var globalSettings = factory.GetRequiredService<IOptions<GlobalSettings>>().Value;
                var connectionStrings = factory.GetRequiredService<IOptions<ConnectionStrings>>().Value;
                var hostingEnvironment = factory.GetRequiredService<IHostingEnvironment>();

                var dbCreator = factory.GetRequiredService<IDbProviderFactoryCreator>();
                var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                var loggerFactory = factory.GetRequiredService<ILoggerFactory>();

                return globalSettings.MainDomLock.Equals("SqlMainDomLock") || isWindows == false
                    ? (IMainDomLock)new SqlMainDomLock(loggerFactory.CreateLogger<SqlMainDomLock>(), loggerFactory, globalSettings, connectionStrings, dbCreator, hostingEnvironment)
                    : new MainDomSemaphoreLock(loggerFactory.CreateLogger<MainDomSemaphoreLock>(), hostingEnvironment);
            });

            return builder;
        }
    }
}

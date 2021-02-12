using System.Runtime.InteropServices;
using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.HealthChecks.NotificationMethods;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Logging.Serilog.Enrichers;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.HealthChecks;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Infrastructure.Install;
using Umbraco.Core;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Core.Packaging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Runtime;
using Umbraco.Core.Scoping;
using Umbraco.Core.Serialization;
using Umbraco.Extensions;
using Umbraco.Infrastructure.Media;
using Umbraco.Infrastructure.Runtime;
using Umbraco.Web;
using Umbraco.Web.Media;
using Umbraco.Web.Migrations.PostMigrations;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.PropertyEditors.ValueConverters;
using Umbraco.Web.Routing;
using Umbraco.Web.Search;

namespace Umbraco.Cms.Infrastructure.DependencyInjection
{
    public static partial class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Adds all core Umbraco services required to run which may be replaced later in the pipeline
        /// </summary>
        public static IUmbracoBuilder AddCoreInitialServices(this IUmbracoBuilder builder)
        {
            builder
                .AddMainDom()
                .AddLogging();

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

            // register the manifest filter collection builder (collection is empty by default)
            builder.ManifestFilters();

            builder.MediaUrlGenerators()
                .Add<FileUploadPropertyEditor>()
                .Add<ImageCropperPropertyEditor>();

            builder.Services.AddUnique<IPublishedContentTypeFactory, PublishedContentTypeFactory>();

            builder.Services.AddUnique<IShortStringHelper>(factory
                => new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(factory.GetRequiredService<IOptions<RequestHandlerSettings>>().Value)));

            builder.Services.AddUnique<IMigrationBuilder>(factory => new MigrationBuilder(factory));

            builder.Services.AddUnique<IPublishedSnapshotRebuilder, PublishedSnapshotRebuilder>();

            // register the published snapshot accessor - the "current" published snapshot is in the umbraco context
            builder.Services.AddUnique<IPublishedSnapshotAccessor, UmbracoContextPublishedSnapshotAccessor>();

            builder.Services.AddUnique<IVariationContextAccessor, HybridVariationContextAccessor>();

            // Config manipulator
            builder.Services.AddUnique<IConfigManipulator, JsonConfigManipulator>();

            builder.Services.AddUnique<RichTextEditorPastedImages>();
            builder.Services.AddUnique<BlockEditorConverter>();

            // both TinyMceValueConverter (in Core) and RteMacroRenderingValueConverter (in Web) will be
            // discovered when CoreBootManager configures the converters. We will remove the basic one defined
            // in core so that the more enhanced version is active.
            builder.PropertyValueConverters()
                .Remove<SimpleTinyMceValueConverter>();

            builder.Services.AddUnique<IImageUrlGenerator, ImageSharpImageUrlGenerator>();

            builder.Services.AddUnique<IPublishedSnapshotRebuilder, PublishedSnapshotRebuilder>();

            // register *all* checks, except those marked [HideFromTypeFinder] of course
            builder.Services.AddUnique<IMarkdownToHtmlConverter, MarkdownToHtmlConverter>();

            builder.Services.AddUnique<IContentLastChanceFinder, ContentFinderByConfigured404>();

            builder.Services.AddScoped<UmbracoTreeSearcher>();

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

            builder.AddInstaller();

            // Services required to run background jobs (with out the handler)
            builder.Services.AddUnique<IBackgroundTaskQueue, BackgroundTaskQueue>();
            builder.Services.AddUnique<TaskHelper>();

            return builder;
        }

        /// <summary>
        /// Adds logging requirements for Umbraco
        /// </summary>
        private static IUmbracoBuilder AddLogging(this IUmbracoBuilder builder)
        {
            builder.Services.AddUnique<ThreadAbortExceptionEnricher>();
            builder.Services.AddUnique<HttpSessionIdEnricher>();
            builder.Services.AddUnique<HttpRequestNumberEnricher>();
            builder.Services.AddUnique<HttpRequestIdEnricher>();
            return builder;
        }

        private static IUmbracoBuilder AddMainDom(this IUmbracoBuilder builder)
        {
            builder.Services.AddUnique<IMainDomLock>(factory =>
            {
                var globalSettings = factory.GetRequiredService<IOptions<GlobalSettings>>().Value;
                var connectionStrings = factory.GetRequiredService<IOptions<ConnectionStrings>>().Value;
                var hostingEnvironment = factory.GetRequiredService<IHostingEnvironment>();

                var dbCreator = factory.GetRequiredService<IDbProviderFactoryCreator>();
                var databaseSchemaCreatorFactory = factory.GetRequiredService<DatabaseSchemaCreatorFactory>();
                var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                var loggerFactory = factory.GetRequiredService<ILoggerFactory>();

                return globalSettings.MainDomLock.Equals("SqlMainDomLock") || isWindows == false
                    ? (IMainDomLock)new SqlMainDomLock(loggerFactory.CreateLogger<SqlMainDomLock>(), loggerFactory, globalSettings, connectionStrings, dbCreator, hostingEnvironment, databaseSchemaCreatorFactory)
                    : new MainDomSemaphoreLock(loggerFactory.CreateLogger<MainDomSemaphoreLock>(), hostingEnvironment);
            });

            return builder;
        }
    }
}

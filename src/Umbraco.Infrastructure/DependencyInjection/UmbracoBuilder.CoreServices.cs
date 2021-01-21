using System.Runtime.InteropServices;
using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Hosting;
using Umbraco.Core.Install;
using Umbraco.Core.Logging.Serilog.Enrichers;
using Umbraco.Core.Mail;
using Umbraco.Core.Manifest;
using Umbraco.Core.Media;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Migrations.PostMigrations;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Packaging;
using Umbraco.Core.Persistence;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Runtime;
using Umbraco.Core.Scoping;
using Umbraco.Core.Serialization;
using Umbraco.Core.Strings;
using Umbraco.Core.Templates;
using Umbraco.Examine;
using Umbraco.Infrastructure.Examine;
using Umbraco.Infrastructure.HostedServices;
using Umbraco.Infrastructure.Logging.Serilog.Enrichers;
using Umbraco.Infrastructure.Media;
using Umbraco.Infrastructure.Runtime;
using Umbraco.Web;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.HealthCheck.NotificationMethods;
using Umbraco.Web.Install;
using Umbraco.Web.Media;
using Umbraco.Web.Migrations.PostMigrations;
using Umbraco.Web.Models.PublishedContent;
using Umbraco.Web.PropertyEditors;
using Umbraco.Web.PropertyEditors.ValueConverters;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Search;
using Umbraco.Web.Trees;

namespace Umbraco.Infrastructure.DependencyInjection
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

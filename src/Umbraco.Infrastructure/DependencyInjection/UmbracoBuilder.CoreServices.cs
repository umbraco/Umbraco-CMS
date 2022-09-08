using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using SixLabors.ImageSharp;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Handlers;
using Umbraco.Cms.Core.HealthChecks.NotificationMethods;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Logging.Serilog.Enrichers;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.DistributedLocking;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.HealthChecks;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Infrastructure.Install;
using Umbraco.Cms.Infrastructure.Mail;
using Umbraco.Cms.Infrastructure.Media;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Migrations.PostMigrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0.DataTypes;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Infrastructure.Runtime;
using Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Infrastructure.Services.Implement;
using Umbraco.Extensions;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace Umbraco.Cms.Infrastructure.DependencyInjection;

public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    /// Adds all core Umbraco services required to run which may be replaced later in the pipeline.
    /// </summary>
    public static IUmbracoBuilder AddCoreInitialServices(this IUmbracoBuilder builder)
    {
        builder
            .AddMainDom()
            .AddLogging();

        builder.Services.AddSingleton<IDistributedLockingMechanismFactory, DefaultDistributedLockingMechanismFactory>();
        builder.Services.AddSingleton<IUmbracoDatabaseFactory, UmbracoDatabaseFactory>();
        builder.Services.AddSingleton(factory => factory.GetRequiredService<IUmbracoDatabaseFactory>().SqlContext);
        builder.NPocoMappers()?.Add<NullableDateMapper>();
        builder.PackageMigrationPlans()?.Add(() => builder.TypeLoader.GetPackageMigrationPlans());

        builder.Services.AddSingleton<IRuntimeState, RuntimeState>();
        builder.Services.AddSingleton<IRuntime, CoreRuntime>();
        builder.Services.AddSingleton<PendingPackageMigrations>();
        builder.AddNotificationAsyncHandler<RuntimeUnattendedInstallNotification, UnattendedInstaller>();
        builder.AddNotificationAsyncHandler<RuntimeUnattendedUpgradeNotification, UnattendedUpgrader>();

        // Add runtime mode validation
        builder.Services.AddSingleton<IRuntimeModeValidationService, RuntimeModeValidationService>();
        builder.RuntimeModeValidators()
            .Add<JITOptimizerValidator>()
            .Add<UmbracoApplicationUrlValidator>()
            .Add<UseHttpsValidator>()
            .Add<RuntimeMinificationValidator>()
            .Add<ModelsBuilderModeValidator>();

        // composers
        builder
            .AddRepositories()
            .AddServices()
            .AddCoreMappingProfiles()
            .AddFileSystems()
            .AddWebAssets();

        // register persistence mappers - required by database factory so needs to be done here
        // means the only place the collection can be modified is in a runtime - afterwards it
        // has been frozen and it is too late
        builder.Mappers()?.AddCoreMappers();

        // register the scope provider
        builder.Services.AddSingleton<ScopeProvider>(sp => ActivatorUtilities.CreateInstance<ScopeProvider>(sp, sp.GetRequiredService<IAmbientScopeStack>())); // implements IScopeProvider, IScopeAccessor
        builder.Services.AddSingleton<ICoreScopeProvider>(f => f.GetRequiredService<ScopeProvider>());
        builder.Services.AddSingleton<IScopeProvider>(f => f.GetRequiredService<ScopeProvider>());
        builder.Services.AddSingleton<Core.Scoping.IScopeProvider>(f => f.GetRequiredService<ScopeProvider>());

        builder.Services.AddSingleton<IAmbientScopeStack, AmbientScopeStack>();
        builder.Services.AddSingleton<IScopeAccessor>(f => f.GetRequiredService<IAmbientScopeStack>());
        builder.Services.AddSingleton<IAmbientScopeContextStack, AmbientScopeContextStack>();

        builder.Services.AddScoped<IHttpScopeReference, HttpScopeReference>();

        builder.Services.AddSingleton<IJsonSerializer, JsonNetSerializer>();
        builder.Services.AddSingleton<IConfigurationEditorJsonSerializer, ConfigurationEditorJsonSerializer>();
        builder.Services.AddSingleton<IMenuItemCollectionFactory, MenuItemCollectionFactory>();

        // register database builder
        // *not* a singleton, don't want to keep it around
        builder.Services.AddTransient<DatabaseBuilder>();

        // register manifest parser, will be injected in collection builders where needed
        builder.Services.AddSingleton<IManifestParser, ManifestParser>();

        // register the manifest filter collection builder (collection is empty by default)
        builder.ManifestFilters();

        builder.MediaUrlGenerators()
            .Add<FileUploadPropertyEditor>()
            .Add<ImageCropperPropertyEditor>();

        builder.Services.AddSingleton<IPublishedContentTypeFactory, PublishedContentTypeFactory>();

        builder.Services.AddSingleton<IShortStringHelper>(factory
            => new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(
                factory.GetRequiredService<IOptionsMonitor<RequestHandlerSettings>>().CurrentValue)));

        builder.Services.AddSingleton<IMigrationPlanExecutor, MigrationPlanExecutor>();
        builder.Services.AddSingleton<IMigrationBuilder>(factory => new MigrationBuilder(factory));

        builder.AddPreValueMigrators();

        builder.Services.AddSingleton<IPublishedSnapshotRebuilder, PublishedSnapshotRebuilder>();

        // register the published snapshot accessor - the "current" published snapshot is in the umbraco context
        builder.Services.AddSingleton<IPublishedSnapshotAccessor, UmbracoContextPublishedSnapshotAccessor>();

        builder.Services.AddSingleton<IVariationContextAccessor, HybridVariationContextAccessor>();

        // Config manipulator
        builder.Services.AddSingleton<IConfigManipulator, JsonConfigManipulator>();

        builder.Services.AddSingleton<RichTextEditorPastedImages>();
        builder.Services.AddSingleton<BlockEditorConverter>();

        // both TinyMceValueConverter (in Core) and RteMacroRenderingValueConverter (in Web) will be
        // discovered when CoreBootManager configures the converters. We will remove the basic one defined
        // in core so that the more enhanced version is active.
        builder.PropertyValueConverters()
            .Remove<SimpleTinyMceValueConverter>();

        // register *all* checks, except those marked [HideFromTypeFinder] of course
        builder.Services.AddSingleton<IMarkdownToHtmlConverter, MarkdownToHtmlConverter>();

        builder.Services.AddSingleton<IContentLastChanceFinder, ContentFinderByConfigured404>();

        builder.Services.AddScoped<UmbracoTreeSearcher>();

        // replace
        builder.Services.AddSingleton<IEmailSender, EmailSender>(
            services => new EmailSender(
                services.GetRequiredService<ILogger<EmailSender>>(),
                services.GetRequiredService<IOptionsMonitor<GlobalSettings>>(),
                services.GetRequiredService<IEventAggregator>(),
                services.GetService<INotificationHandler<SendEmailNotification>>(),
                services.GetService<INotificationAsyncHandler<SendEmailNotification>>()));

        builder.Services.AddSingleton<IExamineManager, ExamineManager>();

        builder.Services.AddScoped<ITagQuery, TagQuery>();

        builder.Services.AddSingleton<IUmbracoTreeSearcherFields, UmbracoTreeSearcherFields>();
        builder.Services.AddSingleton<IPublishedContentQueryAccessor, PublishedContentQueryAccessor>(sp =>
            new PublishedContentQueryAccessor(sp.GetRequiredService<IScopedServiceProvider>()));
        builder.Services.AddScoped<IPublishedContentQuery>(factory =>
        {
            IUmbracoContextAccessor umbCtx = factory.GetRequiredService<IUmbracoContextAccessor>();
            IUmbracoContext umbracoContext = umbCtx.GetRequiredUmbracoContext();
            return new PublishedContentQuery(
                umbracoContext.PublishedSnapshot,
                factory.GetRequiredService<IVariationContextAccessor>(), factory.GetRequiredService<IExamineManager>());
        });

        // register accessors for cultures
        builder.Services.AddSingleton<IDefaultCultureAccessor, DefaultCultureAccessor>();

        builder.Services.AddSingleton<IFilePermissionHelper, FilePermissionHelper>();

        builder.Services.AddSingleton<IUmbracoComponentRenderer, UmbracoComponentRenderer>();

        builder.Services.AddSingleton<IBackOfficeExamineSearcher, NoopBackOfficeExamineSearcher>();

        builder.Services.AddSingleton<UploadAutoFillProperties>();

        builder.Services.AddSingleton<ICronTabParser, NCronTabParser>();

        // Add default ImageSharp configuration and service implementations
        builder.Services.AddSingleton(Configuration.Default);
        builder.Services.AddSingleton<IImageDimensionExtractor, ImageSharpDimensionExtractor>();

        builder.Services.AddTransient<INodeCountService, NodeCountService>();
        builder.AddInstaller();

        // Services required to run background jobs (with out the handler)
        builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

        builder.Services.AddTransient<IFireAndForgetRunner, FireAndForgetRunner>();
        return builder;
    }

    public static IUmbracoBuilder AddLogViewer(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<ILogViewerConfig, LogViewerConfig>();
        builder.Services.AddSingleton<ILogLevelLoader, LogLevelLoader>();
        builder.SetLogViewer<SerilogJsonLogViewer>();
        builder.Services.AddSingleton<ILogViewer>(factory => new SerilogJsonLogViewer(
            factory.GetRequiredService<ILogger<SerilogJsonLogViewer>>(),
            factory.GetRequiredService<ILogViewerConfig>(),
            factory.GetRequiredService<ILoggingConfiguration>(),
            factory.GetRequiredService<ILogLevelLoader>(),
            Log.Logger));

        return builder;
    }

    /// <summary>
    ///     Adds logging requirements for Umbraco
    /// </summary>
    private static IUmbracoBuilder AddLogging(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<ThreadAbortExceptionEnricher>();
        builder.Services.AddSingleton<HttpSessionIdEnricher>();
        builder.Services.AddSingleton<HttpRequestNumberEnricher>();
        builder.Services.AddSingleton<HttpRequestIdEnricher>();
        return builder;
    }

    private static IUmbracoBuilder AddMainDom(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IMainDomKeyGenerator, DefaultMainDomKeyGenerator>();
        builder.Services.AddSingleton<IMainDomLock>(factory =>
        {
            IOptions<GlobalSettings> globalSettings = factory.GetRequiredService<IOptions<GlobalSettings>>();
            IOptionsMonitor<ConnectionStrings> connectionStrings =
                factory.GetRequiredService<IOptionsMonitor<ConnectionStrings>>();
            IHostingEnvironment hostingEnvironment = factory.GetRequiredService<IHostingEnvironment>();

            IDbProviderFactoryCreator dbCreator = factory.GetRequiredService<IDbProviderFactoryCreator>();
            DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory =
                factory.GetRequiredService<DatabaseSchemaCreatorFactory>();
            ILoggerFactory loggerFactory = factory.GetRequiredService<ILoggerFactory>();
            NPocoMapperCollection npocoMappers = factory.GetRequiredService<NPocoMapperCollection>();
            IMainDomKeyGenerator mainDomKeyGenerator = factory.GetRequiredService<IMainDomKeyGenerator>();

            switch (globalSettings.Value.MainDomLock)
            {
                case "SqlMainDomLock":
                    return new SqlMainDomLock(
                        loggerFactory,
                        globalSettings,
                        connectionStrings,
                        dbCreator,
                        mainDomKeyGenerator,
                        databaseSchemaCreatorFactory,
                        npocoMappers);

                case "MainDomSemaphoreLock":
                    return new MainDomSemaphoreLock(
                        loggerFactory.CreateLogger<MainDomSemaphoreLock>(),
                        hostingEnvironment);

                case "FileSystemMainDomLock":
                default:
                    return new FileSystemMainDomLock(
                        loggerFactory.CreateLogger<FileSystemMainDomLock>(),
                        mainDomKeyGenerator, hostingEnvironment,
                        factory.GetRequiredService<IOptionsMonitor<GlobalSettings>>());
            }
        });

        return builder;
    }

    private static IUmbracoBuilder AddPreValueMigrators(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<PreValueMigratorCollectionBuilder>()
            .Append<RenamingPreValueMigrator>()
            .Append<RichTextPreValueMigrator>()
            .Append<UmbracoSliderPreValueMigrator>()
            .Append<MediaPickerPreValueMigrator>()
            .Append<ContentPickerPreValueMigrator>()
            .Append<NestedContentPreValueMigrator>()
            .Append<DecimalPreValueMigrator>()
            .Append<ListViewPreValueMigrator>()
            .Append<DropDownFlexiblePreValueMigrator>()
            .Append<ValueListPreValueMigrator>()
            .Append<MarkdownEditorPreValueMigrator>();

        return builder;
    }

    public static IUmbracoBuilder AddCoreNotifications(this IUmbracoBuilder builder)
    {
        // add handlers for sending user notifications (i.e. emails)
        builder.Services.AddSingleton<UserNotificationsHandler.Notifier>();
        builder
            .AddNotificationHandler<ContentSavedNotification, UserNotificationsHandler>()
            .AddNotificationHandler<ContentSortedNotification, UserNotificationsHandler>()
            .AddNotificationHandler<ContentPublishedNotification, UserNotificationsHandler>()
            .AddNotificationHandler<ContentMovedNotification, UserNotificationsHandler>()
            .AddNotificationHandler<ContentMovedToRecycleBinNotification, UserNotificationsHandler>()
            .AddNotificationHandler<ContentCopiedNotification, UserNotificationsHandler>()
            .AddNotificationHandler<ContentRolledBackNotification, UserNotificationsHandler>()
            .AddNotificationHandler<ContentSentToPublishNotification, UserNotificationsHandler>()
            .AddNotificationHandler<ContentUnpublishedNotification, UserNotificationsHandler>()
            .AddNotificationHandler<AssignedUserGroupPermissionsNotification, UserNotificationsHandler>()
            .AddNotificationHandler<PublicAccessEntrySavedNotification, UserNotificationsHandler>();

        // add handlers for building content relations
        builder
            .AddNotificationHandler<ContentCopiedNotification, RelateOnCopyNotificationHandler>()
            .AddNotificationHandler<ContentMovedNotification, RelateOnTrashNotificationHandler>()
            .AddNotificationHandler<ContentMovedToRecycleBinNotification, RelateOnTrashNotificationHandler>()
            .AddNotificationHandler<MediaMovedNotification, RelateOnTrashNotificationHandler>()
            .AddNotificationHandler<MediaMovedToRecycleBinNotification, RelateOnTrashNotificationHandler>();

        // add notification handlers for property editors
        builder
            .AddNotificationHandler<ContentSavingNotification, BlockEditorPropertyHandler>()
            .AddNotificationHandler<ContentCopyingNotification, BlockEditorPropertyHandler>()
            .AddNotificationHandler<ContentSavingNotification, NestedContentPropertyHandler>()
            .AddNotificationHandler<ContentCopyingNotification, NestedContentPropertyHandler>()
            .AddNotificationHandler<ContentCopiedNotification, FileUploadPropertyEditor>()
            .AddNotificationHandler<ContentDeletedNotification, FileUploadPropertyEditor>()
            .AddNotificationHandler<MediaDeletedNotification, FileUploadPropertyEditor>()
            .AddNotificationHandler<MediaSavingNotification, FileUploadPropertyEditor>()
            .AddNotificationHandler<MemberDeletedNotification, FileUploadPropertyEditor>()
            .AddNotificationHandler<ContentCopiedNotification, ImageCropperPropertyEditor>()
            .AddNotificationHandler<ContentDeletedNotification, ImageCropperPropertyEditor>()
            .AddNotificationHandler<MediaDeletedNotification, ImageCropperPropertyEditor>()
            .AddNotificationHandler<MediaSavingNotification, ImageCropperPropertyEditor>()
            .AddNotificationHandler<MemberDeletedNotification, ImageCropperPropertyEditor>();

        // add notification handlers for redirect tracking
        builder
            .AddNotificationHandler<ContentPublishingNotification, RedirectTrackingHandler>()
            .AddNotificationHandler<ContentPublishedNotification, RedirectTrackingHandler>()
            .AddNotificationHandler<ContentMovingNotification, RedirectTrackingHandler>()
            .AddNotificationHandler<ContentMovedNotification, RedirectTrackingHandler>();

        // Add notification handlers for DistributedCache
        builder
            .AddNotificationHandler<DictionaryItemDeletedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<DictionaryItemSavedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<LanguageSavedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<LanguageDeletedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<MemberSavedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<MemberDeletedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<PublicAccessEntrySavedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<PublicAccessEntryDeletedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<UserSavedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<UserDeletedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<UserGroupWithUsersSavedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<UserGroupDeletedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<MemberGroupDeletedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<MemberGroupSavedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<DataTypeDeletedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<DataTypeSavedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<TemplateDeletedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<TemplateSavedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<RelationTypeDeletedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<RelationTypeSavedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<DomainDeletedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<DomainSavedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<MacroSavedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<MacroDeletedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<MediaTreeChangeNotification, DistributedCacheBinder>()
            .AddNotificationHandler<ContentTypeChangedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<MediaTypeChangedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<MemberTypeChangedNotification, DistributedCacheBinder>()
            .AddNotificationHandler<ContentTreeChangeNotification, DistributedCacheBinder>()
            ;

        // add notification handlers for auditing
        builder
            .AddNotificationHandler<MemberSavedNotification, AuditNotificationsHandler>()
            .AddNotificationHandler<MemberDeletedNotification, AuditNotificationsHandler>()
            .AddNotificationHandler<AssignedMemberRolesNotification, AuditNotificationsHandler>()
            .AddNotificationHandler<RemovedMemberRolesNotification, AuditNotificationsHandler>()
            .AddNotificationHandler<ExportedMemberNotification, AuditNotificationsHandler>()
            .AddNotificationHandler<UserSavedNotification, AuditNotificationsHandler>()
            .AddNotificationHandler<UserDeletedNotification, AuditNotificationsHandler>()
            .AddNotificationHandler<UserGroupWithUsersSavedNotification, AuditNotificationsHandler>()
            .AddNotificationHandler<AssignedUserGroupPermissionsNotification, AuditNotificationsHandler>();

        return builder;
    }
}

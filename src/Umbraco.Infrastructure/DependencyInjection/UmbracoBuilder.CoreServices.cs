using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DeliveryApi.Accessors;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Handlers;
using Umbraco.Cms.Core.HealthChecks.NotificationMethods;
using Umbraco.Cms.Core.HostedServices;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Logging.Serilog.Enrichers;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Models.Context;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Configuration;
using Umbraco.Cms.Infrastructure.DeliveryApi;
using Umbraco.Cms.Infrastructure.DistributedLocking;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.HealthChecks;
using Umbraco.Cms.Infrastructure.Install;
using Umbraco.Cms.Infrastructure.Mail;
using Umbraco.Cms.Infrastructure.Mail.Interfaces;
using Umbraco.Cms.Infrastructure.Manifest;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Infrastructure.Persistence.Relations;
using Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;
using Umbraco.Cms.Infrastructure.Routing;
using Umbraco.Cms.Infrastructure.Runtime;
using Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Cms.Infrastructure.Security;
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
        builder.NPocoMappers().Add<NullableDateMapper>();
        builder.PackageMigrationPlans().Add(builder.TypeLoader.GetPackageMigrationPlans());

        builder.Services.AddSingleton<IRuntimeState, RuntimeState>();
        builder.Services.AddSingleton<IRuntime, CoreRuntime>();
        builder.Services.AddSingleton<PendingPackageMigrations>();
        builder.AddNotificationAsyncHandler<RuntimeUnattendedInstallNotification, UnattendedInstaller>();
        builder.AddNotificationAsyncHandler<RuntimeUnattendedUpgradeNotification, UnattendedUpgrader>();
        builder.AddNotificationAsyncHandler<RuntimePremigrationsUpgradeNotification, PremigrationUpgrader>();

        // Database availability check.
        builder.Services.AddUnique<IDatabaseAvailabilityCheck, DefaultDatabaseAvailabilityCheck>();

        // Add runtime mode validation
        builder.Services.AddSingleton<IRuntimeModeValidationService, RuntimeModeValidationService>();
        builder.RuntimeModeValidators()
            .Add<JITOptimizerValidator>()
            .Add<UmbracoApplicationUrlValidator>()
            .Add<UseHttpsValidator>()
            .Add<ModelsBuilderModeValidator>();

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
        builder.Services.AddSingleton<ScopeProvider>(sp => ActivatorUtilities.CreateInstance<ScopeProvider>(sp, sp.GetRequiredService<IAmbientScopeStack>())); // implements IScopeProvider, IScopeAccessor
        builder.Services.AddSingleton<ICoreScopeProvider>(f => f.GetRequiredService<ScopeProvider>());
        builder.Services.AddSingleton<IScopeProvider>(f => f.GetRequiredService<ScopeProvider>());
        builder.Services.AddSingleton<Core.Scoping.IScopeProvider>(f => f.GetRequiredService<ScopeProvider>());

        builder.Services.AddSingleton<IAmbientScopeStack, AmbientScopeStack>();
        builder.Services.AddSingleton<IScopeAccessor>(f => f.GetRequiredService<IAmbientScopeStack>());
        builder.Services.AddSingleton<IAmbientScopeContextStack, AmbientScopeContextStack>();

        builder.Services.AddScoped<IHttpScopeReference, HttpScopeReference>();

        builder.Services.AddSingleton<IJsonSerializer, SystemTextJsonSerializer>();
        builder.Services.AddSingleton<IConfigurationEditorJsonSerializer, SystemTextConfigurationEditorJsonSerializer>();
        builder.Services.AddUnique<IJsonSerializerEncoderFactory, DefaultJsonSerializerEncoderFactory>();
        builder.Services.AddUnique<IWebhookJsonSerializer, SystemTextWebhookJsonSerializer>();

        // register database builder
        // *not* a singleton, don't want to keep it around
        builder.Services.AddTransient<DatabaseBuilder>();

        // register manifest parser, will be injected in collection builders where needed
        builder.Services.AddSingleton<IPackageManifestReader, BackOfficePackageManifestReader>();
        builder.Services.AddSingleton<IPackageManifestReader, AppPluginsPackageManifestReader>();
        builder.Services.AddSingleton<IPackageManifestService, PackageManifestService>();

        builder.MediaUrlGenerators()
            .Add<FileUploadPropertyEditor>()
            .Add<ImageCropperPropertyEditor>();

        builder.Services.AddSingleton<IPublishedContentTypeFactory, PublishedContentTypeFactory>();

        builder.Services.AddSingleton<IShortStringHelper>(factory
            => new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(
                factory.GetRequiredService<IOptionsMonitor<RequestHandlerSettings>>().CurrentValue)));

        builder.Services.AddSingleton<IMigrationPlanExecutor, MigrationPlanExecutor>();
        builder.Services.AddSingleton<IMigrationBuilder>(factory => new MigrationBuilder(factory));

        builder.Services.AddSingleton<IVariationContextAccessor, HybridVariationContextAccessor>();
        builder.Services.AddSingleton<IBackOfficeVariationContextAccessor, HttpContextBackOfficeVariationContextAccessor>();

        // Config manipulator
        builder.Services.AddSingleton<IConfigManipulator, JsonConfigManipulator>();

        builder.Services.AddSingleton<RichTextEditorPastedImages>();
        builder.Services.AddSingleton<BlockEditorConverter>();
        builder.Services.AddSingleton<BlockListPropertyValueConstructorCache>();
        builder.Services.AddSingleton<BlockGridPropertyValueConstructorCache>();
        builder.Services.AddSingleton<RichTextBlockPropertyValueConstructorCache>();
        builder.Services.AddSingleton<BlockEditorVarianceHandler>();

        // both SimpleTinyMceValueConverter (in Core) and RteBlockRenderingValueConverter (in Infrastructure) will be
        // discovered when CoreBootManager configures the converters. We will remove the basic one defined
        // in core so that the more enhanced version is active.
        builder.PropertyValueConverters()
            .Remove<SimpleRichTextValueConverter>();

        // register *all* checks, except those marked [HideFromTypeFinder] of course
        builder.Services.AddSingleton<IMarkdownToHtmlConverter, MarkdownToHtmlConverter>();

        builder.Services.AddSingleton<IContentLastChanceFinder, ContentFinderByConfigured404>();

        builder.Services.AddTransient<IEmailSenderClient, BasicSmtpEmailSenderClient>();

        // replace
        builder.Services.AddSingleton<IEmailSender, EmailSender>(
            services => new EmailSender(
                services.GetRequiredService<ILogger<EmailSender>>(),
                services.GetRequiredService<IOptionsMonitor<GlobalSettings>>(),
                services.GetRequiredService<IEventAggregator>(),
                services.GetRequiredService<IEmailSenderClient>(),
                services.GetService<INotificationHandler<SendEmailNotification>>(),
                services.GetService<INotificationAsyncHandler<SendEmailNotification>>()));

        builder.Services.AddTransient<IUserInviteSender, EmailUserInviteSender>();
        builder.Services.AddTransient<IUserForgotPasswordSender, EmailUserForgotPasswordSender>();

        builder.Services.AddSingleton<IExamineManager, NoopExamineManager>();
        builder.Services.AddSingleton<IIndexRebuilder, NoopIndexRebuilder>();

        builder.Services.AddScoped<ITagQuery, TagQuery>();

        builder.Services.AddSingleton<IUmbracoTreeSearcherFields, UmbracoTreeSearcherFields>();
        builder.Services.AddSingleton<IPublishedContentQueryAccessor, PublishedContentQueryAccessor>(sp =>
            new PublishedContentQueryAccessor(sp.GetRequiredService<IScopedServiceProvider>()));
        builder.Services.AddScoped<IPublishedContentQuery>(factory =>
        {
            IUmbracoContextAccessor umbCtx = factory.GetRequiredService<IUmbracoContextAccessor>();
            return new PublishedContentQuery(
                factory.GetRequiredService<IVariationContextAccessor>(),
                factory.GetRequiredService<IExamineManager>(),
                factory.GetRequiredService<IPublishedContentCache>(),
                factory.GetRequiredService<IPublishedMediaCache>());
        });

        // register accessors for cultures
        builder.Services.AddSingleton<IDefaultCultureAccessor, DefaultCultureAccessor>();

        builder.Services.AddSingleton<IFilePermissionHelper, FilePermissionHelper>();

        builder.Services.AddSingleton<IUmbracoComponentRenderer, UmbracoComponentRenderer>();

        builder.Services.AddSingleton<IBackOfficeExamineSearcher, NoopBackOfficeExamineSearcher>();

        builder.Services.AddSingleton<UploadAutoFillProperties>();
        builder.Services.AddSingleton<IImageDimensionExtractor, NoopImageDimensionExtractor>();
        builder.Services.AddSingleton<IImageUrlGenerator, NoopImageUrlGenerator>();

        builder.Services.AddSingleton<ICronTabParser, NCronTabParser>();

        builder.Services.AddTransient<INodeCountService, NodeCountService>();

        builder.Services.AddSingleton<IRedirectTracker, RedirectTracker>();

        builder.AddInstaller();

        // Services required to run background jobs
        // We can simplify this registration once the obsolete IBackgroundTaskQueue is removed.
        builder.Services.AddSingleton<HostedServices.BackgroundTaskQueue>();
        builder.Services.AddSingleton<IBackgroundTaskQueue>(s => s.GetRequiredService<HostedServices.BackgroundTaskQueue>());
#pragma warning disable CS0618 // Type or member is obsolete
        builder.Services.AddSingleton<HostedServices.IBackgroundTaskQueue>(s => s.GetRequiredService<HostedServices.BackgroundTaskQueue>());
#pragma warning restore CS0618 // Type or member is obsolete

        builder.Services.AddTransient<IFireAndForgetRunner, FireAndForgetRunner>();

        builder.AddPropertyIndexValueFactories();

        builder.AddDeliveryApiCoreServices();
        builder.Services.AddTransient<IWebhookFiringService, WebhookFiringService>();

        builder.Services.AddUnique<IPasswordChanger<BackOfficeIdentityUser>, PasswordChanger<BackOfficeIdentityUser>>();
        builder.Services.AddUnique<IPasswordChanger<MemberIdentityUser>, PasswordChanger<MemberIdentityUser>>();
        builder.Services.AddTransient<IMemberEditingService, MemberEditingService>();

        builder.Services.AddSingleton<IBlockEditorElementTypeCache, BlockEditorElementTypeCache>();

        builder.Services.AddSingleton<IRichTextRequiredValidator, RichTextRequiredValidator>();

        builder.Services.AddSingleton<IRichTextRegexValidator, RichTextRegexValidator>();

        return builder;
    }

    public static IUmbracoBuilder AddPropertyIndexValueFactories(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IBlockValuePropertyIndexValueFactory, BlockValuePropertyIndexValueFactory>();
        builder.Services.AddSingleton<ITagPropertyIndexValueFactory, TagPropertyIndexValueFactory>();
        builder.Services.AddSingleton<IRichTextPropertyIndexValueFactory, RichTextPropertyIndexValueFactory>();

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
            .AddNotificationHandler<ContentSavingNotification, BlockListPropertyNotificationHandler>()
            .AddNotificationHandler<ContentCopyingNotification, BlockListPropertyNotificationHandler>()
            .AddNotificationHandler<ContentScaffoldedNotification, BlockListPropertyNotificationHandler>()
            .AddNotificationHandler<ContentSavingNotification, BlockGridPropertyNotificationHandler>()
            .AddNotificationHandler<ContentCopyingNotification, BlockGridPropertyNotificationHandler>()
            .AddNotificationHandler<ContentScaffoldedNotification, BlockGridPropertyNotificationHandler>()
            .AddNotificationHandler<ContentSavingNotification, RichTextPropertyNotificationHandler>()
            .AddNotificationHandler<ContentCopyingNotification, RichTextPropertyNotificationHandler>()
            .AddNotificationHandler<ContentScaffoldedNotification, RichTextPropertyNotificationHandler>()
            .AddNotificationHandler<ContentCopiedNotification, FileUploadContentCopiedOrScaffoldedNotificationHandler>()
            .AddNotificationHandler<ContentScaffoldedNotification, FileUploadContentCopiedOrScaffoldedNotificationHandler>()
            .AddNotificationHandler<ContentSavedBlueprintNotification, FileUploadContentCopiedOrScaffoldedNotificationHandler>()
            .AddNotificationHandler<ContentDeletedNotification, FileUploadContentDeletedNotificationHandler>()
            .AddNotificationHandler<ContentDeletedBlueprintNotification, FileUploadContentDeletedNotificationHandler>()
            .AddNotificationHandler<MediaDeletedNotification, FileUploadContentDeletedNotificationHandler>()
            .AddNotificationHandler<MemberDeletedNotification, FileUploadContentDeletedNotificationHandler>()
            .AddNotificationHandler<MediaSavingNotification, FileUploadMediaSavingNotificationHandler>()
            .AddNotificationHandler<ContentCopiedNotification, ImageCropperPropertyEditor>()
            .AddNotificationHandler<ContentDeletedNotification, ImageCropperPropertyEditor>()
            .AddNotificationHandler<MediaDeletedNotification, ImageCropperPropertyEditor>()
            .AddNotificationHandler<MediaSavingNotification, ImageCropperPropertyEditor>()
            .AddNotificationHandler<MemberDeletedNotification, ImageCropperPropertyEditor>()
            .AddNotificationHandler<ContentTypeCacheRefresherNotification, ConstructorCacheClearNotificationHandler>()
            .AddNotificationHandler<DataTypeCacheRefresherNotification, ConstructorCacheClearNotificationHandler>();

        // add notification handlers for redirect tracking
        builder
            .AddNotificationHandler<ContentPublishingNotification, RedirectTrackingHandler>()
            .AddNotificationHandler<ContentPublishedNotification, RedirectTrackingHandler>()
            .AddNotificationHandler<ContentMovingNotification, RedirectTrackingHandler>()
            .AddNotificationHandler<ContentMovedNotification, RedirectTrackingHandler>();

        // Add notification handlers for DistributedCache
        builder
            .AddNotificationHandler<DictionaryItemDeletedNotification, DictionaryItemDeletedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<DictionaryItemSavedNotification, DictionaryItemSavedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<LanguageSavedNotification, LanguageSavedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<LanguageDeletedNotification, LanguageDeletedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<MemberSavedNotification, MemberSavedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<MemberDeletedNotification, MemberDeletedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<PublicAccessEntrySavedNotification, PublicAccessEntrySavedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<PublicAccessEntryDeletedNotification, PublicAccessEntryDeletedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<UserSavedNotification, UserSavedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<UserDeletedNotification, UserDeletedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<UserGroupWithUsersSavedNotification, UserGroupWithUsersSavedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<UserGroupDeletedNotification, UserGroupDeletedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<MemberGroupDeletedNotification, MemberGroupDeletedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<MemberGroupSavedNotification, MemberGroupSavedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<DataTypeDeletedNotification, DataTypeDeletedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<DataTypeSavedNotification, DataTypeSavedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<TemplateDeletedNotification, TemplateDeletedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<TemplateSavedNotification, TemplateSavedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<RelationTypeDeletedNotification, RelationTypeDeletedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<RelationTypeSavedNotification, RelationTypeSavedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<DomainDeletedNotification, DomainDeletedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<DomainSavedNotification, DomainSavedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<MediaTreeChangeNotification, MediaTreeChangeDistributedCacheNotificationHandler>()
            .AddNotificationHandler<ContentTypeChangedNotification, ContentTypeChangedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<MediaTypeChangedNotification, MediaTypeChangedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<MemberTypeChangedNotification, MemberTypeChangedDistributedCacheNotificationHandler>()
            .AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>()
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

        // Handlers for publish warnings
        builder.AddNotificationHandler<ContentPublishedNotification, AddDomainWarningsWhenPublishingNotificationHandler>();
        builder.AddNotificationAsyncHandler<ContentPublishedNotification, AddUnroutableContentWarningsWhenPublishingNotificationHandler>();

        // Handlers for save warnings
        builder
            .AddNotificationAsyncHandler<ContentTypeSavingNotification, WarnDocumentTypeElementSwitchNotificationHandler>()
            .AddNotificationAsyncHandler<ContentTypeSavedNotification, WarnDocumentTypeElementSwitchNotificationHandler>();

        // Handles for relation persistence on content save.
        builder
            .AddNotificationHandler<ContentSavedNotification, ContentRelationsUpdate>()
            .AddNotificationHandler<ContentPublishedNotification, ContentRelationsUpdate>()
            .AddNotificationHandler<MediaSavedNotification, ContentRelationsUpdate>()
            .AddNotificationHandler<MemberSavedNotification, ContentRelationsUpdate>();

        return builder;
    }

    private static IUmbracoBuilder AddDeliveryApiCoreServices(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IApiElementBuilder, ApiElementBuilder>();
        builder.Services.AddSingleton<IApiContentBuilder, ApiContentBuilder>();
        builder.Services.AddSingleton<IApiContentResponseBuilder, ApiContentResponseBuilder>();
        builder.Services.AddSingleton<IApiMediaBuilder, ApiMediaBuilder>();
        builder.Services.AddSingleton<IApiMediaWithCropsBuilder, ApiMediaWithCropsBuilder>();
        builder.Services.AddSingleton<IApiMediaWithCropsResponseBuilder, ApiMediaWithCropsResponseBuilder>();
        builder.Services.AddSingleton<IApiContentNameProvider, ApiContentNameProvider>();
        builder.Services.AddSingleton<IOutputExpansionStrategyAccessor, NoopOutputExpansionStrategyAccessor>();
        builder.Services.AddSingleton<IRequestStartItemProviderAccessor, NoopRequestStartItemProviderAccessor>();
        builder.Services.AddSingleton<IRequestCultureService, NoopRequestCultureService>();
        builder.Services.AddSingleton<IRequestRoutingService, NoopRequestRoutingService>();
        builder.Services.AddSingleton<IRequestRedirectService, NoopRequestRedirectService>();
        builder.Services.AddSingleton<IRequestPreviewService, NoopRequestPreviewService>();
        builder.Services.AddSingleton<IRequestMemberAccessService, NoopRequestMemberAccessService>();
        builder.Services.AddTransient<ICurrentMemberClaimsProvider, NoopCurrentMemberClaimsProvider>();
        builder.Services.AddSingleton<IApiAccessService, NoopApiAccessService>();
        builder.Services.AddSingleton<IApiContentQueryService, NoopApiContentQueryService>();
        builder.Services.AddSingleton<IApiMediaQueryService, NoopApiMediaQueryService>();
        builder.Services.AddSingleton<IApiMediaUrlProvider, ApiMediaUrlProvider>();
        builder.Services.AddSingleton<IApiContentRouteBuilder, ApiContentRouteBuilder>();
        builder.Services.AddSingleton<IApiContentPathProvider, ApiContentPathProvider>();
        builder.Services.AddSingleton<IApiContentPathResolver, ApiContentPathResolver>();
        builder.Services.AddSingleton<IApiPublishedContentCache, ApiPublishedContentCache>();
        builder.Services.AddSingleton<IApiRichTextElementParser, ApiRichTextElementParser>();
        builder.Services.AddSingleton<IApiRichTextMarkupParser, ApiRichTextMarkupParser>();
        builder.Services.AddSingleton<IApiPropertyRenderer, ApiPropertyRenderer>();
        builder.Services.AddSingleton<IApiDocumentUrlService, ApiDocumentUrlService>();
        builder.Services.AddScoped<IMemberClientCredentialsManager, MemberClientCredentialsManager>();

        return builder;
    }
}

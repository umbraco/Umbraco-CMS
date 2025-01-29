// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Diagnostics;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Editors;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Features;
using Umbraco.Cms.Core.Handlers;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.DynamicRoot;
using Umbraco.Cms.Core.Preview;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.PublishedCache.Internal;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.FileSystem;
using Umbraco.Cms.Core.Services.ImportExport;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Services.Querying;
using Umbraco.Cms.Core.Services.Querying.RecycleBin;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using Umbraco.Cms.Core.Services.Filters;

namespace Umbraco.Cms.Core.DependencyInjection
{
    public class UmbracoBuilder : IUmbracoBuilder
    {
        private readonly Dictionary<Type, ICollectionBuilder> _builders = new Dictionary<Type, ICollectionBuilder>();

        public IServiceCollection Services { get; }

        public IConfiguration Config { get; }

        public TypeLoader TypeLoader { get; }

        /// <inheritdoc />
        public ILoggerFactory BuilderLoggerFactory { get; }

        /// <inheritdoc />
        [Obsolete("Only here to comply with obsolete implementation. Scheduled for removal in v16")]
        public IHostingEnvironment? BuilderHostingEnvironment { get; }

        public IProfiler Profiler { get; }

        public AppCaches AppCaches { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoBuilder"/> class primarily for testing.
        /// </summary>
        public UmbracoBuilder(IServiceCollection services, IConfiguration config, TypeLoader typeLoader)
            : this(services, config, typeLoader, NullLoggerFactory.Instance, new NoopProfiler(), AppCaches.Disabled, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoBuilder"/> class.
        /// </summary>
        [Obsolete("Use a non obsolete constructor instead. Scheduled for removal in v16")]
        public UmbracoBuilder(
            IServiceCollection services,
            IConfiguration config,
            TypeLoader typeLoader,
            ILoggerFactory loggerFactory,
            IProfiler profiler,
            AppCaches appCaches,
            IHostingEnvironment? hostingEnvironment)
        {
            Services = services;
            Config = config;
            BuilderLoggerFactory = loggerFactory;
            BuilderHostingEnvironment = hostingEnvironment;
            Profiler = profiler;
            AppCaches = appCaches;
            TypeLoader = typeLoader;

            AddCoreServices();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoBuilder"/> class.
        /// </summary>
        public UmbracoBuilder(
            IServiceCollection services,
            IConfiguration config,
            TypeLoader typeLoader,
            ILoggerFactory loggerFactory,
            IProfiler profiler,
            AppCaches appCaches)
        {
            Services = services;
            Config = config;
            BuilderLoggerFactory = loggerFactory;
            Profiler = profiler;
            AppCaches = appCaches;
            TypeLoader = typeLoader;

            AddCoreServices();
        }

        /// <summary>
        /// Gets a collection builder (and registers the collection).
        /// </summary>
        /// <typeparam name="TBuilder">The type of the collection builder.</typeparam>
        /// <returns>The collection builder.</returns>
        public TBuilder WithCollectionBuilder<TBuilder>()
            where TBuilder : ICollectionBuilder
        {
            Type typeOfBuilder = typeof(TBuilder);

            if (_builders.TryGetValue(typeOfBuilder, out ICollectionBuilder? o))
            {
                return (TBuilder)o;
            }

            TBuilder builder;
            if (typeof(TBuilder).GetConstructor(Type.EmptyTypes) != null)
            {
                builder = Activator.CreateInstance<TBuilder>();
            }
            else if (typeof(TBuilder).GetConstructor(new[] { typeof(IUmbracoBuilder) }) != null)
            {
                // Handle those collection builders which need a reference to umbraco builder i.e. DistributedLockingCollectionBuilder.
                builder = (TBuilder)Activator.CreateInstance(typeof(TBuilder), this)!;
            }
            else
            {
                throw new InvalidOperationException("A CollectionBuilder must have either a parameterless constructor or a constructor whose only parameter is of type IUmbracoBuilder");
            }

            _builders[typeOfBuilder] = builder;
            return builder;
        }

        public void Build()
        {
            foreach (ICollectionBuilder builder in _builders.Values)
            {
                builder.RegisterWith(Services);
            }

            _builders.Clear();
        }

        private void AddCoreServices()
        {
            Services.AddSingleton(AppCaches);
            Services.AddSingleton(Profiler);

            // Register as singleton to allow injection everywhere.
            Services.AddSingleton<ServiceFactory>(p => p.GetService!);
            Services.AddSingleton<IEventAggregator, EventAggregator>();

            Services.AddLazySupport();

            // Adds no-op registrations as many core services require these dependencies but these
            // dependencies cannot be fulfilled in the Core project
            Services.AddUnique<IMarchal, NoopMarchal>();
            Services.AddUnique<IApplicationShutdownRegistry, NoopApplicationShutdownRegistry>();

            Services.AddUnique<IMainDom, MainDom>();
            Services.AddUnique<IMainDomLock, MainDomSemaphoreLock>();

            Services.AddUnique<IIOHelper>(factory =>
            {
                IHostingEnvironment hostingEnvironment = factory.GetRequiredService<IHostingEnvironment>();

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return new IOHelperLinux(hostingEnvironment);
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return new IOHelperOSX(hostingEnvironment);
                }

                return new IOHelperWindows(hostingEnvironment);
            });

            Services.AddUnique(factory => factory.GetRequiredService<AppCaches>().RuntimeCache);
            Services.AddUnique(factory => factory.GetRequiredService<AppCaches>().RequestCache);
            Services.AddUnique<IProfilingLogger, ProfilingLogger>();
            Services.AddUnique<IUmbracoVersion, UmbracoVersion>();
            Services.AddUnique<IEntryAssemblyMetadata, EntryAssemblyMetadata>();

            this.AddAllCoreCollectionBuilders();
            this.AddNotificationHandler<UmbracoApplicationStartingNotification, EssentialDirectoryCreator>();

            Services.AddSingleton<UmbracoRequestPaths>();

            Services.AddUnique<ICultureDictionaryFactory, DefaultCultureDictionaryFactory>();
            Services.AddSingleton(f => f.GetRequiredService<ICultureDictionaryFactory>().CreateDictionary());

            Services.AddSingleton<UriUtility>();

            Services.AddSingleton<IMetricsConsentService, MetricsConsentService>();

            // will be injected in controllers when needed to invoke rest endpoints on Our
            Services.AddUnique<IInstallationService, InstallationService>();
            Services.AddUnique<IUpgradeService, UpgradeService>();

            Services.AddUnique<IPublishedUrlProvider, UrlProvider>();
            Services.AddUnique<ISiteDomainMapper, SiteDomainMapper>();

            Services.AddSingleton<HtmlLocalLinkParser>();
            Services.AddSingleton<HtmlImageSourceParser>();
            Services.AddSingleton<HtmlUrlParser>();

            // register properties fallback
            Services.AddUnique<IPublishedValueFallback, PublishedValueFallback>();

            Services.AddSingleton<UmbracoFeatures>();

            // register published router
            Services.AddUnique<IPublishedRouter, PublishedRouter>();
            Services.AddUnique<IPublishedUrlInfoProvider, PublishedUrlInfoProvider>();

            Services.AddUnique<IEventMessagesFactory, DefaultEventMessagesFactory>();
            Services.AddUnique<IEventMessagesAccessor, HybridEventMessagesAccessor>();

            Services.AddUnique<ISmsSender, NotImplementedSmsSender>();
            Services.AddUnique<IEmailSender, NotImplementedEmailSender>();

            Services.AddUnique<IDataValueEditorFactory, DataValueEditorFactory>();

            // register distributed cache
            Services.AddUnique(f => new DistributedCache(f.GetRequiredService<IServerMessenger>(), f.GetRequiredService<CacheRefresherCollection>()));
            Services.AddUnique<ICacheRefresherNotificationFactory, CacheRefresherNotificationFactory>();

            // register the http context and umbraco context accessors
            // we *should* use the HttpContextUmbracoContextAccessor, however there are cases when
            // we have no http context, eg when booting Umbraco or in background threads, so instead
            // let's use an hybrid accessor that can fall back to a ThreadStatic context.
            Services.AddUnique<IUmbracoContextAccessor, HybridUmbracoContextAccessor>();

            Services.AddSingleton<LegacyPasswordSecurity>();
            Services.AddSingleton<UserEditorAuthorizationHelper>();
            Services.AddSingleton<ContentPermissions>();
            Services.AddSingleton<MediaPermissions>();

            Services.AddSingleton<PropertyEditorCollection>();

            // register a server registrar, by default it's the db registrar
            Services.AddUnique<IServerRoleAccessor>(f =>
            {
                GlobalSettings globalSettings = f.GetRequiredService<IOptions<GlobalSettings>>().Value;
                var singleServer = globalSettings.DisableElectionForSingleServer;
                return singleServer
                    ? new SingleServerRoleAccessor()
                    : new ElectedServerRoleAccessor(f.GetRequiredService<IServerRegistrationService>());
            });

            // For Umbraco to work it must have the default IPublishedModelFactory
            // which may be replaced by models builder but the default is required to make plain old IPublishedContent
            // instances.
            Services.AddSingleton<IPublishedModelFactory>(factory => factory.CreateDefaultPublishedModelFactory());

            Services
                .AddNotificationAsyncHandler<MemberGroupSavingNotification, PublicAccessHandler>()
                .AddNotificationHandler<MemberGroupSavedNotification, PublicAccessHandler>()
                .AddNotificationAsyncHandler<MemberGroupDeletingNotification, PublicAccessHandler>()
                .AddNotificationHandler<MemberGroupDeletedNotification, PublicAccessHandler>();

            Services.AddSingleton<ISyncBootStateAccessor, NonRuntimeLevelBootStateAccessor>();

            // Register ValueEditorCache used for validation
            Services.AddSingleton<IValueEditorCache, ValueEditorCache>();

            // Register telemetry service used to gather data about installed packages
            Services.AddUnique<ISiteIdentifierService, SiteIdentifierService>();
            Services.AddUnique<ITelemetryService, TelemetryService>();

            Services.AddUnique<IKeyValueService, KeyValueService>();
            Services.AddUnique<IPublicAccessService, PublicAccessService>();
            Services.AddUnique<IContentVersionService, ContentVersionService>();
            Services.AddUnique<IUserGroupPermissionService, UserGroupPermissionService>();
            Services.AddUnique<IUserGroupService, UserGroupService>();
            Services.AddUnique<IUserPermissionService, UserPermissionService>();
            Services.AddUnique<IUserService, UserService>();
            Services.AddUnique<IWebProfilerService, WebProfilerService>();
            Services.AddUnique<ILocalizationService, LocalizationService>();
            Services.AddUnique<IDictionaryItemService, DictionaryItemService>();
            Services.AddUnique<IDataTypeContainerService, DataTypeContainerService>();
            Services.AddUnique<IContentTypeContainerService, ContentTypeContainerService>();
            Services.AddUnique<IMediaTypeContainerService, MediaTypeContainerService>();
            Services.AddUnique<IContentBlueprintContainerService, ContentBlueprintContainerService>();
            Services.AddUnique<IIsoCodeValidator, IsoCodeValidator>();
            Services.AddUnique<ICultureService, CultureService>();
            Services.AddUnique<ILanguageService, LanguageService>();
            Services.AddUnique<IMemberGroupService, MemberGroupService>();
            Services.AddUnique<IRedirectUrlService, RedirectUrlService>();
            Services.AddUnique<IConsentService, ConsentService>();
            Services.AddUnique<IPropertyValidationService, PropertyValidationService>();
            Services.AddUnique<IDomainService, DomainService>();
            Services.AddUnique<ITagService, TagService>();
            Services.AddUnique<IContentPermissionService, ContentPermissionService>();
            Services.AddUnique<IDictionaryPermissionService, DictionaryPermissionService>();
            Services.AddUnique<IContentService, ContentService>();
            Services.AddUnique<IContentBlueprintEditingService, ContentBlueprintEditingService>();
            Services.AddUnique<IContentEditingService, ContentEditingService>();
            Services.AddUnique<IContentPublishingService, ContentPublishingService>();
            Services.AddUnique<IContentValidationService, ContentValidationService>();
            Services.AddUnique<IContentVersionCleanupPolicy, DefaultContentVersionCleanupPolicy>();
            Services.AddUnique<IMemberService, MemberService>();
            Services.AddUnique<IMemberValidationService, MemberValidationService>();
            Services.AddUnique<IMediaPermissionService, MediaPermissionService>();
            Services.AddUnique<IMediaService, MediaService>();
            Services.AddUnique<IMediaEditingService, MediaEditingService>();
            Services.AddUnique<IMediaValidationService, MediaValidationService>();
            Services.AddUnique<IContentTypeService, ContentTypeService>();
            Services.AddUnique<IContentTypeBaseServiceProvider, ContentTypeBaseServiceProvider>();
            Services.AddUnique<IMediaTypeService, MediaTypeService>();
            Services.AddUnique<IContentTypeEditingService, ContentTypeEditingService>();
            Services.AddUnique<IMediaTypeEditingService, MediaTypeEditingService>();
            Services.AddUnique<IFileService, FileService>();
            Services.AddUnique<ITemplateService, TemplateService>();
            Services.AddUnique<IScriptService, ScriptService>();
            Services.AddUnique<IStylesheetService, StylesheetService>();
            Services.AddUnique<IStylesheetFolderService, StylesheetFolderService>();
            Services.AddUnique<IPartialViewService, PartialViewService>();
            Services.AddUnique<IScriptFolderService, ScriptFolderService>();
            Services.AddUnique<IPartialViewFolderService, PartialViewFolderService>();
            Services.AddUnique<ITemporaryFileService, TemporaryFileService>();
            Services.AddUnique<ITemplateContentParserService, TemplateContentParserService>();
            Services.AddUnique<IEntityService, EntityService>();
            Services.AddUnique<IOEmbedService, OEmbedService>();
            Services.AddUnique<IRelationService, RelationService>();
            Services.AddUnique<IMemberTypeService, MemberTypeService>();
            Services.AddUnique<IMemberContentEditingService, MemberContentEditingService>();
            Services.AddUnique<IMemberTypeEditingService, MemberTypeEditingService>();
            Services.AddUnique<INotificationService, NotificationService>();
            Services.AddUnique<ITrackedReferencesService, TrackedReferencesService>();
            Services.AddUnique<ITreeEntitySortingService, TreeEntitySortingService>();
            Services.AddUnique<ExternalLoginService>(factory => new ExternalLoginService(
                factory.GetRequiredService<ICoreScopeProvider>(),
                factory.GetRequiredService<ILoggerFactory>(),
                factory.GetRequiredService<IEventMessagesFactory>(),
                factory.GetRequiredService<IExternalLoginWithKeyRepository>()));
            Services.AddUnique<ILogViewerService, LogViewerService>();
            Services.AddUnique<IExternalLoginWithKeyService>(factory => factory.GetRequiredService<ExternalLoginService>());
            Services.AddUnique<ILocalizedTextService>(factory => new LocalizedTextService(
                factory.GetRequiredService<Lazy<LocalizedTextServiceFileSources>>(),
                factory.GetRequiredService<ILogger<LocalizedTextService>>()));

            Services.AddUnique<IEntityXmlSerializer, EntityXmlSerializer>();

            Services.AddSingleton<ConflictingPackageData>();
            Services.AddSingleton<CompiledPackageXmlParser>();
            Services.AddUnique<IPreviewTokenGenerator, NoopPreviewTokenGenerator>();
            Services.AddUnique<IPreviewService, PreviewService>();
            Services.AddUnique<DocumentNavigationService, DocumentNavigationService>();
            Services.AddUnique<IDocumentNavigationQueryService>(x => x.GetRequiredService<DocumentNavigationService>());
            Services.AddUnique<IDocumentNavigationManagementService>(x => x.GetRequiredService<DocumentNavigationService>());
            Services.AddUnique<MediaNavigationService, MediaNavigationService>();
            Services.AddUnique<IMediaNavigationQueryService>(x => x.GetRequiredService<MediaNavigationService>());
            Services.AddUnique<IMediaNavigationManagementService>(x => x.GetRequiredService<MediaNavigationService>());

            Services.AddUnique<PublishStatusService, PublishStatusService>();
            Services.AddUnique<IPublishStatusManagementService>(x => x.GetRequiredService<PublishStatusService>());
            Services.AddUnique<IPublishStatusQueryService>(x => x.GetRequiredService<PublishStatusService>());

            // Register a noop IHtmlSanitizer & IMarkdownSanitizer to be replaced
            Services.AddUnique<IHtmlSanitizer, NoopHtmlSanitizer>();
            Services.AddUnique<IMarkdownSanitizer, NoopMarkdownSanitizer>();

            Services.AddUnique<IPropertyTypeUsageService, PropertyTypeUsageService>();
            Services.AddUnique<IDataTypeUsageService, DataTypeUsageService>();

            Services.AddUnique<ICultureImpactFactory>(provider => new CultureImpactFactory(provider.GetRequiredService<IOptionsMonitor<ContentSettings>>()));
            Services.AddUnique<IDictionaryService, DictionaryService>();
            Services.AddUnique<ITemporaryMediaService, TemporaryMediaService>();
            Services.AddUnique<IMediaImportService, MediaImportService>();

            // Register filestream security analyzers
            Services.AddUnique<IFileStreamSecurityValidator,FileStreamSecurityValidator>();
            Services.AddUnique<IDynamicRootService,DynamicRoot.DynamicRootService>();

            // Register Webhook services
            Services.AddUnique<IWebhookService, WebhookService>();
            Services.AddUnique<IWebhookLogService, WebhookLogService>();
            Services.AddUnique<IWebhookLogFactory, WebhookLogFactory>();
            Services.AddUnique<IWebhookRequestService>(factory => new WebhookRequestService(
                factory.GetRequiredService<ICoreScopeProvider>(),
                factory.GetRequiredService<IWebhookRequestRepository>(),
                factory.GetRequiredService<IWebhookJsonSerializer>()));

            // Data type configuration cache
            Services.AddUnique<IDataTypeConfigurationCache, DataTypeConfigurationCache>();
            Services.AddNotificationHandler<DataTypeCacheRefresherNotification, DataTypeConfigurationCacheRefresher>();

            // Two factor providers
            Services.AddUnique<ITwoFactorLoginService, TwoFactorLoginService>();
            Services.AddUnique<IUserTwoFactorLoginService, UserTwoFactorLoginService>();

            // Add Query services
            Services.AddUnique<IDocumentRecycleBinQueryService, DocumentRecycleBinQueryService>();
            Services.AddUnique<IMediaRecycleBinQueryService, MediaRecycleBinQueryService>();
            Services.AddUnique<IContentQueryService, ContentQueryService>();

            // Authorizers
            Services.AddSingleton<IAuthorizationHelper, AuthorizationHelper>();
            Services.AddSingleton<IContentPermissionAuthorizer, ContentPermissionAuthorizer>();
            Services.AddSingleton<IDictionaryPermissionAuthorizer, DictionaryPermissionAuthorizer>();
            Services.AddSingleton<IFeatureAuthorizer, FeatureAuthorizer>();
            Services.AddSingleton<IMediaPermissionAuthorizer, MediaPermissionAuthorizer>();
            Services.AddSingleton<IUserGroupPermissionAuthorizer, UserGroupPermissionAuthorizer>();
            Services.AddSingleton<IUserPermissionAuthorizer, UserPermissionAuthorizer>();

            // Segments
            Services.AddUnique<ISegmentService, NoopSegmentService>();

            // definition Import/export
            Services.AddUnique<ITemporaryFileToXmlImportService, TemporaryFileToXmlImportService>();
            Services.AddUnique<IContentTypeImportService, ContentTypeImportService>();
            Services.AddUnique<IMediaTypeImportService, MediaTypeImportService>();

            // add validation services
            Services.AddUnique<IElementSwitchValidator, ElementSwitchValidator>();

            // Routing
            Services.AddUnique<IDocumentUrlService, DocumentUrlService>();
            Services.AddNotificationAsyncHandler<UmbracoApplicationStartingNotification, DocumentUrlServiceInitializerNotificationHandler>();
        }
    }
}

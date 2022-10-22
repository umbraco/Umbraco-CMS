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
using Umbraco.Cms.Core.Configuration.Grid;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Diagnostics;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Editors;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Features;
using Umbraco.Cms.Core.Handlers;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.PublishedCache.Internal;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Snippets;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

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

            Services.AddSingleton<InstallStatusTracker>();

            Services.AddUnique<ICultureDictionaryFactory, DefaultCultureDictionaryFactory>();
            Services.AddSingleton(f => f.GetRequiredService<ICultureDictionaryFactory>().CreateDictionary());

            Services.AddSingleton<UriUtility>();

            Services.AddUnique<IDashboardService, DashboardService>();
            Services.AddSingleton<IMetricsConsentService, MetricsConsentService>();

            // will be injected in controllers when needed to invoke rest endpoints on Our
            Services.AddUnique<IInstallationService, InstallationService>();
            Services.AddUnique<IUpgradeService, UpgradeService>();

            // Grid config is not a real config file as we know them
            Services.AddUnique<IGridConfig, GridConfig>();

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

            Services.AddUnique<IEventMessagesFactory, DefaultEventMessagesFactory>();
            Services.AddUnique<IEventMessagesAccessor, HybridEventMessagesAccessor>();
            Services.AddUnique<ITreeService, TreeService>();
            Services.AddUnique<ISectionService, SectionService>();

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
            Services.AddSingleton<ParameterEditorCollection>();

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
                .AddNotificationHandler<MemberGroupSavedNotification, PublicAccessHandler>()
                .AddNotificationHandler<MemberGroupDeletedNotification, PublicAccessHandler>();

            Services.AddSingleton<ISyncBootStateAccessor, NonRuntimeLevelBootStateAccessor>();

            // register a basic/noop published snapshot service to be replaced
            Services.AddSingleton<IPublishedSnapshotService, InternalPublishedSnapshotService>();

            // Register ValueEditorCache used for validation
            Services.AddSingleton<IValueEditorCache, ValueEditorCache>();

            // Register telemetry service used to gather data about installed packages
            Services.AddUnique<ISiteIdentifierService, SiteIdentifierService>();
            Services.AddUnique<ITelemetryService, TelemetryService>();

            Services.AddUnique<IKeyValueService, KeyValueService>();
            Services.AddUnique<IPublicAccessService, PublicAccessService>();
            Services.AddUnique<IContentVersionService, ContentVersionService>();
            Services.AddUnique<IUserService, UserService>();
            Services.AddUnique<ILocalizationService, LocalizationService>();
            Services.AddUnique<IMacroService, MacroService>();
            Services.AddUnique<IMemberGroupService, MemberGroupService>();
            Services.AddUnique<IRedirectUrlService, RedirectUrlService>();
            Services.AddUnique<IConsentService, ConsentService>();
            Services.AddUnique<IPropertyValidationService, PropertyValidationService>();
            Services.AddUnique<IDomainService, DomainService>();
            Services.AddUnique<ITagService, TagService>();
            Services.AddUnique<IContentService, ContentService>();
            Services.AddUnique<IContentVersionCleanupPolicy, DefaultContentVersionCleanupPolicy>();
            Services.AddUnique<IMemberService, MemberService>();
            Services.AddUnique<IMediaService, MediaService>();
            Services.AddUnique<IContentTypeService, ContentTypeService>();
            Services.AddUnique<IContentTypeBaseServiceProvider, ContentTypeBaseServiceProvider>();
            Services.AddUnique<IMediaTypeService, MediaTypeService>();
            Services.AddUnique<IFileService, FileService>();
            Services.AddUnique<IEntityService, EntityService>();
            Services.AddUnique<IRelationService, RelationService>();
            Services.AddUnique<IMemberTypeService, MemberTypeService>();
            Services.AddUnique<INotificationService, NotificationService>();
            Services.AddUnique<ITrackedReferencesService, TrackedReferencesService>();
            Services.AddUnique<ExternalLoginService>(factory => new ExternalLoginService(
                factory.GetRequiredService<ICoreScopeProvider>(),
                factory.GetRequiredService<ILoggerFactory>(),
                factory.GetRequiredService<IEventMessagesFactory>(),
                factory.GetRequiredService<IExternalLoginWithKeyRepository>()
            ));
            Services.AddUnique<IExternalLoginService>(factory => factory.GetRequiredService<ExternalLoginService>());
            Services.AddUnique<IExternalLoginWithKeyService>(factory => factory.GetRequiredService<ExternalLoginService>());
            Services.AddUnique<ILocalizedTextService>(factory => new LocalizedTextService(
                factory.GetRequiredService<Lazy<LocalizedTextServiceFileSources>>(),
                factory.GetRequiredService<ILogger<LocalizedTextService>>()));

            Services.AddUnique<IEntityXmlSerializer, EntityXmlSerializer>();

            Services.AddSingleton<ConflictingPackageData>();
            Services.AddSingleton<CompiledPackageXmlParser>();

            // Register a noop IHtmlSanitizer to be replaced
            Services.AddUnique<IHtmlSanitizer, NoopHtmlSanitizer>();

            Services.AddUnique<IPropertyTypeUsageService, PropertyTypeUsageService>();
            Services.AddUnique<IDataTypeUsageService, DataTypeUsageService>();

            Services.AddUnique<ICultureImpactFactory>(provider => new CultureImpactFactory(provider.GetRequiredService<IOptionsMonitor<ContentSettings>>()));
            Services.AddUnique<IDictionaryService, DictionaryService>();
        }
    }
}

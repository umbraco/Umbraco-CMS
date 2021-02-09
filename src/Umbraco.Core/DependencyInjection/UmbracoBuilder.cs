// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
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
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.DependencyInjection
{
    public class UmbracoBuilder : IUmbracoBuilder
    {
        private readonly Dictionary<Type, ICollectionBuilder> _builders = new Dictionary<Type, ICollectionBuilder>();

        public IServiceCollection Services { get; }

        public IConfiguration Config { get; }

        public TypeLoader TypeLoader { get; }

        public ILoggerFactory BuilderLoggerFactory { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoBuilder"/> class.
        /// </summary>
        public UmbracoBuilder(IServiceCollection services, IConfiguration config, TypeLoader typeLoader)
            : this(services, config, typeLoader, NullLoggerFactory.Instance)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoBuilder"/> class.
        /// </summary>
        public UmbracoBuilder(IServiceCollection services, IConfiguration config, TypeLoader typeLoader, ILoggerFactory loggerFactory)
        {
            Services = services;
            Config = config;
            BuilderLoggerFactory = loggerFactory;
            TypeLoader = typeLoader;

            AddCoreServices();
        }

        /// <summary>
        /// Gets a collection builder (and registers the collection).
        /// </summary>
        /// <typeparam name="TBuilder">The type of the collection builder.</typeparam>
        /// <returns>The collection builder.</returns>
        public TBuilder WithCollectionBuilder<TBuilder>()
            where TBuilder : ICollectionBuilder, new()
        {
            Type typeOfBuilder = typeof(TBuilder);

            if (_builders.TryGetValue(typeOfBuilder, out ICollectionBuilder o))
            {
                return (TBuilder)o;
            }

            var builder = new TBuilder();
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
            // Register as singleton to allow injection everywhere.
            Services.AddSingleton<ServiceFactory>(p => p.GetService);
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

            this.AddAllCoreCollectionBuilders();
            this.AddNotificationHandler<UmbracoApplicationStarting, EssentialDirectoryCreator>();

            Services.AddSingleton<ManifestWatcher>();
            Services.AddSingleton<UmbracoRequestPaths>();
            this.AddNotificationAsyncHandler<UmbracoApplicationStarting, AppPluginsManifestWatcherNotificationHandler>();

            Services.AddUnique<InstallStatusTracker>();

            // by default, register a noop factory
            Services.AddUnique<IPublishedModelFactory, NoopPublishedModelFactory>();

            Services.AddUnique<ICultureDictionaryFactory, DefaultCultureDictionaryFactory>();
            Services.AddSingleton(f => f.GetRequiredService<ICultureDictionaryFactory>().CreateDictionary());

            Services.AddUnique<UriUtility>();

            Services.AddUnique<IDashboardService, DashboardService>();

            // will be injected in controllers when needed to invoke rest endpoints on Our
            Services.AddUnique<IInstallationService, InstallationService>();
            Services.AddUnique<IUpgradeService, UpgradeService>();

            // Grid config is not a real config file as we know them
            Services.AddUnique<IGridConfig, GridConfig>();

            Services.AddUnique<IPublishedUrlProvider, UrlProvider>();
            Services.AddUnique<ISiteDomainHelper, SiteDomainHelper>();

            Services.AddUnique<HtmlLocalLinkParser>();
            Services.AddUnique<HtmlImageSourceParser>();
            Services.AddUnique<HtmlUrlParser>();

            // register properties fallback
            Services.AddUnique<IPublishedValueFallback, PublishedValueFallback>();

            Services.AddUnique<UmbracoFeatures>();

            // register published router
            Services.AddUnique<IPublishedRouter, PublishedRouter>();

            Services.AddUnique<IEventMessagesFactory, DefaultEventMessagesFactory>();
            Services.AddUnique<IEventMessagesAccessor, HybridEventMessagesAccessor>();
            Services.AddUnique<ITreeService, TreeService>();
            Services.AddUnique<ISectionService, SectionService>();

            Services.AddUnique<ISmsSender, NotImplementedSmsSender>();
            Services.AddUnique<IEmailSender, NotImplementedEmailSender>();

            // register distributed cache
            Services.AddUnique(f => new DistributedCache(f.GetRequiredService<IServerMessenger>(), f.GetRequiredService<CacheRefresherCollection>()));

            // register the http context and umbraco context accessors
            // we *should* use the HttpContextUmbracoContextAccessor, however there are cases when
            // we have no http context, eg when booting Umbraco or in background threads, so instead
            // let's use an hybrid accessor that can fall back to a ThreadStatic context.
            Services.AddUnique<IUmbracoContextAccessor, HybridUmbracoContextAccessor>();

            Services.AddUnique<LegacyPasswordSecurity>();
            Services.AddUnique<UserEditorAuthorizationHelper>();
            Services.AddUnique<ContentPermissions>();
            Services.AddUnique<MediaPermissions>();

            Services.AddUnique<PropertyEditorCollection>();
            Services.AddUnique<ParameterEditorCollection>();

            // register a server registrar, by default it's the db registrar
            Services.AddUnique<IServerRoleAccessor>(f =>
            {
                GlobalSettings globalSettings = f.GetRequiredService<IOptions<GlobalSettings>>().Value;
                var singleServer = globalSettings.DisableElectionForSingleServer;
                return singleServer
                    ? (IServerRoleAccessor)new SingleServerRoleAccessor()
                    : new ElectedServerRoleAccessor(f.GetRequiredService<IServerRegistrationService>());
            });

            // For Umbraco to work it must have the default IPublishedModelFactory
            // which may be replaced by models builder but the default is required to make plain old IPublishedContent
            // instances.
            Services.AddSingleton<IPublishedModelFactory>(factory => factory.CreateDefaultPublishedModelFactory());
        }
    }
}

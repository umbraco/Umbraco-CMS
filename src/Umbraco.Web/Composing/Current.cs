using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.HealthChecks;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Scoping;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Core.Templates;
using Umbraco.Core.WebAssets;
using Umbraco.Net;
using Umbraco.Web.Actions;
using Umbraco.Web.Cache;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.Services;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Composing
{
    // see notes in Umbraco.Core.Composing.Current.
    public static class Current
    {
        private static readonly object Locker = new object();

        private static IServiceProvider _factory;

        /// <summary>
        /// Gets or sets the factory.
        /// </summary>
        public static IServiceProvider Factory
        {
            get
            {
                if (_factory == null)
                    throw new InvalidOperationException("No factory has been set.");
                return _factory;
            }
            set
            {
                _factory = value;
            }
        }

        private static IUmbracoContextAccessor _umbracoContextAccessor;


        #region Temp & Special

        // TODO: have to keep this until tests are refactored
        // but then, it should all be managed properly in the container
        public static IUmbracoContextAccessor UmbracoContextAccessor
        {
            get
            {
                if (_umbracoContextAccessor != null) return _umbracoContextAccessor;
                return _umbracoContextAccessor = Factory.GetRequiredService<IUmbracoContextAccessor>();
            }
            set => _umbracoContextAccessor = value; // for tests
        }

        #endregion

        #region Web Getters

        public static IUmbracoContext UmbracoContext
            => UmbracoContextAccessor.UmbracoContext;

        public static IBackOfficeSecurityAccessor BackOfficeSecurityAccessor
            => Factory.GetRequiredService<IBackOfficeSecurityAccessor>();

        public static UmbracoHelper UmbracoHelper
            => Factory.GetRequiredService<UmbracoHelper>();
        public static IUmbracoComponentRenderer UmbracoComponentRenderer
            => Factory.GetRequiredService<IUmbracoComponentRenderer>();
        public static ITagQuery TagQuery
            => Factory.GetRequiredService<ITagQuery>();

        public static IRuntimeMinifier RuntimeMinifier
            => Factory.GetRequiredService<IRuntimeMinifier>();

        public static DistributedCache DistributedCache
            => Factory.GetRequiredService<DistributedCache>();

        public static IPublishedSnapshot PublishedSnapshot
            => Factory.GetRequiredService<IPublishedSnapshotAccessor>().PublishedSnapshot;

        public static EventMessages EventMessages
            => Factory.GetRequiredService<IEventMessagesFactory>().GetOrDefault();

        public static UrlProviderCollection UrlProviders
            => Factory.GetRequiredService<UrlProviderCollection>();

        public static MediaUrlProviderCollection MediaUrlProviders
            => Factory.GetRequiredService<MediaUrlProviderCollection>();

        public static HealthCheckCollectionBuilder HealthCheckCollectionBuilder
            => Factory.GetRequiredService<HealthCheckCollectionBuilder>();

        internal static ActionCollectionBuilder ActionCollectionBuilder
            => Factory.GetRequiredService<ActionCollectionBuilder>();

        public static ActionCollection Actions
            => Factory.GetRequiredService<ActionCollection>();

        public static ContentFinderCollection ContentFinders
            => Factory.GetRequiredService<ContentFinderCollection>();

        public static IContentLastChanceFinder LastChanceContentFinder
            => Factory.GetRequiredService<IContentLastChanceFinder>();

        internal static EditorValidatorCollection EditorValidators
            => Factory.GetRequiredService<EditorValidatorCollection>();

        internal static UmbracoApiControllerTypeCollection UmbracoApiControllerTypes
            => Factory.GetRequiredService<UmbracoApiControllerTypeCollection>();

        internal static IPublishedSnapshotService PublishedSnapshotService
            => Factory.GetRequiredService<IPublishedSnapshotService>();

        public static ITreeService TreeService
            => Factory.GetRequiredService<ITreeService>();

        public static ISectionService SectionService
            => Factory.GetRequiredService<ISectionService>();

        public static IIconService IconService
            => Factory.GetRequiredService<IIconService>();

        #endregion

        
        #region Core Getters

        // proxy Core for convenience

        public static IMediaFileSystem MediaFileSystem => Factory.GetRequiredService<IMediaFileSystem>();

        public static UmbracoMapper Mapper => Factory.GetRequiredService<UmbracoMapper>();

        public static IRuntimeState RuntimeState => Factory.GetRequiredService<IRuntimeState>();

        public static CacheRefresherCollection CacheRefreshers => Factory.GetRequiredService<CacheRefresherCollection>();

        internal static IPublishedModelFactory PublishedModelFactory => Factory.GetRequiredService<IPublishedModelFactory>();

        public static IServerMessenger ServerMessenger => Factory.GetRequiredService<IServerMessenger>();

        public static ILogger<object> Logger => Factory.GetRequiredService<ILogger<object>>();

        public static ILoggerFactory LoggerFactory => Factory.GetRequiredService<ILoggerFactory>();

        public static IProfiler Profiler => Factory.GetRequiredService<IProfiler>();

        public static IProfilerHtml ProfilerHtml => Factory.GetRequiredService<IProfilerHtml>();

        public static IProfilingLogger ProfilingLogger => Factory.GetRequiredService<IProfilingLogger>();

        public static AppCaches AppCaches => Factory.GetRequiredService<AppCaches>();

        public static ServiceContext Services => Factory.GetRequiredService<ServiceContext>();

        public static IScopeProvider ScopeProvider => Factory.GetRequiredService<IScopeProvider>();

        public static IPublishedContentTypeFactory PublishedContentTypeFactory => Factory.GetRequiredService<IPublishedContentTypeFactory>();

        public static IPublishedValueFallback PublishedValueFallback => Factory.GetRequiredService<IPublishedValueFallback>();

        public static IVariationContextAccessor VariationContextAccessor => Factory.GetRequiredService<IVariationContextAccessor>();

        public static IIOHelper IOHelper => Factory.GetRequiredService<IIOHelper>();
        public static IHostingEnvironment HostingEnvironment => Factory.GetRequiredService<IHostingEnvironment>();
        public static IIpResolver IpResolver => Factory.GetRequiredService<IIpResolver>();
        public static IUmbracoVersion UmbracoVersion => Factory.GetRequiredService<IUmbracoVersion>();
        public static IPublishedUrlProvider PublishedUrlProvider => Factory.GetRequiredService<IPublishedUrlProvider>();
        public static IMenuItemCollectionFactory MenuItemCollectionFactory => Factory.GetRequiredService<IMenuItemCollectionFactory>();
        public static MembershipHelper MembershipHelper => Factory.GetRequiredService<MembershipHelper>();
        public static IUmbracoApplicationLifetime UmbracoApplicationLifetime => Factory.GetRequiredService<IUmbracoApplicationLifetime>();
        public static IPublishedContentQuery PublishedContentQuery => Factory.GetRequiredService<IPublishedContentQuery>();

        #endregion
    }
}

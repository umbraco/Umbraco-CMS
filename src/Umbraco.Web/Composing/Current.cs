using System;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.HealthCheck;
using Umbraco.Core.Hosting;
using Umbraco.Core.Mapping;
using Umbraco.Core.Templates;
using Umbraco.Net;
using Umbraco.Core.PackageActions;
using Umbraco.Core.Packaging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Runtime;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;
using Umbraco.Core.WebAssets;
using Umbraco.Web.Actions;
using Umbraco.Web.Cache;
using Umbraco.Web.Editors;
using Umbraco.Web.HealthCheck;
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

        private static IFactory _factory;

        /// <summary>
        /// Gets or sets the factory.
        /// </summary>
        public static IFactory Factory
        {
            get
            {
                if (_factory == null)
                    throw new InvalidOperationException("No factory has been set.");
                return _factory;
            }
            set
            {
                if (_factory != null)
                    throw new InvalidOperationException("A factory has already been set.");
                _factory = value;
            }
        }

        private static IUmbracoContextAccessor _umbracoContextAccessor;

        static Current()
        {
            Resetted += (sender, args) =>
            {
                if (_umbracoContextAccessor != null)
                {
                    var umbracoContext = _umbracoContextAccessor.UmbracoContext;
                    umbracoContext?.Dispose();
                }
                _umbracoContextAccessor = null;
            };
        }

        /// <summary>
        /// for UNIT TESTS exclusively! Resets <see cref="Current"/>. Indented for testing only, and not supported in production code.
        /// </summary>
        /// <remarks>
        /// <para>For UNIT TESTS exclusively.</para>
        /// <para>Resets everything that is 'current'.</para>
        /// </remarks>
        public static void Reset()
        {
            _factory.DisposeIfDisposable();
            _factory = null;

            Resetted?.Invoke(null, EventArgs.Empty);
        }

        internal static event EventHandler Resetted;


        #region Temp & Special

        // TODO: have to keep this until tests are refactored
        // but then, it should all be managed properly in the container
        public static IUmbracoContextAccessor UmbracoContextAccessor
        {
            get
            {
                if (_umbracoContextAccessor != null) return _umbracoContextAccessor;
                return _umbracoContextAccessor = Factory.GetInstance<IUmbracoContextAccessor>();
            }
            set => _umbracoContextAccessor = value; // for tests
        }

        #endregion

        #region Web Getters

        public static IUmbracoContext UmbracoContext
            => UmbracoContextAccessor.UmbracoContext;

        public static UmbracoHelper UmbracoHelper
            => Factory.GetInstance<UmbracoHelper>();
        public static IUmbracoComponentRenderer UmbracoComponentRenderer
            => Factory.GetInstance<IUmbracoComponentRenderer>();
        public static ITagQuery TagQuery
            => Factory.GetInstance<ITagQuery>();

        public static IRuntimeMinifier RuntimeMinifier
            => Factory.GetInstance<IRuntimeMinifier>();

        public static DistributedCache DistributedCache
            => Factory.GetInstance<DistributedCache>();

        public static IPublishedSnapshot PublishedSnapshot
            => Factory.GetInstance<IPublishedSnapshotAccessor>().PublishedSnapshot;

        public static EventMessages EventMessages
            => Factory.GetInstance<IEventMessagesFactory>().GetOrDefault();

        public static UrlProviderCollection UrlProviders
            => Factory.GetInstance<UrlProviderCollection>();

        public static MediaUrlProviderCollection MediaUrlProviders
            => Factory.GetInstance<MediaUrlProviderCollection>();

        public static HealthCheckCollectionBuilder HealthCheckCollectionBuilder
            => Factory.GetInstance<HealthCheckCollectionBuilder>();

        internal static ActionCollectionBuilder ActionCollectionBuilder
            => Factory.GetInstance<ActionCollectionBuilder>();

        public static ActionCollection Actions
            => Factory.GetInstance<ActionCollection>();

        public static ContentFinderCollection ContentFinders
            => Factory.GetInstance<ContentFinderCollection>();

        public static IContentLastChanceFinder LastChanceContentFinder
            => Factory.GetInstance<IContentLastChanceFinder>();

        internal static EditorValidatorCollection EditorValidators
            => Factory.GetInstance<EditorValidatorCollection>();

        internal static UmbracoApiControllerTypeCollection UmbracoApiControllerTypes
            => Factory.GetInstance<UmbracoApiControllerTypeCollection>();

        internal static SurfaceControllerTypeCollection SurfaceControllerTypes
            => Factory.GetInstance<SurfaceControllerTypeCollection>();

        internal static IPublishedSnapshotService PublishedSnapshotService
            => Factory.GetInstance<IPublishedSnapshotService>();

        public static ITreeService TreeService
            => Factory.GetInstance<ITreeService>();

        public static ISectionService SectionService
            => Factory.GetInstance<ISectionService>();

        public static IIconService IconService
            => Factory.GetInstance<IIconService>();

        #endregion

        #region Web Constants

        // these are different - not 'resolving' anything, and nothing that could be managed
        // by the container - just registering some sort of application-wide constants or
        // settings - but they fit in Current nicely too

        private static Type _defaultRenderMvcControllerType;

        // internal - can only be accessed through Composition at compose time
        internal static Type DefaultRenderMvcControllerType
        {
            get => _defaultRenderMvcControllerType;
            set
            {
                if (value.Implements<IRenderController>() == false)
                    throw new ArgumentException($"Type {value.FullName} does not implement {typeof(IRenderController).FullName}.", nameof(value));
                _defaultRenderMvcControllerType = value;
            }
        }

        #endregion

        #region Core Getters

        // proxy Core for convenience

        public static IMediaFileSystem MediaFileSystem => Factory.GetInstance<IMediaFileSystem>();

        public static UmbracoMapper Mapper => Factory.GetInstance<UmbracoMapper>();

        public static IRuntimeState RuntimeState => Factory.GetInstance<IRuntimeState>();

        public static CacheRefresherCollection CacheRefreshers => Factory.GetInstance<CacheRefresherCollection>();

        internal static IPublishedModelFactory PublishedModelFactory => Factory.GetInstance<IPublishedModelFactory>();

        public static IServerMessenger ServerMessenger => Factory.GetInstance<IServerMessenger>();

        public static ILogger<object> Logger => Factory.GetInstance<ILogger<object>>();

        public static ILoggerFactory LoggerFactory => Factory.GetInstance<ILoggerFactory>();

        public static IProfiler Profiler => Factory.GetInstance<IProfiler>();

        public static IProfilerHtml ProfilerHtml => Factory.GetInstance<IProfilerHtml>();

        public static IProfilingLogger ProfilingLogger => Factory.GetInstance<IProfilingLogger>();

        public static AppCaches AppCaches => Factory.GetInstance<AppCaches>();

        public static ServiceContext Services => Factory.GetInstance<ServiceContext>();

        public static IScopeProvider ScopeProvider => Factory.GetInstance<IScopeProvider>();

        public static IPublishedContentTypeFactory PublishedContentTypeFactory => Factory.GetInstance<IPublishedContentTypeFactory>();

        public static IPublishedValueFallback PublishedValueFallback => Factory.GetInstance<IPublishedValueFallback>();

        public static IVariationContextAccessor VariationContextAccessor => Factory.GetInstance<IVariationContextAccessor>();

        public static IIOHelper IOHelper => Factory.GetInstance<IIOHelper>();
        public static IHostingEnvironment HostingEnvironment => Factory.GetInstance<IHostingEnvironment>();
        public static IIpResolver IpResolver => Factory.GetInstance<IIpResolver>();
        public static IUmbracoVersion UmbracoVersion => Factory.GetInstance<IUmbracoVersion>();
        public static IPublishedUrlProvider PublishedUrlProvider => Factory.GetInstance<IPublishedUrlProvider>();
        public static IMenuItemCollectionFactory MenuItemCollectionFactory => Factory.GetInstance<IMenuItemCollectionFactory>();
        public static MembershipHelper MembershipHelper => Factory.GetInstance<MembershipHelper>();
        public static IUmbracoApplicationLifetime UmbracoApplicationLifetime => Factory.GetInstance<IUmbracoApplicationLifetime>();
        public static IPublishedContentQuery PublishedContentQuery => Factory.GetInstance<IPublishedContentQuery>();

        #endregion
    }
}

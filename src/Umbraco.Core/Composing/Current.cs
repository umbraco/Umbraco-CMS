using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dictionary;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;
using Umbraco.Core._Legacy.PackageActions;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Provides a static service locator for most singletons.
    /// </summary>
    /// <remarks>
    /// <para>This class is initialized with the container in UmbracoApplicationBase,
    /// right after the container is created in UmbracoApplicationBase.HandleApplicationStart.</para>
    /// <para>Obviously, this is a service locator, which some may consider an anti-pattern. And yet,
    /// practically, it works.</para>
    /// </remarks>
    public static class Current
    {
        private static IFactory _factory;

        // fixme - refactor
        // we don't want Umbraco tests to die because the container has not been properly initialized,
        // for some too-important things such as IShortStringHelper or loggers, so if it's not
        // registered we setup a default one. We should really refactor our tests so that it does
        // not happen.

        private static IShortStringHelper _shortStringHelper;
        private static ILogger _logger;
        private static IProfiler _profiler;
        private static IProfilingLogger _profilingLogger;
        private static IPublishedValueFallback _publishedValueFallback;
        private static UmbracoConfig _config;

        /// <summary>
        /// Gets or sets the factory.
        /// </summary>
        public static IFactory Factory
        {
            get
            {
                if (_factory == null) throw new Exception("No factory has been set.");
                return _factory;
            }
            set
            {
                if (_factory != null) throw new Exception("A factory has already been set.");
                _factory = value;
            }
        }

        internal static bool HasContainer => _factory != null;

        // for UNIT TESTS exclusively!
        // resets *everything* that is 'current'
        internal static void Reset()
        {
            _factory.DisposeIfDisposable();
            _factory = null;

            _shortStringHelper = null;
            _logger = null;
            _profiler = null;
            _profilingLogger = null;
            _publishedValueFallback = null;

            Resetted?.Invoke(null, EventArgs.Empty);
        }

        internal static event EventHandler Resetted;

        #region Getters

        public static IShortStringHelper ShortStringHelper
            => _shortStringHelper ?? (_shortStringHelper = _factory?.TryGetInstance<IShortStringHelper>()
                ?? new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(Config.Umbraco())));

        public static ILogger Logger
            => _logger ?? (_logger = _factory?.TryGetInstance<ILogger>()
                ?? new DebugDiagnosticsLogger());

        public static IProfiler Profiler
            => _profiler ?? (_profiler = _factory?.TryGetInstance<IProfiler>()
                ?? new LogProfiler(Logger));

        public static IProfilingLogger ProfilingLogger
            => _profilingLogger ?? (_profilingLogger = _factory?.TryGetInstance<IProfilingLogger>())
               ?? new ProfilingLogger(Logger, Profiler);

        public static IRuntimeState RuntimeState
            => Factory.GetInstance<IRuntimeState>();

        public static TypeLoader TypeLoader
            => Factory.GetInstance<TypeLoader>();

        public static UmbracoConfig Config
            => _config ?? (_config = _factory?.TryGetInstance<UmbracoConfig>())
               ?? (_config = new UmbracoConfig(Logger, _factory?.TryGetInstance<IRuntimeCacheProvider>(), _factory?.TryGetInstance<IRuntimeState>()));

        public static IFileSystems FileSystems
            => Factory.GetInstance<IFileSystems>();

        public static IMediaFileSystem MediaFileSystem
            => Factory.GetInstance<IMediaFileSystem>();

        public static UrlSegmentProviderCollection UrlSegmentProviders
            => Factory.GetInstance<UrlSegmentProviderCollection>();

        public static CacheRefresherCollection CacheRefreshers
            => Factory.GetInstance<CacheRefresherCollection>();

        public static DataEditorCollection DataEditors
            => Factory.GetInstance<DataEditorCollection>();

        public static PropertyEditorCollection PropertyEditors
            => Factory.GetInstance<PropertyEditorCollection>();

        public static ParameterEditorCollection ParameterEditors
            => Factory.GetInstance<ParameterEditorCollection>();

        internal static ManifestValueValidatorCollection ManifestValidators
            => Factory.GetInstance<ManifestValueValidatorCollection>();

        internal static PackageActionCollection PackageActions
            => Factory.GetInstance<PackageActionCollection>();

        internal static PropertyValueConverterCollection PropertyValueConverters
            => Factory.GetInstance<PropertyValueConverterCollection>();

        internal static IPublishedModelFactory PublishedModelFactory
            => Factory.GetInstance<IPublishedModelFactory>();

        public static IServerMessenger ServerMessenger
            => Factory.GetInstance<IServerMessenger>();

        public static IServerRegistrar ServerRegistrar
            => Factory.GetInstance<IServerRegistrar>();

        public static ICultureDictionaryFactory CultureDictionaryFactory
            => Factory.GetInstance<ICultureDictionaryFactory>();

        public static CacheHelper ApplicationCache
            => Factory.GetInstance<CacheHelper>();

        public static ServiceContext Services
            => Factory.GetInstance<ServiceContext>();

        public static IScopeProvider ScopeProvider
            => Factory.GetInstance<IScopeProvider>();

        public static ISqlContext SqlContext
            => Factory.GetInstance<ISqlContext>();

        public static IPublishedContentTypeFactory PublishedContentTypeFactory
            => Factory.GetInstance<IPublishedContentTypeFactory>();

        public static IPublishedValueFallback PublishedValueFallback
            => _publishedValueFallback ?? Factory.GetInstance<IPublishedValueFallback>() ?? new NoopPublishedValueFallback();

        public static IVariationContextAccessor VariationContextAccessor
            => Factory.GetInstance<IVariationContextAccessor>();

        #endregion
    }
}

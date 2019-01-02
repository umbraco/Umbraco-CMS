using System;
using LightInject;
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
    /// <para>This class is initialized with the container via LightInjectExtensions.ConfigureUmbracoCore,
    /// right after the container is created in UmbracoApplicationBase.HandleApplicationStart.</para>
    /// <para>Obviously, this is a service locator, which some may consider an anti-pattern. And yet,
    /// practically, it works.</para>
    /// </remarks>
    public static class Current
    {
        private static IServiceContainer _container;

        private static IShortStringHelper _shortStringHelper;
        private static ILogger _logger;
        private static IProfiler _profiler;
        private static ProfilingLogger _profilingLogger;
        private static IPublishedValueFallback _publishedValueFallback;

        /// <summary>
        /// Gets or sets the DI container.
        /// </summary>
        public static IServiceContainer Container
        {
            get
            {
                if (_container == null) throw new Exception("No container has been set.");
                return _container;
            }
            set
            {
                if (_container != null) throw new Exception("A container has already been set.");
                _container = value;
            }
        }

        internal static bool HasContainer => _container != null;

        // for UNIT TESTS exclusively!
        // resets *everything* that is 'current'
        internal static void Reset()
        {
            _container?.Dispose();
            _container = null;

            _shortStringHelper = null;
            _logger = null;
            _profiler = null;
            _profilingLogger = null;
            _publishedValueFallback = null;

            Resetted?.Invoke(null, EventArgs.Empty);
        }

        internal static event EventHandler Resetted;

        #region Getters

        // fixme - refactor
        // we don't want Umbraco to die because the container has not been properly initialized,
        // for some too-important things such as IShortStringHelper or loggers, so if it's not
        // registered we setup a default one. We should really refactor our tests so that it does
        // not happen. Will do when we get rid of IShortStringHelper.

        public static IShortStringHelper ShortStringHelper
            => _shortStringHelper ?? (_shortStringHelper = _container?.TryGetInstance<IShortStringHelper>()
                ?? new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(UmbracoConfig.For.UmbracoSettings())));

        public static ILogger Logger
            => _logger ?? (_logger = _container?.TryGetInstance<ILogger>()
                ?? new DebugDiagnosticsLogger());

        public static IProfiler Profiler
            => _profiler ?? (_profiler = _container?.TryGetInstance<IProfiler>()
                ?? new LogProfiler(Logger));

        public static ProfilingLogger ProfilingLogger
            => _profilingLogger ?? (_profilingLogger = _container?.TryGetInstance<ProfilingLogger>())
               ?? new ProfilingLogger(Logger, Profiler);

        public static IRuntimeState RuntimeState
            => Container.GetInstance<IRuntimeState>();

        public static TypeLoader TypeLoader
            => Container.GetInstance<TypeLoader>();

        public static FileSystems FileSystems
            => Container.GetInstance<FileSystems>();

        public static UrlSegmentProviderCollection UrlSegmentProviders
            => Container.GetInstance<UrlSegmentProviderCollection>();

        public static CacheRefresherCollection CacheRefreshers
            => Container.GetInstance<CacheRefresherCollection>();

        public static DataEditorCollection DataEditors
            => Container.GetInstance<DataEditorCollection>();

        public static PropertyEditorCollection PropertyEditors
            => Container.GetInstance<PropertyEditorCollection>();

        public static ParameterEditorCollection ParameterEditors
            => Container.GetInstance<ParameterEditorCollection>();

        internal static ManifestValueValidatorCollection ManifestValidators
            => Container.GetInstance<ManifestValueValidatorCollection>();

        internal static PackageActionCollection PackageActions
            => Container.GetInstance<PackageActionCollection>();

        internal static PropertyValueConverterCollection PropertyValueConverters
            => Container.GetInstance<PropertyValueConverterCollection>();

        internal static IPublishedModelFactory PublishedModelFactory
            => Container.GetInstance<IPublishedModelFactory>();

        public static IServerMessenger ServerMessenger
            => Container.GetInstance<IServerMessenger>();

        public static IServerRegistrar ServerRegistrar
            => Container.GetInstance<IServerRegistrar>();

        public static ICultureDictionaryFactory CultureDictionaryFactory
            => Container.GetInstance<ICultureDictionaryFactory>();

        public static CacheHelper ApplicationCache
            => Container.GetInstance<CacheHelper>();

        public static ServiceContext Services
            => Container.GetInstance<ServiceContext>();

        public static IScopeProvider ScopeProvider
            => Container.GetInstance<IScopeProvider>();

        public static ISqlContext SqlContext
            => Container.GetInstance<ISqlContext>();

        public static IPublishedContentTypeFactory PublishedContentTypeFactory
            => Container.GetInstance<IPublishedContentTypeFactory>();

        public static IPublishedValueFallback PublishedValueFallback
            => _publishedValueFallback ?? Container.GetInstance<IPublishedValueFallback>() ?? new NoopPublishedValueFallback();

        public static IVariationContextAccessor VariationContextAccessor
            => Container.GetInstance<IVariationContextAccessor>();

        #endregion
    }
}

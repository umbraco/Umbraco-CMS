using System;
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
using Umbraco.Core.Hosting;
using Umbraco.Core.Mapping;
using Umbraco.Core.PackageActions;
using Umbraco.Core.Packaging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;
using Umbraco.Net;
using Umbraco.Web.Actions;
using Umbraco.Web.Cache;
using Umbraco.Web.Editors;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Services;
using Umbraco.Web.WebApi;

using CoreCurrent = Umbraco.Core.Composing.Current;

namespace Umbraco.Web.Composing
{
    // see notes in Umbraco.Core.Composing.Current.
    public static class Current
    {
        private static readonly object Locker = new object();

        private static IUmbracoContextAccessor _umbracoContextAccessor;

        static Current()
        {
            CoreCurrent.Resetted += (sender, args) =>
            {
                if (_umbracoContextAccessor != null)
                {
                    var umbracoContext = _umbracoContextAccessor.UmbracoContext;
                    umbracoContext?.Dispose();
                }
                _umbracoContextAccessor = null;
            };
        }

        // for UNIT TESTS exclusively!
        internal static void Reset()
        {
            CoreCurrent.Reset();
        }

        /// <summary>
        /// Gets the factory.
        /// </summary>
        public static IFactory Factory
            => CoreCurrent.Factory;

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

        public static UmbracoContext UmbracoContext
            => UmbracoContextAccessor.UmbracoContext;

        public static UmbracoHelper UmbracoHelper
            => Factory.GetInstance<UmbracoHelper>();

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

        public static FilteredControllerFactoryCollection FilteredControllerFactories
            => Factory.GetInstance<FilteredControllerFactoryCollection>();

        internal static IPublishedSnapshotService PublishedSnapshotService
            => Factory.GetInstance<IPublishedSnapshotService>();

        public static ITreeService TreeService
            => Factory.GetInstance<ITreeService>();

        public static ISectionService SectionService
            => Factory.GetInstance<ISectionService>();

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
                    throw new ArgumentException($"Type {value.FullName} does not implement {typeof (IRenderController).FullName}.", nameof(value));
                _defaultRenderMvcControllerType = value;
            }
        }

        #endregion

        #region Core Getters

        // proxy Core for convenience

        public static IMediaFileSystem MediaFileSystem => Factory.GetInstance<IMediaFileSystem>();

        public static UmbracoMapper Mapper =>  Factory.GetInstance<UmbracoMapper>();

        public static IRuntimeState RuntimeState => Factory.GetInstance<IRuntimeState>();

        public static TypeLoader TypeLoader => Factory.GetInstance<TypeLoader>();

        public static Configs Configs => Factory.GetInstance<Configs>();

        public static UrlSegmentProviderCollection UrlSegmentProviders => Factory.GetInstance<UrlSegmentProviderCollection>();

        public static CacheRefresherCollection CacheRefreshers => Factory.GetInstance<CacheRefresherCollection>();

        public static DataEditorCollection DataEditors =>  Factory.GetInstance<DataEditorCollection>();

        public static DataValueReferenceFactoryCollection DataValueReferenceFactories => Factory.GetInstance<DataValueReferenceFactoryCollection>();

        public static PropertyEditorCollection PropertyEditors => Factory.GetInstance<PropertyEditorCollection>();

        public static ParameterEditorCollection ParameterEditors => Factory.GetInstance<ParameterEditorCollection>();

        internal static ManifestValueValidatorCollection ManifestValidators => Factory.GetInstance<ManifestValueValidatorCollection>();

        internal static IPackageActionRunner PackageActionRunner => Factory.GetInstance<IPackageActionRunner>();

        internal static PackageActionCollection PackageActions => Factory.GetInstance<PackageActionCollection>();

        internal static PropertyValueConverterCollection PropertyValueConverters => Factory.GetInstance<PropertyValueConverterCollection>();

        internal static IPublishedModelFactory PublishedModelFactory => Factory.GetInstance<IPublishedModelFactory>();

        public static IServerMessenger ServerMessenger => Factory.GetInstance<IServerMessenger>();

        public static IServerRegistrar ServerRegistrar => Factory.GetInstance<IServerRegistrar>();

        public static ICultureDictionaryFactory CultureDictionaryFactory => Factory.GetInstance<ICultureDictionaryFactory>();

        public static IShortStringHelper ShortStringHelper => Factory.GetInstance<IShortStringHelper>();

        public static ILogger Logger => Umbraco.Composing.Current.Logger;

        public static IProfiler Profiler => Factory.GetInstance<IProfiler>();

        public static IProfilingLogger ProfilingLogger => Factory.GetInstance<IProfilingLogger>();

        public static AppCaches AppCaches => Factory.GetInstance<AppCaches>();

        public static ServiceContext Services => Factory.GetInstance<ServiceContext>();

        public static IScopeProvider ScopeProvider => Factory.GetInstance<IScopeProvider>();

        public static IFileSystems FileSystems => Factory.GetInstance<IFileSystems>();

        public static ISqlContext SqlContext=> Factory.GetInstance<ISqlContext>();

        public static IPublishedContentTypeFactory PublishedContentTypeFactory => Factory.GetInstance<IPublishedContentTypeFactory>();

        public static IPublishedValueFallback PublishedValueFallback => Factory.GetInstance<IPublishedValueFallback>();

        public static IVariationContextAccessor VariationContextAccessor => Factory.GetInstance<IVariationContextAccessor>();

        public static IIOHelper IOHelper => Factory.GetInstance<IIOHelper>();
        public static IHostingEnvironment HostingEnvironment => Factory.GetInstance<IHostingEnvironment>();
        public static IIpResolver IpResolver => Factory.GetInstance<IIpResolver>();
        public static IUmbracoVersion UmbracoVersion => Factory.GetInstance<IUmbracoVersion>();

        #endregion
    }
}

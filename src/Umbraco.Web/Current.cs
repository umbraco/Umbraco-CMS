using System;
using LightInject;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.Macros;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Strings;
using Umbraco.Core._Legacy.PackageActions;
using Umbraco.Web.Editors;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.WebApi;
using Umbraco.Web._Legacy.Actions;
using CoreCurrent = Umbraco.Core.DependencyInjection.Current;

namespace Umbraco.Web
{
    // must remain internal - this class is here to support the transition from singletons
    // and resolvers to injection - by providing a static access to singleton services - it
    // is initialized once with a service container, in WebBootManager.
    public static class Current
    {
        private static readonly object Locker = new object();

        private static IUmbracoContextAccessor _umbracoContextAccessor;
        private static IFacadeAccessor _facadeAccessor;

        static Current()
        {
            CoreCurrent.Resetted += (sender, args) =>
            {
                if (_umbracoContextAccessor != null)
                    ClearUmbracoContext();
                _umbracoContextAccessor = null;
                _facadeAccessor = null;
            };
        }

        // for UNIT TESTS exclusively!
        internal static void Reset()
        {
            CoreCurrent.Reset();
        }

        public static ServiceContainer Container
            => CoreCurrent.Container;

        // Facade
        //
        // is managed by the FacadeAccessor
        //
        // have to support setting the accessor directly (vs container) for tests
        // fixme - not sure about this - should tests use a container?

        public static IFacadeAccessor FacadeAccessor
        {
            get
            {
                if (_facadeAccessor != null) return _facadeAccessor;
                return (_facadeAccessor = Container.GetInstance<IFacadeAccessor>());
            }
            set { _facadeAccessor = value; } // for tests
        }

        // UmbracoContext
        //
        // is managed by the UmbracoContext Acceesor
        //
        // have to support setting the accessor directly (vs container) for tests
        // fixme - note sure about this - should tests use a container?
        //
        // have to support setting it for now, because of 'ensure umbraco context' which can create
        // contexts pretty much at any time and in an uncontrolled way - and when we do not have
        // proper access to the accessor.
        //
        // have to support clear, because of the weird mixed accessor we're using that can
        // store things in thread-static var that need to be cleared, else it retains rogue values.

        public static IUmbracoContextAccessor UmbracoContextAccessor
        {
            get
            {
                if (_umbracoContextAccessor != null) return _umbracoContextAccessor;
                return (_umbracoContextAccessor = Container.GetInstance<IUmbracoContextAccessor>());
            }
            set { _umbracoContextAccessor = value; } // for tests
        }

        public static UmbracoContext UmbracoContext
            => UmbracoContextAccessor.UmbracoContext;

        public static void SetUmbracoContext(UmbracoContext value, bool canReplace)
        {
            lock (Locker)
            {
                if (UmbracoContextAccessor.UmbracoContext != null && canReplace == false)
                    throw new InvalidOperationException("Current UmbracoContext can be set only once per request.");
                UmbracoContextAccessor.UmbracoContext?.Dispose(); // dispose the one that is being replaced, if any
                UmbracoContextAccessor.UmbracoContext = value;
            }
        }

        public static void ClearUmbracoContext()
        {
            lock (Locker)
            {
                UmbracoContextAccessor.UmbracoContext?.Dispose(); // dispose the one that is being cleared, if any
                UmbracoContextAccessor.UmbracoContext = null;
            }
        }

        #region Web Getters

        public static IFacade Facade
            => FacadeAccessor.Facade;

        public static EventMessages EventMessages
            => Container.GetInstance<IEventMessagesFactory>().GetOrDefault();

        public static UrlProviderCollection UrlProviders
            => Container.GetInstance<UrlProviderCollection>();

        public static HealthCheckCollectionBuilder HealthCheckCollectionBuilder
            => Container.GetInstance<HealthCheckCollectionBuilder>();

        public static ActionCollectionBuilder ActionCollectionBuilder
            => Container.GetInstance<ActionCollectionBuilder>();

        public static ActionCollection Actions
            => Container.GetInstance<ActionCollection>();

        public static MigrationCollectionBuilder MigrationCollectionBuilder
            => Container.GetInstance<MigrationCollectionBuilder>();

        public static ContentFinderCollection ContentFinders
            => Container.GetInstance<ContentFinderCollection>();

        public static IContentLastChanceFinder LastChanceContentFinder
            => Container.GetInstance<IContentLastChanceFinder>();

        internal static EditorValidatorCollection EditorValidators
            => Container.GetInstance<EditorValidatorCollection>();

        internal static XsltExtensionCollection XsltExtensions
            => Container.GetInstance<XsltExtensionCollection>();

        internal static UmbracoApiControllerTypeCollection UmbracoApiControllerTypes
            => Container.GetInstance<UmbracoApiControllerTypeCollection>();

        internal static SurfaceControllerTypeCollection SurfaceControllerTypes
            => Container.GetInstance<SurfaceControllerTypeCollection>();

        public static FilteredControllerFactoryCollection FilteredControllerFactories
            => Container.GetInstance<FilteredControllerFactoryCollection>();

        #endregion

        #region Core Getters

        // proxy Core for convenience

        public static UrlSegmentProviderCollection UrlSegmentProviders
            => Container.GetInstance<UrlSegmentProviderCollection>();

        public static CacheRefresherCollection CacheRefreshers
            => Container.GetInstance<CacheRefresherCollection>();

        public static PropertyEditorCollection PropertyEditors
            => Container.GetInstance<PropertyEditorCollection>();

        public static ParameterEditorCollection ParameterEditors
            => Container.GetInstance<ParameterEditorCollection>();

        internal static ValidatorCollection Validators
            => Container.GetInstance<ValidatorCollection>();

        internal static PackageActionCollection PackageActions
            => Container.GetInstance<PackageActionCollection>();

        internal static PropertyValueConverterCollection PropertyValueConverters
            => Container.GetInstance<PropertyValueConverterCollection>();

        #endregion
    }
}

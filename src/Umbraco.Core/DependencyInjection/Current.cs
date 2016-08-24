using System;
using LightInject;
using Umbraco.Core.Cache;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;
using Umbraco.Core._Legacy.PackageActions;

namespace Umbraco.Core.DependencyInjection
{
    // this class is here to support the transition from singletons and resolvers to injection,
    // by providing a static access to singleton services - it is initialized once with a service
    // container, in CoreBootManager.
    // ideally, it should not exist. practically, time will tell.
    public static class Current
    {
        private static ServiceContainer _container;

        public static ServiceContainer Container
        {
            get
            {
                if (_container == null) throw new Exception("No container has been set.");
                return _container;
            }
            internal set // ok to set - don't be stupid
            {
                if (_container != null) throw new Exception("A container has already been set.");
                _container = value;
            }
        }

        internal static bool HasContainer => _container != null;

        // for UNIT TESTS exclusively!
        internal static void Reset()
        {
            _container = null;
            Resetted?.Invoke(null, EventArgs.Empty);
        }

        internal static event EventHandler Resetted;

        #region Getters

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

        internal static IPublishedContentModelFactory PublishedContentModelFactory
            => Container.GetInstance<IPublishedContentModelFactory>();

        public static IServerMessenger ServerMessenger
            => Container.GetInstance<IServerMessenger>();

        public static IServerRegistrar ServerRegistrar
            => Container.GetInstance<IServerRegistrar>();

        public static ICultureDictionaryFactory CultureDictionaryFactory
            => Container.GetInstance<ICultureDictionaryFactory>();

        #endregion
    }
}

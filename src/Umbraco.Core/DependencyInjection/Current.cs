using System;
using LightInject;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
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
            _shortStringHelper = null;
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

        private static IShortStringHelper _shortStringHelper;

        public static IShortStringHelper ShortStringHelper
        {
            get
            {
                // fixme - refactor
                // we don't want Umbraco to die because the resolver hasn't been initialized
                // as the ShortStringHelper is too important, so as long as it's not there
                // already, we use a default one. That should never happen, but... in can, in
                // some tests - we should really cleanup our tests and get rid of this!

                if (_shortStringHelper != null) return _shortStringHelper;
                var reg = HasContainer ? Container.GetAvailableService<IShortStringHelper>() : null;
                return _shortStringHelper = reg == null
                    ? new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(UmbracoConfig.For.UmbracoSettings()))
                    : Container.GetInstance<IShortStringHelper>();
            }
        }

        #endregion
    }
}

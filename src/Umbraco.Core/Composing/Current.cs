using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PackageActions;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;

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

        // TODO: get rid of these oddities
        // we don't want Umbraco tests to die because the container has not been properly initialized,
        // for some too-important things such as IShortStringHelper or loggers, so if it's not
        // registered we setup a default one. We should really refactor our tests so that it does
        // not happen.

        private static ILogger _logger;
        private static Configs _configs;

        /// <summary>
        /// Gets or sets the factory.
        /// </summary>
        public static IFactory Factory
        {
            get
            {
                if (_factory == null) throw new InvalidOperationException("No factory has been set.");
                return _factory;
            }
            set
            {
                if (_factory != null) throw new InvalidOperationException("A factory has already been set.");
                if (_configs != null) throw new InvalidOperationException("Configs are unlocked.");
                _factory = value;
            }
        }

        public static bool HasFactory => _factory != null;

        /// <summary>
        /// Resets <see cref="Current"/>. Indented for testing only, and not supported in production code.
        /// </summary>
        /// <remarks>
        /// <para>For UNIT TESTS exclusively.</para>
        /// <para>Resets everything that is 'current'.</para>
        /// </remarks>
        public static void Reset()
        {
            _factory.DisposeIfDisposable();
            _factory = null;

            _configs = null;
            _logger = null;

            Resetted?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Unlocks <see cref="Configs"/>. Intended for testing only, and not supported in production code.
        /// </summary>
        /// <remarks>
        /// <para>For UNIT TESTS exclusively.</para>
        /// <para>Unlocks <see cref="Configs"/> so that it is possible to add configurations
        /// directly to <see cref="Current"/> without having to wire composition.</para>
        /// </remarks>
        public static void UnlockConfigs(IConfigsFactory configsFactory, IIOHelper ioHelper)
        {
            if (_factory != null)
                throw new InvalidOperationException("Cannot unlock configs when a factory has been set.");
            _configs = configsFactory.Create(ioHelper);
        }

        internal static event EventHandler Resetted;

        #region Getters


        public static IShortStringHelper ShortStringHelper => Factory.GetInstance<IShortStringHelper>();


        public static ILogger Logger
            => _logger ?? (_logger = _factory?.TryGetInstance<ILogger>()
                                     ?? new DebugDiagnosticsLogger(new MessageTemplates()));

        public static Configs Configs
        {
            get
            {
                if (_configs != null) return _configs;
                if (_factory == null) throw new InvalidOperationException("Can not get Current.Config during composition. Use composition.Config.");
                return _factory.GetInstance<Configs>();
            }
        }

        internal static PackageActionCollection PackageActions
            => Factory.GetInstance<PackageActionCollection>();

        internal static IPublishedModelFactory PublishedModelFactory
            => Factory.GetInstance<IPublishedModelFactory>();

        public static IServerMessenger ServerMessenger
            => Factory.GetInstance<IServerMessenger>();

        public static ServiceContext Services
            => Factory.GetInstance<ServiceContext>();

        public static IIOHelper IOHelper
            => Factory.GetInstance<IIOHelper>();


        #endregion
    }
}

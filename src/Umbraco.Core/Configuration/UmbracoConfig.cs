using System;
using System.Configuration;
using System.IO;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.Dashboard;
using Umbraco.Core.Configuration.Grid;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// The gateway to all umbraco configuration.
    /// </summary>
    /// <remarks>This should be registered as a unique service in the container.</remarks>
    public class UmbracoConfig
    {
        private IGlobalSettings _global;
        private Lazy<IUmbracoSettingsSection> _umbraco;
        private Lazy<IHealthChecks> _healthChecks;
        private Lazy<IDashboardSection> _dashboards;
        private Lazy<IGridConfig> _grids;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoConfig"/> class.
        /// </summary>
        public UmbracoConfig(ILogger logger, IRuntimeCacheProvider runtimeCache, IRuntimeState runtimeState)
        {
            _global = new GlobalSettings();

            var appPluginsDir = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.AppPlugins));
            var configDir = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Config));

            _umbraco = new Lazy<IUmbracoSettingsSection>(() => GetConfig<IUmbracoSettingsSection>("umbracoConfiguration/settings"));
            _dashboards = new Lazy<IDashboardSection>(() =>GetConfig<IDashboardSection>("umbracoConfiguration/dashBoard"));
            _healthChecks = new Lazy<IHealthChecks>(() => GetConfig<IHealthChecks>("umbracoConfiguration/HealthChecks"));
            _grids = new Lazy<IGridConfig>(() => new GridConfig(logger, runtimeCache, appPluginsDir, configDir, runtimeState.Debug));
        }

        /// <summary>
        /// Gets a typed and named config section.
        /// </summary>
        /// <typeparam name="TConfig">The type of the configuration section.</typeparam>
        /// <param name="sectionName">The name of the configuration section.</param>
        /// <returns>The configuration section.</returns>
        public static TConfig GetConfig<TConfig>(string sectionName)
            where TConfig : class
        {
            // note: need to use SafeCallContext here because ConfigurationManager.GetSection ends up getting AppDomain.Evidence
            // which will want to serialize the call context including anything that is in there - what a mess!

            using (new SafeCallContext())
            {
                if ((ConfigurationManager.GetSection(sectionName) is TConfig config))
                    return config;
                var ex = new ConfigurationErrorsException($"Could not get configuration section \"{sectionName}\" from config files.");
                Current.Logger.Error<UmbracoConfig>(ex, "Config error");
                throw ex;
            }
        }

        /// <summary>
        /// Gets the global configuration.
        /// </summary>
        public IGlobalSettings Global()
            => _global;

        /// <summary>
        /// Gets the Umbraco configuration.
        /// </summary>
        public IUmbracoSettingsSection Umbraco()
            => _umbraco.Value;

        /// <summary>
        /// Gets the dashboards configuration.
        /// </summary>
        public IDashboardSection Dashboards()
            => _dashboards.Value;

        /// <summary>
        /// Gets the health checks configuration.
        /// </summary>
        public IHealthChecks HealthChecks()
            => _healthChecks.Value;

        /// <summary>
        /// Gets the grids configuration.
        /// </summary>
        public IGridConfig Grids()
            => _grids.Value;

        /// <summary>
        /// Sets the global configuration, for tests only.
        /// </summary>
        public void SetGlobalConfig(IGlobalSettings value)
        {
            _global = value;
        }

        /// <summary>
        /// Sets the Umbraco configuration, for tests only.
        /// </summary>
        public void SetUmbracoConfig(IUmbracoSettingsSection value)
        {
            _umbraco = new Lazy<IUmbracoSettingsSection>(() => value);
        }

        /// <summary>
        /// Sets the dashboards configuration, for tests only.
        /// </summary>
        /// <param name="value"></param>
        public void SetDashboardsConfig(IDashboardSection value)
        {
            _dashboards = new Lazy<IDashboardSection>(() => value);
        }

        /// <summary>
        /// Sets the health checks configuration, for tests only.
        /// </summary>
        public void SetHealthChecksConfig(IHealthChecks value)
        {
            _healthChecks = new Lazy<IHealthChecks>(() => value);
        }

        /// <summary>
        /// Sets the grids configuration, for tests only.
        /// </summary>
        public void SetGridsConfig(IGridConfig value)
        {
            _grids = new Lazy<IGridConfig>(() => value);
        }

        //TODO: Add other configurations here !
    }
}

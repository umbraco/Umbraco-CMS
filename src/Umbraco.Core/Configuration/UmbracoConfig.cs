using System;
using System.Configuration;
using System.IO;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.Dashboard;
using Umbraco.Core.Configuration.Grid;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// The gateway to all umbraco configuration
    /// </summary>
    public class UmbracoConfig
    {
        #region Singleton

        private static readonly Lazy<UmbracoConfig> Lazy = new Lazy<UmbracoConfig>(() => new UmbracoConfig());

        public static UmbracoConfig For => Lazy.Value;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        private UmbracoConfig()
        {
            // note: need to use SafeCallContext here because ConfigurationManager.GetSection ends up getting AppDomain.Evidence
            // which will want to serialize the call context including anything that is in there - what a mess!

            if (_umbracoSettings == null)
            {
                IUmbracoSettingsSection umbracoSettings;
                using (new SafeCallContext())
                {
                    umbracoSettings = ConfigurationManager.GetSection("umbracoConfiguration/settings") as IUmbracoSettingsSection;
                }
                SetUmbracoSettings(umbracoSettings);
            }

            if (_dashboardSection == null)
            {
                IDashboardSection dashboardConfig;
                using (new SafeCallContext())
                {
                    dashboardConfig = ConfigurationManager.GetSection("umbracoConfiguration/dashBoard") as IDashboardSection;
                }
                SetDashboardSettings(dashboardConfig);
            }

            if (_healthChecks == null)
            {
                var healthCheckConfig = ConfigurationManager.GetSection("umbracoConfiguration/HealthChecks") as IHealthChecks;
                SetHealthCheckSettings(healthCheckConfig);
            }
        }

        /// <summary>
        /// Constructor - can be used for testing
        /// </summary>
        /// <param name="umbracoSettings"></param>
        /// <param name="dashboardSettings"></param>
        /// <param name="healthChecks"></param>
        /// <param name="globalSettings"></param>
        public UmbracoConfig(IUmbracoSettingsSection umbracoSettings, IDashboardSection dashboardSettings, IHealthChecks healthChecks, IGlobalSettings globalSettings)
        {
            SetHealthCheckSettings(healthChecks);
            SetUmbracoSettings(umbracoSettings);
            SetDashboardSettings(dashboardSettings);
            SetGlobalConfig(globalSettings);
        }

        private IHealthChecks _healthChecks;
        private IDashboardSection _dashboardSection;
        private IUmbracoSettingsSection _umbracoSettings;
        private IGridConfig _gridConfig;
        private IGlobalSettings _globalSettings;

        /// <summary>
        /// Gets the IHealthCheck config
        /// </summary>
        public IHealthChecks HealthCheck()
        {
            if (_healthChecks == null)
            {
                var ex = new ConfigurationErrorsException("Could not load the " + typeof(IHealthChecks) + " from config file, ensure the web.config and healthchecks.config files are formatted correctly");
                Current.Logger.Error<UmbracoConfig>(ex, "Config error");
                throw ex;
            }

            return _healthChecks;
        }

        /// <summary>
        /// Gets the IDashboardSection
        /// </summary>
        public IDashboardSection DashboardSettings()
        {
            if (_dashboardSection == null)
            {
                var ex = new ConfigurationErrorsException("Could not load the " + typeof(IDashboardSection) + " from config file, ensure the web.config and Dashboard.config files are formatted correctly");
                Current.Logger.Error<UmbracoConfig>(ex, "Config error");
                throw ex;
            }

            return _dashboardSection;
        }

        /// <summary>
        /// Only for testing
        /// </summary>
        /// <param name="value"></param>
        public void SetDashboardSettings(IDashboardSection value)
        {
            _dashboardSection = value;
        }

        /// <summary>
        /// Only for testing
        /// </summary>
        /// <param name="value"></param>
        public void SetHealthCheckSettings(IHealthChecks value)
        {
            _healthChecks = value;
        }

        /// <summary>
        /// Only for testing
        /// </summary>
        /// <param name="value"></param>
        public void SetUmbracoSettings(IUmbracoSettingsSection value)
        {
            _umbracoSettings = value;
        }

        /// <summary>
        /// Only for testing
        /// </summary>
        /// <param name="value"></param>
        public void SetGlobalConfig(IGlobalSettings value)
        {
            _globalSettings = value;
        }

        /// <summary>
        /// Gets the IGlobalSettings
        /// </summary>
        public IGlobalSettings GlobalSettings()
        {
            return _globalSettings ?? (_globalSettings = new GlobalSettings());
        }

        /// <summary>
        /// Gets the IUmbracoSettings
        /// </summary>
        public IUmbracoSettingsSection UmbracoSettings()
        {
            if (_umbracoSettings == null)
            {
                var ex = new ConfigurationErrorsException("Could not load the " + typeof (IUmbracoSettingsSection) + " from config file, ensure the web.config and umbracoSettings.config files are formatted correctly");
                Current.Logger.Error<UmbracoConfig>(ex, "Config error");
                throw ex;
            }

            return _umbracoSettings;
        }

        /// <summary>
        /// Only for testing
        /// </summary>
        /// <param name="value"></param>
        public void SetGridConfig(IGridConfig value)
        {
            _gridConfig = value;
        }

        /// <summary>
        /// Gets the IGridConfig
        /// </summary>
        public IGridConfig GridConfig(ILogger logger, IRuntimeCacheProvider runtimeCache, DirectoryInfo appPlugins, DirectoryInfo configFolder, bool isDebug)
        {
            if (_gridConfig == null)
            {
                _gridConfig = new GridConfig(logger, runtimeCache, appPlugins, configFolder, isDebug);
            }

            return _gridConfig;
        }

        //TODO: Add other configurations here !
    }
}
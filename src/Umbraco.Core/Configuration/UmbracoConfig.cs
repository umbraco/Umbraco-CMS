﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.BaseRest;
using Umbraco.Core.Configuration.Dashboard;
using Umbraco.Core.Configuration.Grid;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Configuration.UmbracoSettings;
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

        public static UmbracoConfig For
        {
            get { return Lazy.Value; }            
        }

        #endregion

        /// <summary>
        /// Default constructor 
        /// </summary>
        private UmbracoConfig()
        {
            if (_umbracoSettings == null)
            {
                var umbracoSettings = ConfigurationManager.GetSection("umbracoConfiguration/settings") as IUmbracoSettingsSection;                
                SetUmbracoSettings(umbracoSettings);
            }

            if (_baseRestExtensions == null)
            {
                var baseRestExtensions = ConfigurationManager.GetSection("umbracoConfiguration/BaseRestExtensions") as IBaseRestSection;                
                SetBaseRestExtensions(baseRestExtensions);
            }

            if (_dashboardSection == null)
            {
                var dashboardConfig = ConfigurationManager.GetSection("umbracoConfiguration/dashBoard") as IDashboardSection;                
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
        /// <param name="baseRestSettings"></param>
        /// <param name="dashboardSettings"></param>
        /// <param name="healthChecks"></param>
        public UmbracoConfig(IUmbracoSettingsSection umbracoSettings, IBaseRestSection baseRestSettings, IDashboardSection dashboardSettings, IHealthChecks healthChecks)
        {
            SetHealthCheckSettings(healthChecks);
            SetUmbracoSettings(umbracoSettings);
            SetBaseRestExtensions(baseRestSettings);
            SetDashboardSettings(dashboardSettings);
        }

        private IHealthChecks _healthChecks;
        private IDashboardSection _dashboardSection;
        private IUmbracoSettingsSection _umbracoSettings;
        private IBaseRestSection _baseRestExtensions;
        private IGridConfig _gridConfig;

        /// <summary>
        /// Gets the IHealthCheck config
        /// </summary>
        public IHealthChecks HealthCheck()
        {
            if (_healthChecks == null)
            {
                var ex = new ConfigurationErrorsException("Could not load the " + typeof(IHealthChecks) + " from config file, ensure the web.config and healthchecks.config files are formatted correctly");
                LogHelper.Error<UmbracoConfig>("Config error", ex);
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
                LogHelper.Error<UmbracoConfig>("Config error", ex);
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
        /// Gets the IUmbracoSettings
        /// </summary>
        public IUmbracoSettingsSection UmbracoSettings()
        {
            if (_umbracoSettings == null)
            {
                var ex = new ConfigurationErrorsException("Could not load the " + typeof (IUmbracoSettingsSection) + " from config file, ensure the web.config and umbracoSettings.config files are formatted correctly");
                LogHelper.Error<UmbracoConfig>("Config error", ex);
                throw ex;
            }

            return _umbracoSettings;
        }
        
        /// <summary>
        /// Only for testing
        /// </summary>
        /// <param name="value"></param>
        public void SetBaseRestExtensions(IBaseRestSection value)
        {
            _baseRestExtensions = value;
        }

        /// <summary>
        /// Gets the IBaseRestSection
        /// </summary>
        public IBaseRestSection BaseRestExtensions()
        {
            if (_baseRestExtensions == null)
            {
                var ex = new ConfigurationErrorsException("Could not load the " + typeof(IBaseRestSection) + " from config file, ensure the web.config and BaseRestExtensions.config files are formatted correctly");
                LogHelper.Error<UmbracoConfig>("Config error", ex);
                throw ex;
            }

            return _baseRestExtensions;
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
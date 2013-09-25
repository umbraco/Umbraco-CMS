using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using Umbraco.Core.Configuration.BaseRest;
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
            if (UmbracoSettings() == null)
            {
                var umbracoSettings = ConfigurationManager.GetSection("umbracoConfiguration/settings") as IUmbracoSettingsSection;
                if (umbracoSettings == null)
                {
                    LogHelper.Warn<UmbracoConfig>("Could not load the " + typeof(IUmbracoSettingsSection) + " from config file!");
                }
                SetUmbracoSettings(umbracoSettings);
            }

            if (BaseRestExtensions() == null)
            {
                var baseRestExtensions = ConfigurationManager.GetSection("umbracoConfiguration/BaseRestExtensions") as IBaseRestSection;
                if (baseRestExtensions == null)
                {
                    LogHelper.Warn<UmbracoConfig>("Could not load the " + typeof(IBaseRestSection) + " from config file!");
                }
                SetBaseRestExtensions(baseRestExtensions);
            }
        }

        /// <summary>
        /// Constructor - can be used for testing
        /// </summary>
        /// <param name="umbracoSettings"></param>
        /// <param name="baseRestSettings"></param>
        public UmbracoConfig(IUmbracoSettingsSection umbracoSettings, IBaseRestSection baseRestSettings)
        {
            SetUmbracoSettings(umbracoSettings);
            SetBaseRestExtensions(baseRestSettings);
        }

        private IUmbracoSettingsSection _umbracoSettings;

        //ONLY for unit testing
        internal void SetUmbracoSettings(IUmbracoSettingsSection value)
        {
            _umbracoSettings = value;
        }

        /// <summary>
        /// Gets the IUmbracoSettings
        /// </summary>
        public IUmbracoSettingsSection UmbracoSettings()
        {
            return _umbracoSettings;
        }

        private IBaseRestSection _baseRestExtensions;

        //ONLY for unit testing
        public void SetBaseRestExtensions(IBaseRestSection value)
        {
            _baseRestExtensions = value;
        }

        /// <summary>
        /// Gets the IBaseRestSection
        /// </summary>
        public IBaseRestSection BaseRestExtensions()
        {
            return _baseRestExtensions;
        }

        //TODO: Add other configurations here !
    }
}
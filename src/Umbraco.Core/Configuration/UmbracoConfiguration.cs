using System;
using System.Configuration;
using Umbraco.Core.Configuration.BaseRest;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// The gateway to all umbraco configuration
    /// </summary>
    public class UmbracoConfiguration
    {
        #region Singleton

        private static readonly Lazy<UmbracoConfiguration> Lazy = new Lazy<UmbracoConfiguration>(() => new UmbracoConfiguration());

        public static UmbracoConfiguration Current { get { return Lazy.Value; } }

        #endregion

        /// <summary>
        /// Default constructor 
        /// </summary>
        private UmbracoConfiguration()
        {
            if (UmbracoSettings == null)
            {
                var umbracoSettings = ConfigurationManager.GetSection("umbracoConfiguration/settings") as IUmbracoSettings;
                if (umbracoSettings == null)
                {
                    throw new InvalidOperationException("Could not find configuration section 'umbracoConfiguration/settings' or it does not cast to " + typeof(IUmbracoSettings));
                }
                UmbracoSettings = umbracoSettings;
            }
        }

        /// <summary>
        /// Constructor - can be used for testing
        /// </summary>
        /// <param name="umbracoSettings"></param>
        /// <param name="baseRestSettings"></param>
        public UmbracoConfiguration(IUmbracoSettings umbracoSettings, IBaseRest baseRestSettings)
        {
            UmbracoSettings = umbracoSettings;
            BaseRestExtensions = baseRestSettings;
        }
        
        public IUmbracoSettings UmbracoSettings { get; private set; }

        public IBaseRest BaseRestExtensions { get; private set; }

        //TODO: Add other configurations here !
    }
}
using System;
using System.Configuration;
using System.Diagnostics;


namespace Umbraco.Core
{	

	/// <summary>
    /// the Umbraco Application context
    /// </summary>
    /// <remarks>
    /// one per AppDomain, represents the global Umbraco application
    /// </remarks>
    public class ApplicationContext
    {
    	/// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pluginResolver"></param>
        public ApplicationContext(PluginTypeResolver pluginResolver)
        {
            PluginTypes = pluginResolver;
        }

    	/// <summary>
    	/// Singleton accessor
    	/// </summary>
    	public static ApplicationContext Current { get; internal set; }

    	// IsReady is set to true by the boot manager once it has successfully booted
        // note - the original umbraco module checks on content.Instance in umbraco.dll
        //   now, the boot task that setup the content store ensures that it is ready
        bool _isReady = false;
        public bool IsReady
        {
            get
            {
                return _isReady;
            }
            internal set
            {
                AssertIsNotReady();
                _isReady = value;
            }
        }

        /// <summary>
        /// Gets the plugin resolver for the application
        /// </summary>
        public PluginTypeResolver PluginTypes { get; private set; }

        // notes
        //   GlobalSettings.ConfigurationStatus returns the value that's in the web.config, so it's the "configured version"
        //   GlobalSettings.CurrentVersion returns the hard-coded "current version"
        //   the system is configured if they match
        //   if they don't, install runs, updates web.config (presumably) and updates GlobalSettings.ConfiguredStatus
        //
        //   then there is Application["umbracoNeedConfiguration"] which makes no sense... getting rid of it...
        //
        public bool IsConfigured
        {
            // fixme - let's do this for the time being
            get
            {
            	return Configured;
            }
        }

		//TODO: This is temporary as we cannot reference umbraco.businesslogic as this will give a circular reference
		// CURRENT UMBRACO VERSION ID
		private const string _currentVersion = "4.8.0";
		private string CurrentVersion
		{
			get
			{
				// change this to be hardcoded in the binary
				return _currentVersion;
			}
		}
		private bool Configured
		{
			get
			{
				try
				{
					string configStatus = ConfigurationStatus;
					string currentVersion = CurrentVersion;


					if (currentVersion != configStatus)
					{
						//Log.Add(LogTypes.Debug, User.GetUser(0), -1,
						//        "CurrentVersion different from configStatus: '" + currentVersion + "','" + configStatus +
						//        "'");
					}
						

					return (configStatus == currentVersion);
				}
				catch
				{
					return false;
				}
			}
		}
		private string ConfigurationStatus
		{
			get
			{
				try
				{
					return ConfigurationManager.AppSettings["umbracoConfigurationStatus"];
				}
				catch
				{
					return String.Empty;
				}
			}			
		}


        private void AssertIsReady()
        {
            if (!this.IsReady)
                throw new Exception("ApplicationContext is not ready yet.");
        }

        private void AssertIsNotReady()
        {
            if (this.IsReady)
                throw new Exception("ApplicationContext has already been initialized.");
        }
    }
}

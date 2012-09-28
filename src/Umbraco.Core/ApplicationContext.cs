using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;


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
        public ApplicationContext()
        {
         
        }

    	/// <summary>
    	/// Singleton accessor
    	/// </summary>
    	public static ApplicationContext Current { get; internal set; }

    	// IsReady is set to true by the boot manager once it has successfully booted
        // note - the original umbraco module checks on content.Instance in umbraco.dll
        //   now, the boot task that setup the content store ensures that it is ready
        bool _isReady = false;
		System.Threading.ManualResetEventSlim _isReadyEvent = new System.Threading.ManualResetEventSlim(false);
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
				_isReadyEvent.Set();
            }
        }

		public bool WaitForReady(int timeout)
		{
			return _isReadyEvent.WaitHandle.WaitOne(timeout);
		}


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

		private bool Configured
		{
			get
			{
				try
				{
					string configStatus = ConfigurationStatus;
					string currentVersion = GlobalSettings.CurrentVersion;


					if (currentVersion != configStatus)
					{
						LogHelper.Info<ApplicationContext>("CurrentVersion different from configStatus: '" + currentVersion + "','" + configStatus + "'");
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

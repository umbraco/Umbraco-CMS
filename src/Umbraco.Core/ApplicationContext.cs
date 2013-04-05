using System;
using System.Configuration;
using System.Web;
using System.Web.Caching;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;


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
        internal ApplicationContext(DatabaseContext dbContext, ServiceContext serviceContext)
			:this()
    	{
    		if (dbContext == null) throw new ArgumentNullException("dbContext");
    		if (serviceContext == null) throw new ArgumentNullException("serviceContext");

			_databaseContext = dbContext;
			_services = serviceContext;			
    	}

		/// <summary>
		/// Empty constructor normally reserved for unit tests when a DatabaseContext or a ServiceContext is not
		/// necessarily required or needs to be set after construction.
		/// </summary>
		internal ApplicationContext()
		{
			//create a new application cache from the HttpRuntime.Cache
			ApplicationCache = HttpRuntime.Cache == null
				? new CacheHelper(new Cache())
				: new CacheHelper(HttpRuntime.Cache);
		}

		/// <summary>
    	/// Singleton accessor
    	/// </summary>
    	public static ApplicationContext Current { get; internal set; }

		/// <summary>
		/// Returns the application wide cache accessor
		/// </summary>
		/// <remarks>
		/// Any caching that is done in the application (app wide) should be done through this property
		/// </remarks>
		public CacheHelper ApplicationCache { get; private set; }

    	// IsReady is set to true by the boot manager once it has successfully booted
        // note - the original umbraco module checks on content.Instance in umbraco.dll
        //   now, the boot task that setup the content store ensures that it is ready
        bool _isReady = false;
		readonly System.Threading.ManualResetEventSlim _isReadyEvent = new System.Threading.ManualResetEventSlim(false);
		private DatabaseContext _databaseContext;
		private ServiceContext _services;

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

        /// <summary>
        /// The original/first url that the web application executes
        /// </summary>
        /// <remarks>
        /// we need to set the initial url in our ApplicationContext, this is so our keep alive service works and this must
        /// exist on a global context because the keep alive service doesn't run in a web context.
        /// we are NOT going to put a lock on this because locking will slow down the application and we don't really care
        /// if two threads write to this at the exact same time during first page hit.
        /// see: http://issues.umbraco.org/issue/U4-2059
        /// </remarks>
        internal string OriginalRequestUrl { get; set; }

		private bool Configured
		{
			get
			{
				try
				{
					string configStatus = ConfigurationStatus;
					string currentVersion = UmbracoVersion.Current.ToString(3);


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

		/// <summary>
		/// Gets the current DatabaseContext
		/// </summary>
		/// <remarks>
		/// Internal set is generally only used for unit tests
		/// </remarks>
		public DatabaseContext DatabaseContext
		{
			get
			{
				if (_databaseContext == null)
					throw new InvalidOperationException("The DatabaseContext has not been set on the ApplicationContext");
				return _databaseContext;
			}
			internal set { _databaseContext = value; }
		}
		
		/// <summary>
		/// Gets the current ServiceContext
		/// </summary>
		/// <remarks>
		/// Internal set is generally only used for unit tests
		/// </remarks>
		public ServiceContext Services
		{
			get
			{
				if (_services == null)
					throw new InvalidOperationException("The ServiceContext has not been set on the ApplicationContext");
				return _services;
			}
			internal set { _services = value; }
		}
    }
}

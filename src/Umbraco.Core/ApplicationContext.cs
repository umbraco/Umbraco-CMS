using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Profiling;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;

namespace Umbraco.Core
{
	/// <summary>
    /// the Umbraco Application context
    /// </summary>
    /// <remarks>
    /// one per AppDomain, represents the global Umbraco application
    /// </remarks>
    public class ApplicationContext : IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="serviceContext"></param>
        /// <param name="cache"></param>
        /// <param name="logger"></param>
        public ApplicationContext(DatabaseContext dbContext, ServiceContext serviceContext, CacheHelper cache, ProfilingLogger logger)
	    {
            if (dbContext == null) throw new ArgumentNullException("dbContext");
            if (serviceContext == null) throw new ArgumentNullException("serviceContext");
            if (cache == null) throw new ArgumentNullException("cache");
            if (logger == null) throw new ArgumentNullException("logger");
            _databaseContext = dbContext;
            _services = serviceContext;
            ApplicationCache = cache;
            ProfilingLogger = logger;

            Init();
	    }

	    /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="serviceContext"></param>
        /// <param name="cache"></param>
        [Obsolete("Use the other constructor specifying a ProfilingLogger instead")]
        public ApplicationContext(DatabaseContext dbContext, ServiceContext serviceContext, CacheHelper cache)
            : this(dbContext, serviceContext, cache, 
                new ProfilingLogger(LoggerResolver.Current.Logger, ProfilerResolver.Current.Profiler))
        {
        }

        /// <summary>
        /// Creates a basic app context
        /// </summary>
        /// <param name="cache"></param>
        [Obsolete("Use the other constructor specifying a ProfilingLogger instead")]
        public ApplicationContext(CacheHelper cache)
        {
            if (cache == null) throw new ArgumentNullException("cache");
            ApplicationCache = cache;
            ProfilingLogger = new ProfilingLogger(LoggerResolver.Current.Logger, ProfilerResolver.Current.Profiler);
            Init();
        }

	    /// <summary>
	    /// Creates a basic app context
	    /// </summary>
	    /// <param name="cache"></param>
	    /// <param name="logger"></param>
	    public ApplicationContext(CacheHelper cache, ProfilingLogger logger)
        {
	        if (cache == null) throw new ArgumentNullException("cache");
	        if (logger == null) throw new ArgumentNullException("logger");
	        ApplicationCache = cache;
	        ProfilingLogger = logger;
            Init();
        }

	    /// <summary>
	    /// A method used to set and/or ensure that a global ApplicationContext singleton is created.
	    /// </summary>
	    /// <param name="appContext">
	    /// The instance to set on the global application singleton
	    /// </param>
	    /// <param name="replaceContext">If set to true and the singleton is already set, it will be replaced</param>
	    /// <returns></returns>
	    /// <remarks>
	    /// This is NOT thread safe 
	    /// </remarks>
	    public static ApplicationContext EnsureContext(ApplicationContext appContext, bool replaceContext)
	    {
            if (Current != null)
            {
                if (!replaceContext)
                    return Current;
            }
            Current = appContext;
            return Current;
	    }

	    /// <summary>
	    /// A method used to create and ensure that a global ApplicationContext singleton is created.
	    /// </summary>
	    /// <param name="cache"></param>
	    /// <param name="replaceContext">
	    /// If set to true will replace the current singleton instance - This should only be used for unit tests or on app 
	    /// startup if for some reason the boot manager is not the umbraco boot manager.
	    /// </param>
	    /// <param name="dbContext"></param>
	    /// <param name="serviceContext"></param>
	    /// <returns></returns>
	    /// <remarks>
	    /// This is NOT thread safe 
	    /// </remarks>
        [Obsolete("Use the other method specifying an ProfilingLogger instead")]
	    public static ApplicationContext EnsureContext(DatabaseContext dbContext, ServiceContext serviceContext, CacheHelper cache, bool replaceContext)
        {
            if (Current != null)
            {
                if (!replaceContext)
                    return Current;
            }
            var ctx = new ApplicationContext(dbContext, serviceContext, cache);
            Current = ctx;
            return Current;
        }

	    /// <summary>
	    /// A method used to create and ensure that a global ApplicationContext singleton is created.
	    /// </summary>
	    /// <param name="cache"></param>
	    /// <param name="logger"></param>
	    /// <param name="replaceContext">
	    /// If set to true will replace the current singleton instance - This should only be used for unit tests or on app 
	    /// startup if for some reason the boot manager is not the umbraco boot manager.
	    /// </param>
	    /// <param name="dbContext"></param>
	    /// <param name="serviceContext"></param>
	    /// <returns></returns>
	    /// <remarks>
	    /// This is NOT thread safe 
	    /// </remarks>
        public static ApplicationContext EnsureContext(DatabaseContext dbContext, ServiceContext serviceContext, CacheHelper cache, ProfilingLogger logger, bool replaceContext)
        {
            if (Current != null)
            {
                if (!replaceContext)
                    return Current;
            }
            var ctx = new ApplicationContext(dbContext, serviceContext, cache, logger);
            Current = ctx;
            return Current;
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

        /// <summary>
        /// Exposes the global ProfilingLogger - this should generally not be accessed via the UmbracoContext and should normally just be exposed 
        /// on most base classes or injected with IoC
        /// </summary>
        public ProfilingLogger ProfilingLogger { get; private set; }

	    // IsReady is set to true by the boot manager once it has successfully booted
        // note - the original umbraco module checks on content.Instance in umbraco.dll
        //   now, the boot task that setup the content store ensures that it is ready
        bool _isReady = false;
		readonly ManualResetEventSlim _isReadyEvent = new ManualResetEventSlim(false);
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
        
        public bool IsConfigured
        {
            get { return _configured.Value; }
        }

        /// <summary>
        /// If the db is configured, there is a database context and there is an umbraco schema, but we are not 'configured' , then it means we are upgrading
        /// </summary>
	    public bool IsUpgrading
	    {
            get
            {
                if (IsConfigured == false 
                    && DatabaseContext != null 
                    && DatabaseContext.IsDatabaseConfigured)
                {
                    var schemaresult = DatabaseContext.ValidateDatabaseSchema();
                    if (schemaresult.ValidTables.Count > 0) return true;
                }

                return false;
            }
	    }

	    /// <summary>
        /// The application url.
        /// </summary>
        /// <remarks>
        /// The application url is the url that should be used by services to talk to the application,
        /// eg keep alive or scheduled publishing services. It must exist on a global context because
        /// some of these services may not run within a web context.
        /// The format of the application url is:
        /// - has a scheme (http or https)
        /// - has the SystemDirectories.Umbraco path
        /// - does not end with a slash
        /// It is initialized on the first request made to the server, by UmbracoModule.EnsureApplicationUrl:
        /// - if umbracoSettings:settings/web.routing/@umbracoApplicationUrl is set, use the value (new setting)
        /// - if umbracoSettings:settings/scheduledTasks/@baseUrl is set, use the value (backward compatibility)
        /// - otherwise, use the url of the (first) request.
        /// Not locking, does not matter if several threads write to this.
        /// See also issues:
        /// - http://issues.umbraco.org/issue/U4-2059
        /// - http://issues.umbraco.org/issue/U4-6788
        /// - http://issues.umbraco.org/issue/U4-5728
        /// - http://issues.umbraco.org/issue/U4-5391
        /// </remarks>
        internal string UmbracoApplicationUrl
        {
            get
            {
                ApplicationUrlHelper.EnsureApplicationUrl(this);
                return _umbracoApplicationUrl;
            }
        }

	    // ReSharper disable once InconsistentNaming
	    internal string _umbracoApplicationUrl;

        private Lazy<bool> _configured;
        internal MainDom MainDom { get; private set; }
       
	    private void Init()
		{
            MainDom = new MainDom(ProfilingLogger.Logger);
            MainDom.Acquire();
            
            //Create the lazy value to resolve whether or not the application is 'configured'
            _configured = new Lazy<bool>(() =>
            {
                try
                {
                    var configStatus = ConfigurationStatus;
                    var currentVersion = UmbracoVersion.GetSemanticVersion();

                    var ok =
                        //we are not configured if this is null    
                        string.IsNullOrWhiteSpace(configStatus) == false
                        //they must match
                        && configStatus == currentVersion;

                    if (ok)
                    {
                        //The versions are the same in config, but are they the same in the database. We can only check this
                        // if we have a db context available, if we don't then we are not installed anyways
                        if (DatabaseContext.IsDatabaseConfigured && DatabaseContext.CanConnect)
                        {
                            var found = Services.MigrationEntryService.FindEntry(GlobalSettings.UmbracoMigrationName, UmbracoVersion.GetSemanticVersion());
                            if (found == null)
                            {
                                //we haven't executed this migration in this environment, so even though the config versions match, 
                                // this db has not been updated.
                                ProfilingLogger.Logger.Debug<ApplicationContext>(string.Format("The migration for version: '{0} has not been executed, there is no record in the database", currentVersion.ToSemanticString()));
                                ok = false;
                            }
                        }
                    }
                    else
                    {
                        ProfilingLogger.Logger.Debug<ApplicationContext>(string.Format("CurrentVersion different from configStatus: '{0}','{1}'", currentVersion.ToSemanticString(), configStatus));
                    }

                    return ok;
                }
                catch (Exception ex)
                {
                    LogHelper.Error<ApplicationContext>("Error determining if application is configured, returning false", ex);
                    return false;
                }

            }); 
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

	    internal ServerRole GetCurrentServerRole()
	    {
	        var registrar = ServerRegistrarResolver.Current.Registrar as IServerRegistrar2;
            return registrar == null ? ServerRole.Unknown : registrar.GetCurrentServerRole();
	    }

        private volatile bool _disposed;
        private readonly ReaderWriterLockSlim _disposalLocker = new ReaderWriterLockSlim();

        /// <summary>
        /// This will dispose and reset all resources used to run the application
        /// </summary>
        /// <remarks>
        /// IMPORTANT: Never dispose this object if you require the Umbraco application to run, disposing this object
        /// is generally used for unit testing and when your application is shutting down after you have booted Umbraco.
        /// </remarks>
        void IDisposable.Dispose()
        {
            // Only operate if we haven't already disposed
            if (_disposed) return;

            using (new WriteLock(_disposalLocker))
            {
                // Check again now we're inside the lock
                if (_disposed) return;

                //clear the cache
                if (ApplicationCache != null)
                {
                    ApplicationCache.ClearAllCache();    
                }
                //reset all resolvers
                ResolverCollection.ResetAll();
                //reset resolution itself (though this should be taken care of by resetting any of the resolvers above)
                Resolution.Reset();
                
                //reset the instance objects
                this.ApplicationCache = null;
                if (_databaseContext != null) //need to check the internal field here
                {
                    if (DatabaseContext.IsDatabaseConfigured && DatabaseContext.Database != null)
                    {
                        DatabaseContext.Database.Dispose();       
                    }                    
                }
                this.DatabaseContext = null;
                this.Services = null;
                this._isReady = false; //set the internal field
                
                // Indicate that the instance has been disposed.
                _disposed = true;
            }
        }
    }
}

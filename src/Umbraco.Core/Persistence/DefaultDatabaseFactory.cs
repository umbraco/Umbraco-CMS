using System;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Persistence
{
	/// <summary>
	/// The default implementation for the IDatabaseFactory
	/// </summary>
	/// <remarks>
	/// If we are running in an http context
	/// it will create one per context, otherwise it will be a global singleton object which is NOT thread safe
	/// since we need (at least) a new instance of the database object per thread.
	/// </remarks>
	internal class DefaultDatabaseFactory : DisposableObject, IDatabaseFactory
	{
	    private readonly string _connectionStringName;
	    private readonly ILogger _logger;
	    public string ConnectionString { get; private set; }
        public string ProviderName { get; private set; }
        
        //very important to have ThreadStatic:
        // see: http://issues.umbraco.org/issue/U4-2172
        [ThreadStatic]
        private static volatile UmbracoDatabase _nonHttpInstance;

		private static readonly object Locker = new object();

	    /// <summary>
	    /// Constructor accepting custom connection string
	    /// </summary>
	    /// <param name="connectionStringName">Name of the connection string in web.config</param>
	    /// <param name="logger"></param>
	    public DefaultDatabaseFactory(string connectionStringName, ILogger logger)
		{
	        if (logger == null) throw new ArgumentNullException("logger");
	        Mandate.ParameterNotNullOrEmpty(connectionStringName, "connectionStringName");
			_connectionStringName = connectionStringName;
	        _logger = logger;
		}

	    /// <summary>
	    /// Constructor accepting custom connectino string and provider name
	    /// </summary>
	    /// <param name="connectionString">Connection String to use with Database</param>
	    /// <param name="providerName">Database Provider for the Connection String</param>
	    /// <param name="logger"></param>
	    public DefaultDatabaseFactory(string connectionString, string providerName, ILogger logger)
		{
	        if (logger == null) throw new ArgumentNullException("logger");
	        Mandate.ParameterNotNullOrEmpty(connectionString, "connectionString");
			Mandate.ParameterNotNullOrEmpty(providerName, "providerName");
			ConnectionString = connectionString;
			ProviderName = providerName;
            _logger = logger;
		}

		public UmbracoDatabase CreateDatabase()
		{
			//no http context, create the singleton global object
			if (HttpContext.Current == null)
			{
                if (_nonHttpInstance == null)
				{
					lock (Locker)
					{
						//double check
                        if (_nonHttpInstance == null)
						{
                            _nonHttpInstance = string.IsNullOrEmpty(ConnectionString) == false && string.IsNullOrEmpty(ProviderName) == false
                                                  ? new UmbracoDatabase(ConnectionString, ProviderName, _logger)
                                                  : new UmbracoDatabase(_connectionStringName, _logger);
						}
					}
				}
                return _nonHttpInstance;
			}

			//we have an http context, so only create one per request
			if (HttpContext.Current.Items.Contains(typeof(DefaultDatabaseFactory)) == false)
			{
			    HttpContext.Current.Items.Add(typeof (DefaultDatabaseFactory),
			                                  string.IsNullOrEmpty(ConnectionString) == false && string.IsNullOrEmpty(ProviderName) == false
                                                  ? new UmbracoDatabase(ConnectionString, ProviderName, _logger)
                                                  : new UmbracoDatabase(_connectionStringName, _logger));
			}
			return (UmbracoDatabase)HttpContext.Current.Items[typeof(DefaultDatabaseFactory)];
		}

		protected override void DisposeResources()
		{
			if (HttpContext.Current == null)
			{
                _nonHttpInstance.Dispose();
			}
			else
			{
				if (HttpContext.Current.Items.Contains(typeof(DefaultDatabaseFactory)))
				{
					((UmbracoDatabase)HttpContext.Current.Items[typeof(DefaultDatabaseFactory)]).Dispose();
				}
			}
		}
	}
}
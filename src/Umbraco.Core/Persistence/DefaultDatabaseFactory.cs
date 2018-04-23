using System;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;

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
	internal class DefaultDatabaseFactory : DisposableObjectSlim, IDatabaseFactory2
	{
	    private readonly string _connectionStringName;
	    private readonly ILogger _logger;
	    public string ConnectionString { get; private set; }
        public string ProviderName { get; private set; }

        //private static readonly object Locker = new object();

        // bwc imposes a weird x-dependency between database factory and scope provider...
        public IScopeProviderInternal ScopeProvider { get; set; }

	    /// <summary>
	    /// Constructor accepting custom connection string
	    /// </summary>
	    /// <param name="connectionStringName">Name of the connection string in web.config</param>
	    /// <param name="logger"></param>
	    public DefaultDatabaseFactory(string connectionStringName, ILogger logger)
		{
            if (logger == null) throw new ArgumentNullException("logger");
	        Mandate.ParameterNotNullOrEmpty(connectionStringName, "connectionStringName");

            //if (NonContextValue != null) throw new Exception("NonContextValue is not null.");

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

            //if (NonContextValue != null) throw new Exception("NonContextValue is not null.");

            ConnectionString = connectionString;
			ProviderName = providerName;
            _logger = logger;
		}

		public UmbracoDatabase CreateDatabase()
		{
		    return ScopeProvider.GetAmbientOrNoScope().Database;
		}

	    public UmbracoDatabase CreateNewDatabase()
	    {
	        return CreateDatabaseInstance();

	    }

        internal UmbracoDatabase CreateDatabaseInstance()
	    {
            var database = string.IsNullOrEmpty(ConnectionString) == false && string.IsNullOrEmpty(ProviderName) == false
                    ? new UmbracoDatabase(ConnectionString, ProviderName, _logger)
                    : new UmbracoDatabase(_connectionStringName, _logger);
	        database.DatabaseFactory = this;
            return database;
	    }

        protected override void DisposeResources()
		{
        }
    }
}
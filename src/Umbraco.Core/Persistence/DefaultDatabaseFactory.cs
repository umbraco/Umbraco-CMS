using System;
using System.Configuration;
using System.Web;
using NPoco;
using NPoco.FluentMappings;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.FaultHandling;
using Umbraco.Core.Persistence.Mappers;

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
	    private readonly DatabaseFactory _databaseFactory;
        private readonly RetryPolicy _connectionRetryPolicy;
        private readonly RetryPolicy _commandRetryPolicy;

        public string ConnectionString { get; private set; }
        public string ProviderName { get; private set; }

        //very important to have ThreadStatic:
        // see: http://issues.umbraco.org/issue/U4-2172
        [ThreadStatic]
        private static Lazy<UmbracoDatabase> _nonHttpInstance;
        
	    /// <summary>
	    /// Constructor accepting custom connection string
	    /// </summary>
	    /// <param name="connectionStringName">Name of the connection string in web.config</param>
	    /// <param name="logger"></param>
	    public DefaultDatabaseFactory(string connectionStringName, ILogger logger)
            : this(logger)
		{
	        Mandate.ParameterNotNullOrEmpty(connectionStringName, "connectionStringName");
			_connectionStringName = connectionStringName;

            if (ConfigurationManager.ConnectionStrings[connectionStringName] == null)
                throw new InvalidOperationException("Can't find a connection string with the name '" + connectionStringName + "'");
            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;

            _connectionRetryPolicy = RetryPolicyFactory.GetDefaultSqlConnectionRetryPolicyByConnectionString(connectionString);
            _commandRetryPolicy = RetryPolicyFactory.GetDefaultSqlCommandRetryPolicyByConnectionString(connectionString);
        }

        /// <summary>
        /// Constructor accepting custom connectino string and provider name
        /// </summary>
        /// <param name="connectionString">Connection String to use with Database</param>
        /// <param name="providerName">Database Provider for the Connection String</param>
        /// <param name="logger"></param>
        public DefaultDatabaseFactory(string connectionString, string providerName, ILogger logger)
            : this(logger)
		{
	        Mandate.ParameterNotNullOrEmpty(connectionString, "connectionString");
			Mandate.ParameterNotNullOrEmpty(providerName, "providerName");
			ConnectionString = connectionString;
			ProviderName = providerName;

            _connectionRetryPolicy = RetryPolicyFactory.GetDefaultSqlConnectionRetryPolicyByConnectionString(connectionString);
            _commandRetryPolicy = RetryPolicyFactory.GetDefaultSqlCommandRetryPolicyByConnectionString(connectionString);
        }

        private DefaultDatabaseFactory(ILogger logger)
	    {
            if (logger == null) throw new ArgumentNullException("logger");
            _logger = logger;

            // ensure we have only 1 set of mappers, and 1 PocoDataFactory, for all database
            // so that everything NPoco is properly cached for the lifetime of the application
	        var mappers = new MapperCollection { new PocoMapper() };
	        var pocoDataFactory = new FluentPocoDataFactory((type, iPocoDataFactory) => new PocoDataBuilder(type, mappers).Init());
            var config = new FluentConfig(xmappers => pocoDataFactory);

            // create the database factory
            _databaseFactory = DatabaseFactory.Config(x => x
                .UsingDatabase(CreateDatabaseInstance) // creating UmbracoDatabase instances
                .WithFluentConfig(config)); // with proper configuration

            _nonHttpInstance = new Lazy<UmbracoDatabase>(() => (UmbracoDatabase)_databaseFactory.GetDatabase());
        }

	    private UmbracoDatabase CreateDatabaseInstance()
	    {
	        return string.IsNullOrEmpty(ConnectionString) == false && string.IsNullOrEmpty(ProviderName) == false
                ? new UmbracoDatabase(ConnectionString, ProviderName, _logger, _connectionRetryPolicy, _commandRetryPolicy)
                : new UmbracoDatabase(_connectionStringName, _logger, _connectionRetryPolicy, _commandRetryPolicy);
        }

		public UmbracoDatabase CreateDatabase()
		{
			// no http context, create the singleton global object
			if (HttpContext.Current == null)
			{
			    return _nonHttpInstance.Value;
			}

			// we have an http context, so only create one per request
		    var db = HttpContext.Current.Items[typeof (DefaultDatabaseFactory)] as UmbracoDatabase;
		    if (db == null) HttpContext.Current.Items[typeof (DefaultDatabaseFactory)] = db = (UmbracoDatabase) _databaseFactory.GetDatabase();
		    return db;
		}

		protected override void DisposeResources()
		{
			if (HttpContext.Current == null && _nonHttpInstance.IsValueCreated)
			{
                _nonHttpInstance.Value.Dispose();
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
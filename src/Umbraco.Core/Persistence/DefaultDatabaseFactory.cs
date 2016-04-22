using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Web;
using NPoco;
using NPoco.FluentMappings;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.FaultHandling;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Default implementation of <see cref="IDatabaseFactory"/>.
    /// </summary>
    /// <remarks>
    /// <para>This factory implementation creates and manages an "ambient" database connection. When running
    /// within an Http context, "ambient" means "associated with that context". Otherwise, it means "static to
    /// the current thread". In this latter case, note that the database connection object is not thread safe.</para>
    /// <para>It wraps an NPoco DatabaseFactory which is initializes with a proper IPocoDataFactory to ensure
    /// that NPoco's plumbing is cached appropriately for the whole application.</para>
    /// </remarks>
    internal class DefaultDatabaseFactory : DisposableObject, IDatabaseFactory
    {
        private readonly ISqlSyntaxProvider[] _sqlSyntaxProviders;
	    private readonly ILogger _logger;
        private bool _configured;

	    private DatabaseFactory _databaseFactory;
        private IPocoDataFactory _pocoDataFactory;
        private string _connectionString;
        private string _providerName;
        private DbProviderFactory _dbProviderFactory;
        private DatabaseType _databaseType;
        private ISqlSyntaxProvider _sqlSyntax;
        private RetryPolicy _connectionRetryPolicy;
        private RetryPolicy _commandRetryPolicy;

        // fixme - what needs to be private fields vs public properties?
        public bool Configured => _configured;
        public ISqlSyntaxProvider SqlSyntax
        {
            get
            {
                EnsureConfigured();
                return _sqlSyntax;
            }
        }

        // very important to have ThreadStatic,
        // see: http://issues.umbraco.org/issue/U4-2172
        [ThreadStatic]
        private static Lazy<UmbracoDatabase> _nonHttpInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDatabaseFactory"/> with the default connection, and a logger.
        /// </summary>
        /// <param name="sqlSyntaxProviders">The collection of available sql syntax providers.</param>
        /// <param name="logger">A logger.</param>
        /// <remarks>Used by LightInject.</remarks>
        public DefaultDatabaseFactory(IEnumerable<ISqlSyntaxProvider> sqlSyntaxProviders, ILogger logger)
            : this(GlobalSettings.UmbracoConnectionName, sqlSyntaxProviders, logger)
        {
            if (Configured == false)
                DatabaseContext.GiveLegacyAChance(this, logger);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDatabaseFactory"/> with a connection string name and a logger.
        /// </summary>
        /// <param name="connectionStringName">The name of the connection string in web.config.</param>
        /// <param name="sqlSyntaxProviders">The collection of available sql syntax providers.</param>
        /// <param name="logger">A logger</param>
        /// <remarks>Used by the other ctor and in tests.</remarks>
        public DefaultDatabaseFactory(string connectionStringName, IEnumerable<ISqlSyntaxProvider> sqlSyntaxProviders, ILogger logger)
		{
            if (sqlSyntaxProviders == null) throw new ArgumentNullException(nameof(sqlSyntaxProviders));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (string.IsNullOrWhiteSpace(connectionStringName)) throw new ArgumentException("Value cannot be null nor empty.", nameof(connectionStringName));

            _sqlSyntaxProviders = sqlSyntaxProviders.ToArray();
            _logger = logger;

            var settings = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (settings == null)
                return; // not configured

            Configure(settings.ConnectionString, settings.ProviderName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDatabaseFactory"/> with a connection string, a provider name and a logger.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="providerName">The name of the database provider.</param>
	    /// <param name="sqlSyntaxProviders">The collection of available sql syntax providers.</param>
        /// <param name="logger">A logger.</param>
        /// <remarks>Used in tests.</remarks>
        public DefaultDatabaseFactory(string connectionString, string providerName, IEnumerable<ISqlSyntaxProvider> sqlSyntaxProviders, ILogger logger)
		{
            if (sqlSyntaxProviders == null) throw new ArgumentNullException(nameof(sqlSyntaxProviders));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _sqlSyntaxProviders = sqlSyntaxProviders.ToArray();
            _logger = logger;

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(providerName))
                return; // not configured

            Configure(connectionString, providerName);
		}

        public void Configure(string connectionString, string providerName)
        {
            if (_configured) throw new InvalidOperationException("Already configured.");

            Mandate.ParameterNotNullOrEmpty(connectionString, nameof(connectionString));
            Mandate.ParameterNotNullOrEmpty(providerName, nameof(providerName));

            _connectionString = connectionString;
            _providerName = providerName;

            _connectionRetryPolicy = RetryPolicyFactory.GetDefaultSqlConnectionRetryPolicyByConnectionString(_connectionString);
            _commandRetryPolicy = RetryPolicyFactory.GetDefaultSqlCommandRetryPolicyByConnectionString(_connectionString);

            _dbProviderFactory = DbProviderFactories.GetFactory(_providerName);
            if (_dbProviderFactory == null)
                throw new Exception($"Can't find a provider factory for provider name \"{_providerName}\".");
            _databaseType = DatabaseType.Resolve(_dbProviderFactory.GetType().Name, _providerName);
            if (_databaseType == null)
                throw new Exception($"Can't find an NPoco database type for provider name \"{_providerName}\".");

            _sqlSyntax = GetSqlSyntaxProvider(_providerName);
            if (_sqlSyntax == null)
                throw new Exception($"Can't find a sql syntax provider for provider name \"{_providerName}\".");

            // ensure we have only 1 set of mappers, and 1 PocoDataFactory, for all database
            // so that everything NPoco is properly cached for the lifetime of the application
            var mappers = new MapperCollection { new PocoMapper() };
            var factory = new FluentPocoDataFactory((type, iPocoDataFactory) => new PocoDataBuilder(type, mappers).Init());
            _pocoDataFactory = factory;
            var config = new FluentConfig(xmappers => factory);

            // create the database factory
            _databaseFactory = DatabaseFactory.Config(x => x
                .UsingDatabase(CreateDatabaseInstance) // creating UmbracoDatabase instances
                .WithFluentConfig(config)); // with proper configuration

            _nonHttpInstance = new Lazy<UmbracoDatabase>(() => (UmbracoDatabase) _databaseFactory.GetDatabase());
            _configured = true;
        }

        // gets the sql syntax provider that corresponds, from attribute
        private ISqlSyntaxProvider GetSqlSyntaxProvider(string providerName)
        {
            var name = providerName.ToLowerInvariant();
            var provider = _sqlSyntaxProviders.FirstOrDefault(x =>
                x.GetType()
                    .FirstAttribute<SqlSyntaxProviderAttribute>()
                    .ProviderName.ToLowerInvariant()
                    .Equals(name));
            if (provider != null) return provider;
            throw new InvalidOperationException($"Unknown provider name \"{providerName}\"");

            // previously we'd try to return SqlServerSyntaxProvider by default but this is bad
            //provider = _syntaxProviders.FirstOrDefault(x => x.GetType() == typeof(SqlServerSyntaxProvider));
        }

        /// <summary>
        /// Gets a value indicating whether it is possible to connect to the database.
        /// </summary>
        /// <returns></returns>
        public bool CanConnect => _configured && DbConnectionExtensions.IsConnectionAvailable(_connectionString, _providerName);

        private void EnsureConfigured()
        {
            if (_configured == false)
                throw new InvalidOperationException("Not configured.");
        }

        // method used by NPoco's DatabaseFactory to actually create the database instance
        private UmbracoDatabase CreateDatabaseInstance()
	    {
	        return new UmbracoDatabase(_connectionString, _sqlSyntax, _databaseType, _dbProviderFactory, _logger, _connectionRetryPolicy, _commandRetryPolicy);
	    }

        /// <summary>
        /// Gets (creates or retrieves) the "ambient" database connection.
        /// </summary>
        /// <returns>The "ambient" database connection.</returns>
		public UmbracoDatabase GetDatabase()
        {
            EnsureConfigured();

			// no http context, create the thread-static singleton object
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
            // this is weird, because _nonHttpInstance is thread-static, so we would need
            // to dispose the factory in each thread where a database has been used - else
            // it only disposes the current thread's database instance.
            //
            // besides, we don't really want to dispose the factory, which is a singleton...

		    UmbracoDatabase db = null;

			if (HttpContext.Current == null)
		    {
                if (_nonHttpInstance.IsValueCreated)
                {
                    db = _nonHttpInstance.Value;
	    	        _nonHttpInstance = null;
                }
		    }
		    else
		    {
		        db = HttpContext.Current.Items[typeof(DefaultDatabaseFactory)] as UmbracoDatabase;
		        HttpContext.Current.Items[typeof (DefaultDatabaseFactory)] = null;
		    }

		    db?.Dispose();
		}
	}
}
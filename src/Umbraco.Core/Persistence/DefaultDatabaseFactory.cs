using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Threading;
using NPoco;
using NPoco.FluentMappings;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.FaultHandling;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.Querying;
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
        private readonly IMappingResolver _mappingResolver;
        private readonly IScopeContextAdapter _scopeContextAdapter;
        private readonly ISqlSyntaxProvider[] _sqlSyntaxProviders;
        private readonly ILogger _logger;

        private const string HttpItemKey = "Umbraco.Core.Persistence.DefaultDatabaseFactory";
        private DatabaseFactory _databaseFactory;
        private IPocoDataFactory _pocoDataFactory;
        private string _connectionString;
        private string _providerName;
        private DbProviderFactory _dbProviderFactory;
        private DatabaseType _databaseType;
        private ISqlSyntaxProvider _sqlSyntax;
        private RetryPolicy _connectionRetryPolicy;
        private RetryPolicy _commandRetryPolicy;
        private IQueryFactory _queryFactory;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public bool Configured { get; private set; }
        public ISqlSyntaxProvider SqlSyntax
        {
            get
            {
                EnsureConfigured();
                return _sqlSyntax;
            }
        }

        public IQueryFactory QueryFactory
        {
            get
            {
                EnsureConfigured();
                return _queryFactory ?? (_queryFactory = new QueryFactory(SqlSyntax, _mappingResolver));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDatabaseFactory"/> with the default connection, and a logger.
        /// </summary>
        /// <param name="sqlSyntaxProviders">The collection of available sql syntax providers.</param>
        /// <param name="logger">A logger.</param>
        /// <param name="scopeContextAdapter"></param>
        /// <param name="mappingResolver"></param>
        /// <remarks>Used by LightInject.</remarks>
        public DefaultDatabaseFactory(IEnumerable<ISqlSyntaxProvider> sqlSyntaxProviders, ILogger logger, IScopeContextAdapter scopeContextAdapter, IMappingResolver mappingResolver)
            : this(GlobalSettings.UmbracoConnectionName, sqlSyntaxProviders, logger, scopeContextAdapter, mappingResolver)
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
        /// <param name="scopeContextAdapter"></param>
        /// <param name="mappingResolver"></param>
        /// <remarks>Used by the other ctor and in tests.</remarks>
        public DefaultDatabaseFactory(string connectionStringName, IEnumerable<ISqlSyntaxProvider> sqlSyntaxProviders, ILogger logger, IScopeContextAdapter scopeContextAdapter, IMappingResolver mappingResolver)
        {
            if (sqlSyntaxProviders == null) throw new ArgumentNullException(nameof(sqlSyntaxProviders));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (scopeContextAdapter == null) throw new ArgumentNullException(nameof(scopeContextAdapter));
            if (string.IsNullOrWhiteSpace(connectionStringName)) throw new ArgumentException("Value cannot be null nor empty.", nameof(connectionStringName));
            if (mappingResolver == null) throw new ArgumentNullException(nameof(mappingResolver));

            _mappingResolver = mappingResolver;
            _sqlSyntaxProviders = sqlSyntaxProviders.ToArray();
            _logger = logger;
            _scopeContextAdapter = scopeContextAdapter;

            _logger.Debug<DefaultDatabaseFactory>("Created!");

            var settings = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (settings == null)
                return; // not configured

            // could as well be <add name="umbracoDbDSN" connectionString="" providerName="" />
            // so need to test the values too
            var connectionString = settings.ConnectionString;
            var providerName = settings.ProviderName;
            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(providerName))
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
        /// <param name="scopeContextAdapter"></param>
        /// <param name="mappingResolver"></param>
        /// <remarks>Used in tests.</remarks>
        public DefaultDatabaseFactory(string connectionString, string providerName, IEnumerable<ISqlSyntaxProvider> sqlSyntaxProviders, ILogger logger, IScopeContextAdapter scopeContextAdapter, IMappingResolver mappingResolver)
        {
            if (sqlSyntaxProviders == null) throw new ArgumentNullException(nameof(sqlSyntaxProviders));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (scopeContextAdapter == null) throw new ArgumentNullException(nameof(scopeContextAdapter));
            if (mappingResolver == null) throw new ArgumentNullException(nameof(mappingResolver));

            _mappingResolver = mappingResolver;
            _sqlSyntaxProviders = sqlSyntaxProviders.ToArray();
            _logger = logger;
            _scopeContextAdapter = scopeContextAdapter;

            _logger.Debug<DefaultDatabaseFactory>("Created!");

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(providerName))
                return; // not configured

            Configure(connectionString, providerName);
        }

        public void Configure(string connectionString, string providerName)
        {
            using (new WriteLock(_lock))
            {
                _logger.Debug<DefaultDatabaseFactory>("Configuring!");

                if (Configured) throw new InvalidOperationException("Already configured.");

                if (connectionString.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(connectionString));
                if (providerName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(providerName));

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

                if (_databaseFactory == null) throw new NullReferenceException("The call to DatabaseFactory.Config yielded a null DatabaseFactory instance");

                _logger.Debug<DefaultDatabaseFactory>("Created _nonHttpInstance");
                Configured = true;
            }
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
        public bool CanConnect => Configured && DbConnectionExtensions.IsConnectionAvailable(_connectionString, _providerName);

        private void EnsureConfigured()
        {
            using (new ReadLock(_lock))
            {
                if (Configured == false)
                    throw new InvalidOperationException("Not configured.");
            }
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

            // check if it's in scope
            var db = _scopeContextAdapter.Get(HttpItemKey) as UmbracoDatabase;
            if (db != null) return db;
            db = (UmbracoDatabase) _databaseFactory.GetDatabase();
            _scopeContextAdapter.Set(HttpItemKey, db);
            return db;
        }

        protected override void DisposeResources()
        {
            // this is weird, because _nonHttpInstance is thread-static, so we would need
            // to dispose the factory in each thread where a database has been used - else
            // it only disposes the current thread's database instance.
            //
            // besides, we don't really want to dispose the factory, which is a singleton...

            var db = _scopeContextAdapter.Get(HttpItemKey) as UmbracoDatabase;
            _scopeContextAdapter.Clear(HttpItemKey);
            db?.Dispose();
            Configured = false;
        }
    }
}
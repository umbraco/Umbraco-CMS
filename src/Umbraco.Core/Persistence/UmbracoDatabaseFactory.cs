using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Threading;
using NPoco;
using NPoco.FluentMappings;
using Umbraco.Core.Configuration;
using Umbraco.Core.Exceptions;
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
    internal class UmbracoDatabaseFactory : DisposableObject, IDatabaseFactory
    {
        //private readonly IUmbracoDatabaseAccessor _umbracoDatabaseAccessor;
        private readonly IDatabaseScopeAccessor _databaseScopeAccessor;
        private readonly ISqlSyntaxProvider[] _sqlSyntaxProviders;
        private readonly IMapperCollection _mappers;
        private readonly ILogger _logger;

        private DatabaseFactory _npocoDatabaseFactory;
        private IPocoDataFactory _pocoDataFactory;
        private string _connectionString;
        private string _providerName;
        private DbProviderFactory _dbProviderFactory;
        private DatabaseType _databaseType;
        private ISqlSyntaxProvider _sqlSyntax;
        private IQueryFactory _queryFactory;
        private SqlContext _sqlContext;
        private RetryPolicy _connectionRetryPolicy;
        private RetryPolicy _commandRetryPolicy;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoDatabaseFactory"/>.
        /// </summary>
        /// <remarks>Used by LightInject.</remarks>
        public UmbracoDatabaseFactory(IEnumerable<ISqlSyntaxProvider> sqlSyntaxProviders, ILogger logger, IDatabaseScopeAccessor databaseScopeAccessor, IMapperCollection mappers)
            : this(GlobalSettings.UmbracoConnectionName, sqlSyntaxProviders, logger, databaseScopeAccessor, mappers)
        {
            if (Configured == false)
                DatabaseBuilder.GiveLegacyAChance(this, logger);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoDatabaseFactory"/>.
        /// </summary>
        /// <remarks>Used by the other ctor and in tests.</remarks>
        public UmbracoDatabaseFactory(string connectionStringName, IEnumerable<ISqlSyntaxProvider> sqlSyntaxProviders, ILogger logger, IDatabaseScopeAccessor databaseScopeAccessor, IMapperCollection mappers)
        {
            if (sqlSyntaxProviders == null) throw new ArgumentNullException(nameof(sqlSyntaxProviders));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (databaseScopeAccessor == null) throw new ArgumentNullException(nameof(databaseScopeAccessor));
            if (string.IsNullOrWhiteSpace(connectionStringName)) throw new ArgumentNullOrEmptyException(nameof(connectionStringName));
            if (mappers == null) throw new ArgumentNullException(nameof(mappers));

            _mappers = mappers;
            _sqlSyntaxProviders = sqlSyntaxProviders.ToArray();
            _logger = logger;
            _databaseScopeAccessor = databaseScopeAccessor;

            var settings = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (settings == null)
                return; // not configured

            // could as well be <add name="umbracoDbDSN" connectionString="" providerName="" />
            // so need to test the values too
            var connectionString = settings.ConnectionString;
            var providerName = settings.ProviderName;
            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(providerName))
            {
                logger.Debug<UmbracoDatabaseFactory>("Missing connection string or provider name, defer configuration.");
                return; // not configured
            }

            Configure(settings.ConnectionString, settings.ProviderName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoDatabaseFactory"/>.
        /// </summary>
        /// <remarks>Used in tests.</remarks>
        public UmbracoDatabaseFactory(string connectionString, string providerName, IEnumerable<ISqlSyntaxProvider> sqlSyntaxProviders, ILogger logger, IDatabaseScopeAccessor databaseScopeAccessor, IMapperCollection mappers)
        {
            if (sqlSyntaxProviders == null) throw new ArgumentNullException(nameof(sqlSyntaxProviders));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (databaseScopeAccessor == null) throw new ArgumentNullException(nameof(databaseScopeAccessor));
            if (mappers == null) throw new ArgumentNullException(nameof(mappers));

            _mappers = mappers;
            _sqlSyntaxProviders = sqlSyntaxProviders.ToArray();
            _logger = logger;
            _databaseScopeAccessor = databaseScopeAccessor;

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(providerName))
            {
                logger.Debug<UmbracoDatabaseFactory>("Missing connection string or provider name, defer configuration.");
                return; // not configured
            }

            Configure(connectionString, providerName);
        }

        #endregion

        /// <summary>
        /// Gets a value indicating whether the database is configured (no connect test).
        /// </summary>
        /// <remarks></remarks>
        public bool Configured { get; private set; }

        /// <summary>
        /// Gets a value indicating whether it is possible to connect to the database.
        /// </summary>
        public bool CanConnect => Configured && DbConnectionExtensions.IsConnectionAvailable(_connectionString, _providerName);

        /// <summary>
        /// Gets the database sql syntax provider.
        /// </summary>
        public ISqlSyntaxProvider SqlSyntax
        {
            get
            {
                EnsureConfigured();
                return _sqlSyntax;
            }
        }

        /// <summary>
        /// Gets the database query factory.
        /// </summary>
        public IQueryFactory QueryFactory {
            get
            {
                EnsureConfigured();
                return _queryFactory;
            }
        }

        public Sql<SqlContext> Sql() => NPoco.Sql.BuilderFor(_sqlContext);

        // will be configured by the database context
        public void Configure(string connectionString, string providerName)
        {
            using (new WriteLock(_lock))
            {
                _logger.Debug<UmbracoDatabaseFactory>("Configuring.");

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
                var mappers = new NPoco.MapperCollection { new PocoMapper() };
                var factory = new FluentPocoDataFactory((type, iPocoDataFactory) => new PocoDataBuilder(type, mappers).Init());
                _pocoDataFactory = factory;
                var config = new FluentConfig(xmappers => factory);

                // create the database factory
                _npocoDatabaseFactory = DatabaseFactory.Config(x => x
                    .UsingDatabase(CreateDatabaseInstance) // creating UmbracoDatabase instances
                    .WithFluentConfig(config)); // with proper configuration

                if (_npocoDatabaseFactory == null) throw new NullReferenceException("The call to DatabaseFactory.Config yielded a null DatabaseFactory instance.");

                // these are created here because it is the UmbracoDatabaseFactory that determines
                // the sql syntax, poco data factory, and database type - so it "owns" the context
                // and the query factory
                _sqlContext = new SqlContext(_sqlSyntax, _pocoDataFactory, _databaseType);
                _queryFactory = new QueryFactory(_sqlSyntax, _mappers);

                _logger.Debug<UmbracoDatabaseFactory>("Configured.");
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
            return new UmbracoDatabase(_connectionString, _sqlContext, _dbProviderFactory, _logger, _connectionRetryPolicy, _commandRetryPolicy);
        }

        // fixme temp?
        public UmbracoDatabase Database => GetDatabase();

        /// <summary>
        /// Gets (creates or retrieves) the ambient database connection.
        /// </summary>
        /// <returns>The ambient database connection.</returns>
		public UmbracoDatabase GetDatabase()
        {
            EnsureConfigured();

            var scope = _databaseScopeAccessor.Scope;
            if (scope == null) throw new InvalidOperationException("Out of scope.");
            return scope.Database;

            //// check if it's in scope
            //var db = _umbracoDatabaseAccessor.UmbracoDatabase;
            //if (db != null) return db;
            //db = (UmbracoDatabase) _npocoDatabaseFactory.GetDatabase();
            //_umbracoDatabaseAccessor.UmbracoDatabase = db;
            //return db;
        }

        /// <summary>
        /// Creates a new database instance.
        /// </summary>
        /// <remarks>The database instance is not part of any scope and must be disposed after being used.</remarks>
        public UmbracoDatabase CreateDatabase()
        {
            return (UmbracoDatabase) _npocoDatabaseFactory.GetDatabase();
        }

        protected override void DisposeResources()
        {
            // this is weird, because hybrid accessors store different databases per
            // thread, so we don't really know what we are disposing here...
            // besides, we don't really want to dispose the factory, which is a singleton...

            // fixme - does not make any sense!
            //var db = _umbracoDatabaseAccessor.UmbracoDatabase;
            //_umbracoDatabaseAccessor.UmbracoDatabase = null;
            //db?.Dispose();
            Configured = false;
        }

        // during tests, the thread static var can leak between tests
        // this method provides a way to force-reset the variable
	    internal void ResetForTests()
	    {
            // fixme - does not make any sense!
            //var db = _umbracoDatabaseAccessor.UmbracoDatabase;
            //_umbracoDatabaseAccessor.UmbracoDatabase = null;
            //db?.Dispose();
	        _databaseScopeAccessor.Scope = null;
	    }

        //public bool HasAmbient => _umbracoDatabaseAccessor.UmbracoDatabase != null;

        //public UmbracoDatabase DetachAmbient()
        //{
        //    var database = _umbracoDatabaseAccessor.UmbracoDatabase;
        //    _umbracoDatabaseAccessor.UmbracoDatabase = null;
        //    return database;
        //}

        //public void AttachAmbient(UmbracoDatabase database)
        //{
        //    var tmp = _umbracoDatabaseAccessor.UmbracoDatabase;
        //    _umbracoDatabaseAccessor.UmbracoDatabase = database;
        //    tmp?.Dispose();

        //    // fixme - what shall we do with tmp?
        //    // fixme - what about using "disposing" of the database to remove it from "ambient"?!
        //}

        //public IDisposable CreateScope(bool force = false) // fixme - why would we ever force?
        //{
        //    if (HasAmbient)
        //    {
        //        return force
        //            ? new DatabaseScope(this, DetachAmbient(), GetDatabase())
        //            : new DatabaseScope(this, null, null);
        //    }

        //    // create a new, temp, database (will be disposed with DatabaseScope)
        //    return new DatabaseScope(this, null, GetDatabase());
        //}

        public IDisposable CreateScope()
        {
            return new DatabaseScope(_databaseScopeAccessor, this);
        }

        /*
        private class DatabaseScope : IDisposable
        {
            private readonly UmbracoDatabaseFactory _factory;
            private readonly UmbracoDatabase _orig;
            private readonly UmbracoDatabase _temp;

            // orig is the original database that was ambient when the scope was created
            //   if not null, it has been detached in order to be replaced by temp, which cannot be null
            //   if null, either there was no ambient database, or we don't want to replace it
            // temp is the scope database that is created for the scope
            //   if not null, it has been attached and is not the ambient database,
            //     and when the scope is disposed it will be detached, disposed, and replaced by orig
            //   if null, the scope is nested and reusing the ambient database, without touching anything

            public DatabaseScope(UmbracoDatabaseFactory factory, UmbracoDatabase orig, UmbracoDatabase temp)
            {
                if (factory == null) throw new ArgumentNullException(nameof(factory));
                _factory = factory;

                _orig = orig;
                _temp = temp;
            }

            public void Dispose()
            {
                if (_temp != null) // if the scope had its own database
                {
                    // detach and ensure consistency, then dispose
                    var temp = _factory.DetachAmbient();
                    if (temp != _temp) throw new Exception("bam!");
                    temp.Dispose();

                    // re-instate original database if any
                    if (_orig != null)
                        _factory.AttachAmbient(_orig);
                }
                GC.SuppressFinalize(this);
            }
        }
        */
    }
}
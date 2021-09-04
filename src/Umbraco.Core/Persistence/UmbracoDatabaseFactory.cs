using System;
using System.Configuration;
using System.Data.Common;
using System.Threading;
using NPoco;
using NPoco.FluentMappings;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.FaultHandling;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Default implementation of <see cref="IUmbracoDatabaseFactory"/>.
    /// </summary>
    /// <remarks>
    /// <para>This factory implementation creates and manages an "ambient" database connection. When running
    /// within an Http context, "ambient" means "associated with that context". Otherwise, it means "static to
    /// the current thread". In this latter case, note that the database connection object is not thread safe.</para>
    /// <para>It wraps an NPoco UmbracoDatabaseFactory which is initializes with a proper IPocoDataFactory to ensure
    /// that NPoco's plumbing is cached appropriately for the whole application.</para>
    /// </remarks>
    // TODO: these comments are not true anymore
    // TODO: this class needs not be disposable!
    internal class UmbracoDatabaseFactory : DisposableObjectSlim, IUmbracoDatabaseFactory
    {
        private readonly Lazy<IMapperCollection> _mappers;
        private readonly ILogger _logger;

        private object _lock = new object();

        private DatabaseFactory _npocoDatabaseFactory;
        private IPocoDataFactory _pocoDataFactory;
        private string _connectionString;
        private string _providerName;
        private DbProviderFactory _dbProviderFactory;
        private DatabaseType _databaseType;
        private ISqlSyntaxProvider _sqlSyntax;
        private RetryPolicy _connectionRetryPolicy;
        private RetryPolicy _commandRetryPolicy;
        private NPoco.MapperCollection _pocoMappers;
        private SqlContext _sqlContext;
        private bool _upgrading;
        private bool _initialized;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoDatabaseFactory"/>.
        /// </summary>
        /// <remarks>Used by core runtime.</remarks>
        public UmbracoDatabaseFactory(ILogger logger, Lazy<IMapperCollection> mappers)
            : this(Constants.System.UmbracoConnectionName, logger, mappers)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoDatabaseFactory"/>.
        /// </summary>
        /// <remarks>Used by the other ctor and in tests.</remarks>
        public UmbracoDatabaseFactory(string connectionStringName, ILogger logger, Lazy<IMapperCollection> mappers)
        {
            if (connectionStringName == null) throw new ArgumentNullException(nameof(connectionStringName));
            if (string.IsNullOrWhiteSpace(connectionStringName)) throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(connectionStringName));

            _mappers = mappers ?? throw new ArgumentNullException(nameof(mappers));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var settings = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (settings == null)
            {
                logger.Debug<UmbracoDatabaseFactory>("Missing connection string, defer configuration.");
                return; // not configured
            }

            // could as well be <add name="umbracoDbDSN" connectionString="" providerName="" />
            // so need to test the values too
            var connectionString = settings.ConnectionString;
            var providerName = settings.ProviderName;
            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(providerName))
            {
                logger.Debug<UmbracoDatabaseFactory>("Empty connection string or provider name, defer configuration.");
                return; // not configured
            }

            Configure(settings.ConnectionString, settings.ProviderName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoDatabaseFactory"/>.
        /// </summary>
        /// <remarks>Used in tests.</remarks>
        public UmbracoDatabaseFactory(string connectionString, string providerName, ILogger logger, Lazy<IMapperCollection> mappers)
        {
            _mappers = mappers ?? throw new ArgumentNullException(nameof(mappers));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(providerName))
            {
                logger.Debug<UmbracoDatabaseFactory>("Missing connection string or provider name, defer configuration.");
                return; // not configured
            }

            Configure(connectionString, providerName);
        }

        #endregion

        /// <inheritdoc />
        public bool Configured
        {
            get
            {
                lock (_lock)
                {
                    return !_connectionString.IsNullOrWhiteSpace() && !_providerName.IsNullOrWhiteSpace();
                }
            }
        }

        /// <inheritdoc />
        public bool Initialized => Volatile.Read(ref _initialized);

        /// <inheritdoc />
        public string ConnectionString => _connectionString;

        /// <inheritdoc />
        public string ProviderName => _providerName;

        /// <inheritdoc />
        public bool CanConnect =>
            // actually tries to connect to the database (regardless of configured/initialized)
            !_connectionString.IsNullOrWhiteSpace() && !_providerName.IsNullOrWhiteSpace() &&
            DbConnectionExtensions.IsConnectionAvailable(_connectionString, _providerName);

        private void UpdateSqlServerDatabaseType()
        {
            // replace NPoco database type by a more efficient one

            var setting = ConfigurationManager.AppSettings[Constants.AppSettings.Debug.DatabaseFactoryServerVersion];
            var fromSettings = false;

            if (setting.IsNullOrWhiteSpace() || !setting.StartsWith("SqlServer.")
                || !Enum<SqlServerSyntaxProvider.VersionName>.TryParse(setting.Substring("SqlServer.".Length), out var versionName, true))
            {
                versionName = ((SqlServerSyntaxProvider) _sqlSyntax).GetSetVersion(_connectionString, _providerName, _logger).ProductVersionName;
            }
            else
            {
                fromSettings = true;
            }

            switch (versionName)
            {
                case SqlServerSyntaxProvider.VersionName.V2008:
                    _databaseType = DatabaseType.SqlServer2008;
                    break;
                case SqlServerSyntaxProvider.VersionName.V2012:
                case SqlServerSyntaxProvider.VersionName.V2014:
                case SqlServerSyntaxProvider.VersionName.V2016:
                case SqlServerSyntaxProvider.VersionName.V2017:
                case SqlServerSyntaxProvider.VersionName.V2019:
                    _databaseType = DatabaseType.SqlServer2012;
                    break;
                // else leave unchanged
            }

            _logger.Debug<UmbracoDatabaseFactory, SqlServerSyntaxProvider.VersionName, DatabaseType, string>("SqlServer {SqlServerVersion}, DatabaseType is {DatabaseType} ({Source}).",
                versionName, _databaseType, fromSettings ? "settings" : "detected");
        }

        /// <inheritdoc />
        public ISqlContext SqlContext
        {
            get
            {
                // must be initialized to have a context
                EnsureInitialized();
                return _sqlContext;
            }
        }

        /// <inheritdoc />
        public void ConfigureForUpgrade()
        {
            _upgrading = true;
        }

        /// <inheritdoc />
        public void Configure(string connectionString, string providerName)
        {
            if (connectionString.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(connectionString));
            if (providerName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(providerName));

            lock (_lock)
            {
                if (Volatile.Read(ref _initialized))
                    throw new InvalidOperationException("Already initialized.");

                _connectionString = connectionString;
                _providerName = providerName;
            }

            // rest to be lazy-initialized
        }

        private void EnsureInitialized()
        {
            LazyInitializer.EnsureInitialized(ref _sqlContext, ref _initialized, ref _lock, Initialize);
        }

        private SqlContext Initialize()
        {
            _logger.Debug<UmbracoDatabaseFactory>("Initializing.");

            if (_connectionString.IsNullOrWhiteSpace()) throw new InvalidOperationException("The factory has not been configured with a proper connection string.");
            if (_providerName.IsNullOrWhiteSpace()) throw new InvalidOperationException("The factory has not been configured with a proper provider name.");

            // cannot initialize without being able to talk to the database
            if (!DbConnectionExtensions.IsConnectionAvailable(_connectionString, _providerName))
                throw new Exception("Cannot connect to the database.");

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

            if (_databaseType.IsSqlServer())
                UpdateSqlServerDatabaseType();

            // ensure we have only 1 set of mappers, and 1 PocoDataFactory, for all database
            // so that everything NPoco is properly cached for the lifetime of the application
            _pocoMappers = new NPoco.MapperCollection { new PocoMapper() };
            var factory = new FluentPocoDataFactory(GetPocoDataFactoryResolver);
            _pocoDataFactory = factory;
            var config = new FluentConfig(xmappers => factory);

            // create the database factory
            _npocoDatabaseFactory = DatabaseFactory.Config(x => x
                .UsingDatabase(CreateDatabaseInstance) // creating UmbracoDatabase instances
                .WithFluentConfig(config)); // with proper configuration

            if (_npocoDatabaseFactory == null)
                throw new NullReferenceException("The call to UmbracoDatabaseFactory.Config yielded a null UmbracoDatabaseFactory instance.");

            _logger.Debug<UmbracoDatabaseFactory>("Initialized.");

            return new SqlContext(_sqlSyntax, _databaseType, _pocoDataFactory, _mappers);
        }

        /// <inheritdoc />
        public IUmbracoDatabase CreateDatabase()
        {
            // must be initialized to create a database
            EnsureInitialized();
            return (IUmbracoDatabase) _npocoDatabaseFactory.GetDatabase();
        }

        // gets initialized poco data builders
        private InitializedPocoDataBuilder GetPocoDataFactoryResolver(Type type, IPocoDataFactory factory)
            => new UmbracoPocoDataBuilder(type, _pocoMappers, _upgrading).Init();

        // gets the sql syntax provider that corresponds, from attribute
        private ISqlSyntaxProvider GetSqlSyntaxProvider(string providerName)
        {
            switch (providerName)
            {
                case Constants.DbProviderNames.SqlCe:
                    return new SqlCeSyntaxProvider();
                case Constants.DbProviderNames.SqlServer:
                    return new SqlServerSyntaxProvider();
                default:
                    throw new InvalidOperationException($"Unknown provider name \"{providerName}\"");
            }
        }

        // method used by NPoco's UmbracoDatabaseFactory to actually create the database instance
        private UmbracoDatabase CreateDatabaseInstance()
        {
            return new UmbracoDatabase(_connectionString, SqlContext, _dbProviderFactory, _logger, _connectionRetryPolicy, _commandRetryPolicy);
        }

        protected override void DisposeResources()
        {
            // this is weird, because hybrid accessors store different databases per
            // thread, so we don't really know what we are disposing here...
            // besides, we don't really want to dispose the factory, which is a singleton...

            // TODO: the class does not need be disposable
            //var db = _umbracoDatabaseAccessor.UmbracoDatabase;
            //_umbracoDatabaseAccessor.UmbracoDatabase = null;
            //db?.Dispose();
            Volatile.Write(ref _initialized, false);
        }

        // during tests, the thread static var can leak between tests
        // this method provides a way to force-reset the variable
        internal void ResetForTests()
        {
            // TODO: remove all this eventually
            //var db = _umbracoDatabaseAccessor.UmbracoDatabase;
            //_umbracoDatabaseAccessor.UmbracoDatabase = null;
            //db?.Dispose();
            //_databaseScopeAccessor.Scope = null;
        }
    }
}

using System;
using System.Configuration;
using System.Data.Common;
using System.Threading;
using LightInject;
using NPoco;
using NPoco.FluentMappings;
using Umbraco.Core.Exceptions;
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
    internal class UmbracoDatabaseFactory : DisposableObject, IUmbracoDatabaseFactory
    {
        private readonly Lazy<IMapperCollection> _mappers;
        private readonly ILogger _logger;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private DatabaseFactory _npocoDatabaseFactory;
        private IPocoDataFactory _pocoDataFactory;
        private string _connectionString;
        private string _providerName;
        private DbProviderFactory _dbProviderFactory;
        private DatabaseType _databaseType;
        private bool _serverVersionDetected;
        private ISqlSyntaxProvider _sqlSyntax;
        private RetryPolicy _connectionRetryPolicy;
        private RetryPolicy _commandRetryPolicy;
        private NPoco.MapperCollection _pocoMappers;
        private bool _upgrading;

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
            if (string.IsNullOrWhiteSpace(connectionStringName)) throw new ArgumentNullOrEmptyException(nameof(connectionStringName));

            _mappers = mappers ?? throw new ArgumentNullException(nameof(mappers));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
        public bool Configured { get; private set; }

        /// <inheritdoc />
        public string ConnectionString
        {
            get
            {
                EnsureConfigured();
                return _connectionString;
            }
        }

        /// <inheritdoc />
        public bool CanConnect
        {
            get
            {
                if (!Configured || !DbConnectionExtensions.IsConnectionAvailable(_connectionString, _providerName)) return false;

                if (_serverVersionDetected) return true;

                if (_databaseType.IsSqlServer())
                    DetectSqlServerVersion();
                _serverVersionDetected = true;

                return true;
            }
        }

        private void DetectSqlServerVersion()
        {
            // replace NPoco database type by a more efficient one

            var setting = ConfigurationManager.AppSettings["Umbraco.DatabaseFactory.ServerVersion"];
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
                    _databaseType = DatabaseType.SqlServer2012;
                    break;
                // else leave unchanged
            }

            _logger.Debug<UmbracoDatabaseFactory>("SqlServer {SqlServerVersion}, DatabaseType is {DatabaseType} ({Source}).",
                versionName, _databaseType, fromSettings ? "settings" : "detected");
        }

        /// <inheritdoc />
        public ISqlContext SqlContext { get; private set; }

        /// <inheritdoc />
        public void ConfigureForUpgrade()
        {
            _upgrading = true;
        }

        /// <inheritdoc />
        public void Configure(string connectionString, string providerName)
        {
            try
            {
                _lock.EnterWriteLock();

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
                _pocoMappers = new NPoco.MapperCollection { new PocoMapper() };
                var factory = new FluentPocoDataFactory(GetPocoDataFactoryResolver);
                _pocoDataFactory = factory;
                var config = new FluentConfig(xmappers => factory);

                // create the database factory
                _npocoDatabaseFactory = DatabaseFactory.Config(x => x
                    .UsingDatabase(CreateDatabaseInstance) // creating UmbracoDatabase instances
                    .WithFluentConfig(config)); // with proper configuration

                if (_npocoDatabaseFactory == null) throw new NullReferenceException("The call to UmbracoDatabaseFactory.Config yielded a null UmbracoDatabaseFactory instance.");

                SqlContext = new SqlContext(_sqlSyntax, _databaseType, _pocoDataFactory, _mappers);

                _logger.Debug<UmbracoDatabaseFactory>("Configured.");
                Configured = true;
            }
            finally
            {
                if (_lock.IsWriteLockHeld)
                    _lock.ExitWriteLock();
            }
        }

        /// <inheritdoc />
        public IUmbracoDatabase CreateDatabase()
        {
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
                case Constants.DbProviderNames.MySql:
                    return new MySqlSyntaxProvider(_logger);
                case Constants.DbProviderNames.SqlCe:
                    return new SqlCeSyntaxProvider();
                case Constants.DbProviderNames.SqlServer:
                    return new SqlServerSyntaxProvider();
                default:
                    throw new InvalidOperationException($"Unknown provider name \"{providerName}\"");
            }
        }

        // ensures that the database is configured, else throws
        private void EnsureConfigured()
        {
            _lock.EnterReadLock();
            try
            {
                if (Configured == false)
                    throw new InvalidOperationException("Not configured.");
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                    _lock.ExitReadLock();
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
            //_databaseScopeAccessor.Scope = null;
        }
    }
}

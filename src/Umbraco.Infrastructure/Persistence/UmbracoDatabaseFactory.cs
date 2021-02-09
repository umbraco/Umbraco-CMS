using System;
using System.Data.Common;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPoco;
using NPoco.FluentMappings;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Core.Migrations.Install;
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
    public class UmbracoDatabaseFactory : DisposableObjectSlim, IUmbracoDatabaseFactory
    {
        private readonly IDbProviderFactoryCreator _dbProviderFactoryCreator;
        private readonly DatabaseSchemaCreatorFactory _databaseSchemaCreatorFactory;
        private readonly GlobalSettings _globalSettings;
        private readonly Lazy<IMapperCollection> _mappers;
        private readonly ILogger<UmbracoDatabaseFactory> _logger;
        private readonly ILoggerFactory _loggerFactory;

        private object _lock = new object();

        private DatabaseFactory _npocoDatabaseFactory;
        private IPocoDataFactory _pocoDataFactory;
        private string _providerName;
        private DatabaseType _databaseType;
        private ISqlSyntaxProvider _sqlSyntax;
        private IBulkSqlInsertProvider _bulkSqlInsertProvider;
        private RetryPolicy _connectionRetryPolicy;
        private RetryPolicy _commandRetryPolicy;
        private NPoco.MapperCollection _pocoMappers;
        private SqlContext _sqlContext;
        private bool _upgrading;
        private bool _initialized;

        private DbProviderFactory _dbProviderFactory = null;

        private DbProviderFactory DbProviderFactory
        {
            get
            {
                if (_dbProviderFactory == null)
                {
                    _dbProviderFactory = string.IsNullOrWhiteSpace(_providerName)
                        ? null
                        : _dbProviderFactoryCreator.CreateFactory(_providerName);
                }

                return _dbProviderFactory;
            }
        }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoDatabaseFactory"/>.
        /// </summary>
        /// <remarks>Used by core runtime.</remarks>
        public UmbracoDatabaseFactory(ILogger<UmbracoDatabaseFactory> logger, ILoggerFactory loggerFactory, IOptions<GlobalSettings> globalSettings, IOptions<ConnectionStrings> connectionStrings, Lazy<IMapperCollection> mappers,IDbProviderFactoryCreator dbProviderFactoryCreator, DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory)
            : this(logger, loggerFactory, globalSettings.Value, connectionStrings.Value, mappers, dbProviderFactoryCreator, databaseSchemaCreatorFactory)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoDatabaseFactory"/>.
        /// </summary>
        /// <remarks>Used by the other ctor and in tests.</remarks>
        public UmbracoDatabaseFactory(ILogger<UmbracoDatabaseFactory> logger, ILoggerFactory loggerFactory, GlobalSettings globalSettings, ConnectionStrings connectionStrings,  Lazy<IMapperCollection> mappers, IDbProviderFactoryCreator dbProviderFactoryCreator, DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory)
        {

            _globalSettings = globalSettings;
            _mappers = mappers ?? throw new ArgumentNullException(nameof(mappers));
            _dbProviderFactoryCreator = dbProviderFactoryCreator  ?? throw new ArgumentNullException(nameof(dbProviderFactoryCreator));
            _databaseSchemaCreatorFactory = databaseSchemaCreatorFactory ?? throw new ArgumentNullException(nameof(databaseSchemaCreatorFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggerFactory = loggerFactory;

            var settings = connectionStrings.UmbracoConnectionString;

            if (settings == null)
            {
                logger.LogDebug("Missing connection string, defer configuration.");
                return; // not configured
            }

            // could as well be <add name="umbracoDbDSN" connectionString="" providerName="" />
            // so need to test the values too
            var connectionString = settings.ConnectionString;
            var providerName = settings.ProviderName;
            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(providerName))
            {
                logger.LogDebug("Empty connection string or provider name, defer configuration.");
                return; // not configured
            }

            Configure(settings.ConnectionString, settings.ProviderName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoDatabaseFactory"/>.
        /// </summary>
        /// <remarks>Used in tests.</remarks>
        public UmbracoDatabaseFactory(ILogger<UmbracoDatabaseFactory> logger, ILoggerFactory loggerFactory, string connectionString, string providerName, Lazy<IMapperCollection> mappers, IDbProviderFactoryCreator dbProviderFactoryCreator, DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory)
        {
            _mappers = mappers ?? throw new ArgumentNullException(nameof(mappers));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggerFactory = loggerFactory;
            _dbProviderFactoryCreator = dbProviderFactoryCreator ?? throw new ArgumentNullException(nameof(dbProviderFactoryCreator));
            _databaseSchemaCreatorFactory = databaseSchemaCreatorFactory ?? throw new ArgumentNullException(nameof(databaseSchemaCreatorFactory));

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(providerName))
            {
                logger.LogDebug("Missing connection string or provider name, defer configuration.");
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
                    return !ConnectionString.IsNullOrWhiteSpace() && !_providerName.IsNullOrWhiteSpace();
                }
            }
        }

        /// <inheritdoc />
        public bool Initialized => Volatile.Read(ref _initialized);

        /// <inheritdoc />
        public string ConnectionString { get; private set; }

        /// <inheritdoc />
        public bool CanConnect =>
            // actually tries to connect to the database (regardless of configured/initialized)
            !ConnectionString.IsNullOrWhiteSpace() && !_providerName.IsNullOrWhiteSpace() &&
            DbConnectionExtensions.IsConnectionAvailable(ConnectionString, DbProviderFactory);

        private void UpdateSqlServerDatabaseType()
        {
            // replace NPoco database type by a more efficient one

            var setting = _globalSettings.DatabaseFactoryServerVersion;
            var fromSettings = false;

            if (setting.IsNullOrWhiteSpace() || !setting.StartsWith("SqlServer.")
                || !Enum<SqlServerSyntaxProvider.VersionName>.TryParse(setting.Substring("SqlServer.".Length), out var versionName, true))
            {
                versionName = ((SqlServerSyntaxProvider) _sqlSyntax).GetSetVersion(ConnectionString, _providerName, _logger).ProductVersionName;
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

            _logger.LogDebug("SqlServer {SqlServerVersion}, DatabaseType is {DatabaseType} ({Source}).",
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
        public IBulkSqlInsertProvider BulkSqlInsertProvider
        {
            get
            {
                // must be initialized to have a bulk insert provider
                EnsureInitialized();
                return _bulkSqlInsertProvider;
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

                ConnectionString = connectionString;
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
            _logger.LogDebug("Initializing.");

            if (ConnectionString.IsNullOrWhiteSpace()) throw new InvalidOperationException("The factory has not been configured with a proper connection string.");
            if (_providerName.IsNullOrWhiteSpace()) throw new InvalidOperationException("The factory has not been configured with a proper provider name.");

            if (DbProviderFactory == null)
                throw new Exception($"Can't find a provider factory for provider name \"{_providerName}\".");

            // cannot initialize without being able to talk to the database
            // TODO: Why not?
            if (!DbConnectionExtensions.IsConnectionAvailable(ConnectionString, DbProviderFactory))
                throw new Exception("Cannot connect to the database.");

            _connectionRetryPolicy = RetryPolicyFactory.GetDefaultSqlConnectionRetryPolicyByConnectionString(ConnectionString);
            _commandRetryPolicy = RetryPolicyFactory.GetDefaultSqlCommandRetryPolicyByConnectionString(ConnectionString);


            _databaseType = DatabaseType.Resolve(DbProviderFactory.GetType().Name, _providerName);
            if (_databaseType == null)
                throw new Exception($"Can't find an NPoco database type for provider name \"{_providerName}\".");

            _sqlSyntax = _dbProviderFactoryCreator.GetSqlSyntaxProvider(_providerName);
            if (_sqlSyntax == null)
                throw new Exception($"Can't find a sql syntax provider for provider name \"{_providerName}\".");

            _bulkSqlInsertProvider = _dbProviderFactoryCreator.CreateBulkSqlInsertProvider(_providerName);

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

            _logger.LogDebug("Initialized.");

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



        // method used by NPoco's UmbracoDatabaseFactory to actually create the database instance
        private UmbracoDatabase CreateDatabaseInstance()
        {
            return new UmbracoDatabase(ConnectionString, SqlContext, DbProviderFactory, _loggerFactory.CreateLogger<UmbracoDatabase>(), _bulkSqlInsertProvider, _databaseSchemaCreatorFactory, _connectionRetryPolicy, _commandRetryPolicy);
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
    }
}

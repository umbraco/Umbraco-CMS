using System.Data.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPoco;
using NPoco.FluentMappings;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Extensions;
using MapperCollection = NPoco.MapperCollection;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
///     Default implementation of <see cref="IUmbracoDatabaseFactory" />.
/// </summary>
/// <remarks>
///     <para>
///         This factory implementation creates and manages an "ambient" database connection. When running
///         within an Http context, "ambient" means "associated with that context". Otherwise, it means "static to
///         the current thread". In this latter case, note that the database connection object is not thread safe.
///     </para>
///     <para>
///         It wraps an NPoco UmbracoDatabaseFactory which is initializes with a proper IPocoDataFactory to ensure
///         that NPoco's plumbing is cached appropriately for the whole application.
///     </para>
/// </remarks>
// TODO: these comments are not true anymore
// TODO: this class needs not be disposable!
public class UmbracoDatabaseFactory : DisposableObjectSlim, IUmbracoDatabaseFactory
{
    private readonly DatabaseSchemaCreatorFactory _databaseSchemaCreatorFactory;
    private readonly IDbProviderFactoryCreator _dbProviderFactoryCreator;
    private readonly IOptions<GlobalSettings> _globalSettings;
    private readonly ILogger<UmbracoDatabaseFactory> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMapperCollection _mappers;
    private readonly NPocoMapperCollection _npocoMappers;
    private IBulkSqlInsertProvider? _bulkSqlInsertProvider;
    private DatabaseType? _databaseType;

    private DbProviderFactory? _dbProviderFactory;
    private bool _initialized;

    private object _lock = new();

    private DatabaseFactory? _npocoDatabaseFactory;
    private IPocoDataFactory? _pocoDataFactory;
    private MapperCollection? _pocoMappers;
    private SqlContext _sqlContext = null!;
    private ISqlSyntaxProvider? _sqlSyntax;

    private ConnectionStrings? _umbracoConnectionString;
    private bool _upgrading;

    #region Constructors

    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoDatabaseFactory" />.
    /// </summary>
    /// <remarks>Used by the other ctor and in tests.</remarks>
    public UmbracoDatabaseFactory(
        ILogger<UmbracoDatabaseFactory> logger,
        ILoggerFactory loggerFactory,
        IOptions<GlobalSettings> globalSettings,
        IOptionsMonitor<ConnectionStrings> connectionStrings,
        IMapperCollection mappers,
        IDbProviderFactoryCreator dbProviderFactoryCreator,
        DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory,
        NPocoMapperCollection npocoMappers)
    {
        _globalSettings = globalSettings;
        _mappers = mappers ?? throw new ArgumentNullException(nameof(mappers));
        _dbProviderFactoryCreator = dbProviderFactoryCreator ??
                                    throw new ArgumentNullException(nameof(dbProviderFactoryCreator));
        _databaseSchemaCreatorFactory = databaseSchemaCreatorFactory ??
                                        throw new ArgumentNullException(nameof(databaseSchemaCreatorFactory));
        _npocoMappers = npocoMappers;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _loggerFactory = loggerFactory;

        ConnectionStrings umbracoConnectionString = connectionStrings.CurrentValue;
        if (!umbracoConnectionString.IsConnectionStringConfigured())
        {
            logger.LogDebug("Missing connection string, defer configuration.");
            return; // not configured
        }

        Configure(umbracoConnectionString);
    }

    #endregion

    private DbProviderFactory? DbProviderFactory
    {
        get
        {
            if (_dbProviderFactory == null)
            {
                _dbProviderFactory = string.IsNullOrWhiteSpace(ProviderName)
                    ? null
                    : _dbProviderFactoryCreator.CreateFactory(ProviderName);
            }

            return _dbProviderFactory;
        }
    }

    /// <inheritdoc />
    public bool Configured
    {
        get
        {
            lock (_lock)
            {
                return !ConnectionString.IsNullOrWhiteSpace() && !ProviderName.IsNullOrWhiteSpace();
            }
        }
    }

    /// <inheritdoc />
    public bool Initialized => Volatile.Read(ref _initialized);

    /// <inheritdoc />
    public string? ConnectionString => _umbracoConnectionString?.ConnectionString;

    /// <inheritdoc />
    public string? ProviderName => _umbracoConnectionString?.ProviderName;

    /// <inheritdoc />
    public bool CanConnect =>

        // actually tries to connect to the database (regardless of configured/initialized)
        !ConnectionString.IsNullOrWhiteSpace() && !ProviderName.IsNullOrWhiteSpace() &&
        DbConnectionExtensions.IsConnectionAvailable(ConnectionString, DbProviderFactory);

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
    public IBulkSqlInsertProvider? BulkSqlInsertProvider
    {
        get
        {
            // must be initialized to have a bulk insert provider
            EnsureInitialized();

            return _bulkSqlInsertProvider;
        }
    }

    /// <inheritdoc />
    public void ConfigureForUpgrade() => _upgrading = true;

    /// <inheritdoc />
    public void Configure(ConnectionStrings umbracoConnectionString)
    {
        if (umbracoConnectionString is null)
        {
            throw new ArgumentNullException(nameof(umbracoConnectionString));
        }

        lock (_lock)
        {
            if (Volatile.Read(ref _initialized))
            {
                throw new InvalidOperationException("Already initialized.");
            }

            _umbracoConnectionString = umbracoConnectionString;
        }

        // rest to be lazy-initialized
    }

    /// <inheritdoc />
    public IUmbracoDatabase CreateDatabase()
    {
        // must be initialized to create a database
        EnsureInitialized();
        return (IUmbracoDatabase)_npocoDatabaseFactory!.GetDatabase();
    }

    private void EnsureInitialized() =>
        LazyInitializer.EnsureInitialized(ref _sqlContext, ref _initialized, ref _lock, Initialize);

    private SqlContext Initialize()
    {
        _logger.LogDebug("Initializing.");

        if (ConnectionString.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException("The factory has not been configured with a proper connection string.");
        }

        if (ProviderName.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException("The factory has not been configured with a proper provider name.");
        }

        if (DbProviderFactory == null)
        {
            throw new Exception($"Can't find a provider factory for provider name \"{ProviderName}\".");
        }

        _databaseType = DatabaseType.Resolve(DbProviderFactory.GetType().Name, ProviderName);
        if (_databaseType == null)
        {
            throw new Exception($"Can't find an NPoco database type for provider name \"{ProviderName}\".");
        }

        _sqlSyntax = _dbProviderFactoryCreator.GetSqlSyntaxProvider(ProviderName!);
        if (_sqlSyntax == null)
        {
            throw new Exception($"Can't find a sql syntax provider for provider name \"{ProviderName}\".");
        }

        _bulkSqlInsertProvider = _dbProviderFactoryCreator.CreateBulkSqlInsertProvider(ProviderName!);

        _databaseType = _sqlSyntax.GetUpdatedDatabaseType(_databaseType, ConnectionString);

        // ensure we have only 1 set of mappers, and 1 PocoDataFactory, for all database
        // so that everything NPoco is properly cached for the lifetime of the application
        _pocoMappers = new MapperCollection();

        // add all registered mappers for NPoco
        _pocoMappers.AddRange(_npocoMappers);

        _pocoMappers.AddRange(_dbProviderFactoryCreator.ProviderSpecificMappers(ProviderName!));

        var factory = new FluentPocoDataFactory(GetPocoDataFactoryResolver, _pocoMappers);
        _pocoDataFactory = factory;
        var config = new FluentConfig(xmappers => factory);

        // create the database factory
        _npocoDatabaseFactory = DatabaseFactory.Config(cfg =>
        {
            cfg.UsingDatabase(CreateDatabaseInstance) // creating UmbracoDatabase instances
                .WithFluentConfig(config); // with proper configuration

            foreach (IProviderSpecificInterceptor interceptor in _dbProviderFactoryCreator
                         .GetProviderSpecificInterceptors(ProviderName!))
            {
                cfg.WithInterceptor(interceptor);
            }
        });

        if (_npocoDatabaseFactory == null)
        {
            throw new NullReferenceException(
                "The call to UmbracoDatabaseFactory.Config yielded a null UmbracoDatabaseFactory instance.");
        }

        _logger.LogDebug("Initialized.");

        return new SqlContext(_sqlSyntax, _databaseType, _pocoDataFactory, _mappers);
    }

    // gets initialized poco data builders
    private InitializedPocoDataBuilder GetPocoDataFactoryResolver(Type type, IPocoDataFactory factory)
        => new UmbracoPocoDataBuilder(type, _pocoMappers, _upgrading).Init();

    // method used by NPoco's UmbracoDatabaseFactory to actually create the database instance
    private UmbracoDatabase? CreateDatabaseInstance()
    {
        if (ConnectionString is null || SqlContext is null || DbProviderFactory is null)
        {
            return null;
        }

        return new UmbracoDatabase(
            ConnectionString,
            SqlContext,
            DbProviderFactory,
            _loggerFactory.CreateLogger<UmbracoDatabase>(),
            _bulkSqlInsertProvider,
            _databaseSchemaCreatorFactory,
            _pocoMappers);
    }

    protected override void DisposeResources() =>

        // this is weird, because hybrid accessors store different databases per
        // thread, so we don't really know what we are disposing here...
        // besides, we don't really want to dispose the factory, which is a singleton...
        // TODO: the class does not need be disposable
        // var db = _umbracoDatabaseAccessor.UmbracoDatabase;
        // _umbracoDatabaseAccessor.UmbracoDatabase = null;
        // db?.Dispose();
        Volatile.Write(ref _initialized, false);
}

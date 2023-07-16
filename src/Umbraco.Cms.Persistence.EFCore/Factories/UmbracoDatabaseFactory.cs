using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Persistence.EFCore.Databases;
using Umbraco.Cms.Persistence.EFCore.DbContexts;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore.Factories;

/// <summary>
///     EF Core implementation of <see cref="Infrastructure.Persistence.IUmbracoDatabaseFactory" />.
/// </summary>
// TODO: This class shouldn't be IDisposable!
public class UmbracoDatabaseFactory : Infrastructure.Persistence.IUmbracoDatabaseFactory
{
    private readonly object _lock = new();
    private readonly DatabaseSchemaCreatorFactory _databaseSchemaCreatorFactory;
    private readonly IDbContextFactory<UmbracoDbContext> _dbContextFactory;

    private ConnectionStrings? _umbracoConnectionString;
    private bool _initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoDatabaseFactory"/> class.
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="databaseSchemaCreatorFactory"></param>
    /// <param name="dbContextFactory"></param>
    /// <param name="connectionStrings"></param>
    /// <remarks>Used by the other ctor and in tests.</remarks>
    public UmbracoDatabaseFactory(
        ILoggerFactory loggerFactory,
        DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory,
        IDbContextFactory<UmbracoDbContext> dbContextFactory,
        IOptionsMonitor<ConnectionStrings> connectionStrings)
    {
        _databaseSchemaCreatorFactory = databaseSchemaCreatorFactory ??
                                        throw new ArgumentNullException(nameof(databaseSchemaCreatorFactory));
        _dbContextFactory = dbContextFactory;
        ILogger<UmbracoDatabaseFactory> logger = loggerFactory.CreateLogger<UmbracoDatabaseFactory>();

        ConnectionStrings umbracoConnectionString = connectionStrings.CurrentValue;
        if (!umbracoConnectionString.IsConnectionStringConfigured())
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Missing connection string, defer configuration.");
            }

            return; // not configured
        }

        Configure(umbracoConnectionString);
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
    public string? ConnectionString => _umbracoConnectionString?.ConnectionString;

    /// <inheritdoc />
    public string? ProviderName => _umbracoConnectionString?.ProviderName;

    /// <inheritdoc />
    public bool CanConnect
    {
        get
        {
            UmbracoDbContext dbContext = _dbContextFactory.CreateDbContext();
            return dbContext.Database.CanConnect();
        }
    }

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
    }

    /// <inheritdoc />
    public Infrastructure.Persistence.IUmbracoDatabase CreateDatabase()
    {
        UmbracoDbContext dbContext = _dbContextFactory.CreateDbContext();
        return new UmbracoDatabase(dbContext, _databaseSchemaCreatorFactory);
    }

    /// <inheritdoc />
    public async Task<Infrastructure.Persistence.IUmbracoDatabase> CreateDatabaseAsync(CancellationToken cancellationToken = default)
    {
        UmbracoDbContext dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return new UmbracoDatabase(dbContext, _databaseSchemaCreatorFactory);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // TODO: Remove IDisposable from this class when NPOCO is removed from the repositories. - It's not needed anymore.
    }

    #region Obsolete
    /// <inheritdoc />
    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    public bool Initialized => throw new NotImplementedException();

    /// <inheritdoc />
    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    public Infrastructure.Persistence.ISqlContext SqlContext => throw new NotImplementedException();

    /// <inheritdoc />
    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    public Infrastructure.Persistence.IBulkSqlInsertProvider? BulkSqlInsertProvider => throw new NotImplementedException();

    /// <inheritdoc />
    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    public void ConfigureForUpgrade() => throw new NotImplementedException();
    #endregion
}

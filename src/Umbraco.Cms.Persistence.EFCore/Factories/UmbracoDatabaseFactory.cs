using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Persistence.EFCore.Databases;
using Umbraco.Cms.Persistence.EFCore.DbContexts;

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
    private readonly Infrastructure.Persistence.UmbracoDatabaseFactory _legacyDatabaseFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoDatabaseFactory"/> class.
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="databaseSchemaCreatorFactory"></param>
    /// <param name="dbContextFactory"></param>
    /// <remarks>Used by the other ctor and in tests.</remarks>
    public UmbracoDatabaseFactory(
        ILoggerFactory loggerFactory,
        DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory,
        IDbContextFactory<UmbracoDbContext> dbContextFactory)
    {
        _databaseSchemaCreatorFactory = databaseSchemaCreatorFactory ??
                                        throw new ArgumentNullException(nameof(databaseSchemaCreatorFactory));
        _dbContextFactory = dbContextFactory;

        // TODO: Remove leagcy factory - We are using the StaticServiceProvider to avoid having breaking changes later
        _legacyDatabaseFactory = (Infrastructure.Persistence.UmbracoDatabaseFactory)StaticServiceProvider.Instance.GetService(typeof(Infrastructure.Persistence.UmbracoDatabaseFactory))!;

        ILogger<UmbracoDatabaseFactory> logger = loggerFactory.CreateLogger<UmbracoDatabaseFactory>();
    }


    /// <inheritdoc />
    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    public bool Configured => _legacyDatabaseFactory.Configured;

    /// <inheritdoc />
    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    public string? ConnectionString => _legacyDatabaseFactory.ConnectionString;

    /// <inheritdoc />
    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    public string? ProviderName => _legacyDatabaseFactory.ProviderName;

    /// <inheritdoc />
    public bool CanConnect
    {
        get
        {
            UmbracoDbContext dbContext = _dbContextFactory.CreateDbContext();
            if (dbContext.Database.CanConnect())
            {
                return true;
            }
            else
            {
                return _legacyDatabaseFactory.CanConnect; // TODO: Remove this when it's time to remove the old UmbracoDatabase implementation and tests have been rewritten to not use Configure to set the connection string
            }
        }
    }

    /// <inheritdoc />
    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    public void Configure(ConnectionStrings umbracoConnectionString) => _legacyDatabaseFactory.Configure(umbracoConnectionString);

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

        _legacyDatabaseFactory.Dispose(); // TODO: Remove this when it's time to remove the old UmbracoDatabase implementation
    }

    #region Obsolete
    /// <inheritdoc />
    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    public bool Initialized => _legacyDatabaseFactory.Initialized;

    /// <inheritdoc />
    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    public Infrastructure.Persistence.ISqlContext SqlContext => _legacyDatabaseFactory.SqlContext;

    /// <inheritdoc />
    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    public Infrastructure.Persistence.IBulkSqlInsertProvider? BulkSqlInsertProvider => _legacyDatabaseFactory.BulkSqlInsertProvider;

    /// <inheritdoc />
    [Obsolete("This will be removed when NPOCO is removed from the repositories.")]
    public void ConfigureForUpgrade() => _legacyDatabaseFactory.ConfigureForUpgrade();
    #endregion
}

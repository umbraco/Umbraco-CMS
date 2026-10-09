using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.Sqlite.Configuration;

/// <summary>
///     Removes shared-cache mode from a file-backed SQLite connection string.
/// </summary>
/// <remarks>
///     <para>
///         Umbraco creates its SQLite databases in WAL mode, which lets readers and writers work concurrently
///         without blocking each other. Shared-cache mode replaces that with table-level locking between the
///         connections that share the cache, and a connection that cannot obtain a table lock fails immediately
///         with <c>SQLITE_LOCKED</c> ("database table is locked") instead of waiting. Combining the two is
///         discouraged by both SQLite and Microsoft.Data.Sqlite, and it breaks the assumption that
///         <see cref="Services.SqliteDistributedLockingMechanism" /> is built on.
///     </para>
///     <para>
///         Connection strings generated before this was corrected still carry <c>Cache=Shared</c>, so it is
///         removed here rather than only at the point the connection string is generated. Shared-cache mode is
///         left alone for in-memory databases, where it is what makes the database shareable between connections.
///     </para>
/// </remarks>
internal sealed class ConfigureSqliteConnectionStringCacheMode : IPostConfigureOptions<ConnectionStrings>
{
    private readonly ILogger<ConfigureSqliteConnectionStringCacheMode> _logger;
    private bool _sharedCacheReported;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigureSqliteConnectionStringCacheMode" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ConfigureSqliteConnectionStringCacheMode(ILogger<ConfigureSqliteConnectionStringCacheMode> logger)
        => _logger = logger;

    /// <inheritdoc />
    public void PostConfigure(string? name, ConnectionStrings options)
    {
        if (options.IsConnectionStringConfigured() is false
            || options.ProviderName.InvariantEquals(Constants.ProviderName) is false)
        {
            return;
        }

        SqliteConnectionStringBuilder connectionStringBuilder;
        try
        {
            connectionStringBuilder = new SqliteConnectionStringBuilder(options.ConnectionString);
        }
        catch (ArgumentException exception)
        {
            // The connection string cannot be rewritten, so leave it alone rather than failing to configure
            // options - which would surface as an obscure startup failure rather than a connection error.
            const string message = "Shared-cache mode could not be removed, because the connection string "
                + "could not be parsed. Remove \"Cache=Shared\" from the connection string instead.";
            _logger.LogWarning(exception, message);
            return;
        }

        if (connectionStringBuilder.Cache is not SqliteCacheMode.Shared
            || connectionStringBuilder.Mode is SqliteOpenMode.Memory)
        {
            return;
        }

        connectionStringBuilder.Remove("Cache");
        options.ConnectionString = connectionStringBuilder.ConnectionString;

        if (_sharedCacheReported is false)
        {
            _sharedCacheReported = true;
            _logger.LogInformation(
                "Shared-cache mode was removed from the SQLite connection string, because it causes "
                + "\"database table is locked\" errors on a write-ahead logging database. Remove \"Cache=Shared\" "
                + "from the connection string to stop this from being corrected on every start.");
        }
    }
}

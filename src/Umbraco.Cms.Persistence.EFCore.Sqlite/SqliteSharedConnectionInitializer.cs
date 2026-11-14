using System.Data.Common;
using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Persistence.EFCore.Sqlite;

/// <summary>
/// Registers EF Core's SQLite functions and collations (for example those used to compare and order
/// <see cref="decimal" /> values) on a shared connection, which EF Core only does for connections it opens itself.
/// </summary>
public class SqliteSharedConnectionInitializer : IEFCoreSharedConnectionInitializer
{
    private readonly ConditionalWeakTable<SqliteConnection, object> _initializedConnections = new();
    private readonly ILogger<SqliteSharedConnectionInitializer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteSharedConnectionInitializer"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public SqliteSharedConnectionInitializer(ILogger<SqliteSharedConnectionInitializer> logger)
        => _logger = logger;

    /// <inheritdoc />
    public string ProviderName => Constants.ProviderNames.SQLLite;

    /// <inheritdoc />
    public void Initialize(DbContext dbContext, DbConnection connection)
    {
        if (NPocoDatabaseExtensions.TryGetTypedConnection(connection, out SqliteConnection? sqliteConnection) is false)
        {
            _logger.LogWarning(
                "Could not find the SQLite connection within the shared connection of type {ConnectionType}, so EF Core's SQLite functions are not available to {DbContextType}.",
                connection.GetType().FullName,
                dbContext.GetType().FullName);
            return;
        }

        if (_initializedConnections.TryGetValue(sqliteConnection, out _))
        {
            return;
        }

        // EF Core registers its functions and collations when it is given an existing SqliteConnection,
        // so resolving the relational connection of a context built on the raw connection applies them.
        // The context does not own the connection, so disposing it leaves the connection open.
        DbContextOptions options = new DbContextOptionsBuilder().UseSqlite(sqliteConnection).Options;
        using (var initializationContext = new DbContext(options))
        {
            initializationContext.GetService<IRelationalConnection>();
        }

        _initializedConnections.AddOrUpdate(sqliteConnection, new object());
    }
}

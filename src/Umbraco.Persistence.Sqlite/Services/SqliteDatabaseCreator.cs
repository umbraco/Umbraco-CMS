using Microsoft.Data.Sqlite;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Persistence.Sqlite.Services;

/// <summary>
/// Implements <see cref="IDatabaseCreator"/> for SQLite.
/// </summary>
public class SqliteDatabaseCreator : IDatabaseCreator
{
    /// <inheritdoc />
    public string ProviderName => Constants.ProviderName;

    /// <inheritdoc />
    /// <remarks>
    /// Creates a SQLite database file.
    /// </remarks>
    public void Create(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
    }
}

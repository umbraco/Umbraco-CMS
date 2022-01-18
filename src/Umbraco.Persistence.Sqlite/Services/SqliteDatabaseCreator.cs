using Microsoft.Data.SqlClient;
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
    public void Create(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();
    }
}

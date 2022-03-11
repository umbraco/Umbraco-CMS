using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Persistence.Sqlite.Services;

/// <summary>
/// Implements <see cref="IDatabaseCreator"/> for SQLite.
/// </summary>
public class SqliteDatabaseCreator : IDatabaseCreator
{
    /// <inheritdoc />
    public string ProviderName => Constants.ProviderName;

    /// <summary>
    /// Creates a SQLite database file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// With journal_mode = wal we have snapshot isolation.
    /// </para>
    /// <para>
    /// Concurrent read/write can take occur, committing a write transaction will have no impact
    /// on open read transactions as they see only committed data from the point in time that they began reading.
    /// </para>
    /// <para>
    /// A write transaction still requires exclusive access to database files so concurrent writes are not possible.
    /// </para>
    /// <para>
    /// Read more <a href="https://www.sqlite.org/isolation.html">Isolation in SQLite</a> <br/>
    /// Read more <a href="https://www.sqlite.org/wal.html">Write-Ahead Logging</a>
    /// </para>
    /// </remarks>
    public void Create(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "PRAGMA journal_mode = wal;";
        command.ExecuteNonQuery();

        command.CommandText = "PRAGMA journal_mode";
        var mode = command.ExecuteScalar();

        Debug.Assert(mode as string == "wal", "incorrect journal_mode");
    }
}

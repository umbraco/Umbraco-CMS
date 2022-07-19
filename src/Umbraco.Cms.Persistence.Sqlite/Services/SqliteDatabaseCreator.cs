using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Persistence.Sqlite.Services;

/// <summary>
///     Implements <see cref="IDatabaseCreator" /> for SQLite.
/// </summary>
public class SqliteDatabaseCreator : IDatabaseCreator
{
    private readonly ILogger<SqliteDatabaseCreator> _logger;

    public SqliteDatabaseCreator(ILogger<SqliteDatabaseCreator> logger) => _logger = logger;

    /// <inheritdoc />
    public string ProviderName => Constants.ProviderName;

    /// <summary>
    ///     Creates a SQLite database file.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         With journal_mode = wal we have snapshot isolation.
    ///     </para>
    ///     <para>
    ///         Concurrent read/write can take occur, committing a write transaction will have no impact
    ///         on open read transactions as they see only committed data from the point in time that they began reading.
    ///     </para>
    ///     <para>
    ///         A write transaction still requires exclusive access to database files so concurrent writes are not possible.
    ///     </para>
    ///     <para>
    ///         Read more <a href="https://www.sqlite.org/isolation.html">Isolation in SQLite</a> <br />
    ///         Read more <a href="https://www.sqlite.org/wal.html">Write-Ahead Logging</a>
    ///     </para>
    /// </remarks>
    public void Create(string connectionString)
    {
        var original = new SqliteConnectionStringBuilder(connectionString);

        if (original.Mode == SqliteOpenMode.Memory || original.DataSource == ":memory:")
        {
            // In-Memory mode - bail
            return;
        }

        if (original.DataSource.StartsWith("file:"))
        {
            // URI mode - bail
            return;
        }

        /* Note: The following may seem a bit mad, but it has a purpose!
         *
         * On azure app services if we wish to ensure the database is persistent we need to write it to the persistent network share
         * e.g. c:\home or /home
         *
         * However the network share does not play nice at all with SQLite locking for rollback mode which is the default for new databases.
         * May work on windows app services with win32 vfs but not at all on linux with unix vfs.
         *
         * The experience is so broken in fact that we can't even create an empty sqlite database file and switch from rollback to wal.
         * However once a wal database is setup it works reasonably well (perhaps a tad slower than one might like) on the persistent share.
         *
         * So instead of creating in the final destination, we can create in /tmp || $env:Temp, set the wal bits
         * and copy the file over to its new home and finally nuke the temp file.
         *
         * We could try to do this only on azure e.g. detect $WEBSITE_RESOURCE_GROUP etc but there's no downside to
         * always initializing in this way and it probably helps for non azure scenarios also (anytime persisting on a cifs mount for example).
         */

        var tempFile = Path.GetTempFileName();
        var tempConnectionString = new SqliteConnectionStringBuilder { DataSource = tempFile, Pooling = false };

        using (var connection = new SqliteConnection(tempConnectionString.ConnectionString))
        {
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = "PRAGMA journal_mode = wal;";
            command.ExecuteNonQuery();
        }

        // Copy our blank(ish) wal mode sqlite database to its final location.
        try
        {
            File.Copy(tempFile, original.DataSource, true);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unable to initialize sqlite database file.");
            throw;
        }

        try
        {
            File.Delete(tempFile);
        }
        catch (Exception ex)
        {
            // We can swallow this, no worries if we can't nuke the practically empty database file.
            _logger.LogWarning(ex, "Unable to cleanup temporary sqlite database file {path}", tempFile);
        }
    }
}

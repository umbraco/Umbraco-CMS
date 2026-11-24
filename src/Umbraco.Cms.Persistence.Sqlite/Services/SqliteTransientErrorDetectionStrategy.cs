using Microsoft.Data.Sqlite;
using SQLitePCL;
using Umbraco.Cms.Infrastructure.Persistence.FaultHandling;

namespace Umbraco.Cms.Persistence.Sqlite.Services;

/// <summary>
/// Detects transient SQLite errors (BUSY, LOCKED) for retry policies.
/// </summary>
public class SqliteTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
{
    /// <inheritdoc />
    public bool IsTransient(Exception ex)
    {
        if (ex is not SqliteException sqliteException)
        {
            return false;
        }

        // The transaction's snapshot is older than the latest commit, so the statement can never succeed within it;
        // retrying would only delay the failure until the retries run out.
        if (sqliteException.SqliteExtendedErrorCode is raw.SQLITE_BUSY_SNAPSHOT)
        {
            return false;
        }

        return sqliteException.IsTransient || sqliteException.IsBusyOrLocked();
    }
}

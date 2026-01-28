using Microsoft.Data.Sqlite;
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

        return sqliteException.IsTransient || sqliteException.IsBusyOrLocked();
    }
}

using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace Umbraco.Cms.Persistence.Sqlite.Services;

/// <summary>
/// Extension methods for SQLite exception handling.
/// </summary>
public static class SqliteExceptionExtensions
{
    /// <summary>
    /// Determines if the SQLite exception is a BUSY or LOCKED error.
    /// </summary>
    /// <param name="ex">The SQLite exception to check.</param>
    /// <returns><c>true</c> if the error is BUSY, LOCKED, or LOCKED_SHAREDCACHE; otherwise <c>false</c>.</returns>
    public static bool IsBusyOrLocked(this SqliteException ex) =>
        ex.SqliteErrorCode
            is raw.SQLITE_BUSY
            or raw.SQLITE_LOCKED
            or raw.SQLITE_LOCKED_SHAREDCACHE;
}

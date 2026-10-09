using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace Umbraco.Cms.Persistence.EFCore;

/// <summary>
/// SQLite-specific exception helpers for code running on the EF Core persistence stack.
/// </summary>
/// <remarks>
/// A parallel helper exists at <c>Umbraco.Cms.Persistence.Sqlite.Services.SqliteExceptionExtensions</c>
/// for the NPoco stack. Both stacks are independent (neither references the other) so the small
/// duplication is intentional — keeps the layering clean.
/// </remarks>
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

using Microsoft.Data.Sqlite;

namespace Umbraco.Cms.Persistence.Sqlite.Services;

public static class SqliteExceptionExtensions
{
    public static bool IsBusyOrLocked(this SqliteException ex) =>
        ex.SqliteErrorCode
            is SQLitePCL.raw.SQLITE_BUSY
            or SQLitePCL.raw.SQLITE_LOCKED
            or SQLitePCL.raw.SQLITE_LOCKED_SHAREDCACHE;
}

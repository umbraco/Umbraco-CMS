using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace Umbraco.Cms.Persistence.Sqlite.Services;

public static class SqliteExceptionExtensions
{
    public static bool IsBusyOrLocked(this SqliteException ex) =>
        ex.SqliteErrorCode
            is raw.SQLITE_BUSY
            or raw.SQLITE_LOCKED
            or raw.SQLITE_LOCKED_SHAREDCACHE;
}

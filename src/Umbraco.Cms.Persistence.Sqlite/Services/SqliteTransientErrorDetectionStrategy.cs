using Microsoft.Data.Sqlite;
using Umbraco.Cms.Infrastructure.Persistence.FaultHandling;

namespace Umbraco.Cms.Persistence.Sqlite.Services;

public class SqliteTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
{
    public bool IsTransient(Exception ex)
    {
        if (ex is not SqliteException sqliteException)
        {
            return false;
        }

        return sqliteException.IsTransient || sqliteException.IsBusyOrLocked();
    }
}

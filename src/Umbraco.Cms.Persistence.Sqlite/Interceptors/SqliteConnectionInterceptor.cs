using System.Data.Common;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Persistence.Sqlite.Interceptors;

/// <summary>
/// Base class for SQLite-specific connection interceptors.
/// </summary>
public abstract class SqliteConnectionInterceptor : IProviderSpecificConnectionInterceptor
{
    /// <inheritdoc />
    public string ProviderName => Constants.ProviderName;

    /// <inheritdoc />
    public abstract DbConnection OnConnectionOpened(IDatabase database, DbConnection conn);

    /// <inheritdoc />
    public virtual void OnConnectionClosing(IDatabase database, DbConnection conn)
    {
    }
}

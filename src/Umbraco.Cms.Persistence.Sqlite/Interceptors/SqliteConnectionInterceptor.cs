using System.Data.Common;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Persistence.Sqlite.Interceptors;

public abstract class SqliteConnectionInterceptor : IProviderSpecificConnectionInterceptor
{
    public string ProviderName => Constants.ProviderName;

    public abstract DbConnection OnConnectionOpened(IDatabase database, DbConnection conn);

    public virtual void OnConnectionClosing(IDatabase database, DbConnection conn)
    {
    }
}

using System;
using NPoco;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core.Persistence.Interceptors
{
    public class LockTimeoutInterceptor : IExceptionInterceptor
    {
        public void OnException(IDatabase database, Exception exception)
        {
            if (database is IUmbracoDatabase umbracoDatabase && umbracoDatabase.SqlContext.SqlSyntax.IsLockTimeoutException(exception))
            {
                throw new LockTimeoutException(exception);
            }

        }
    }
}

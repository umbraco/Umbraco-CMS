using System;
using System.Data.SqlClient;
using NPoco;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core.Persistence.Interceptors
{
    public class LockTimeoutInterceptor : IExceptionInterceptor
    {
        public void OnException(IDatabase database, Exception exception)
        {
            if (exception is SqlException sqlException && sqlException.Number == 1222)
            {
               throw new LockTimeoutException(exception);
            }
        }
    }
}

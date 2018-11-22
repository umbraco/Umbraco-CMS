using System;
using System.Data.SqlClient;

namespace Umbraco.Core.Persistence.FaultHandling.Strategies
{
    /// <summary>
    /// Implements a strategy that detects network connectivity errors such as host not found.
    /// </summary>
    public class NetworkConnectivityErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        public bool IsTransient(Exception ex)
        {
            SqlException sqlException;

            if (ex != null && (sqlException = ex as SqlException) != null)
            {
                switch (sqlException.Number)
                {
                    // SQL Error Code: 11001
                    // A network-related or instance-specific error occurred while establishing a connection to SQL Server. 
                    // The server was not found or was not accessible. Verify that the instance name is correct and that SQL 
                    // Server is configured to allow remote connections. (provider: TCP Provider, error: 0 - No such host is known.)
                    case 11001:
                        return true;
                }
            }

            return false;
        }
    }
}
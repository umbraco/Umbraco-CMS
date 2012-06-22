using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;

namespace SQLCE4Umbraco
{
    public static class SqlCeContextGuardian
    {
        private static SqlCeConnection _constantOpenConnection;
        private static object objLock = new object();

        // Awesome SQL CE 4 speed improvement by Erik Ejskov Jensen - SQL CE 4 MVP
        // It's not an issue with SQL CE 4 that we never close the connection
        public static SqlCeConnection Open(string connectionString)
        {
            var connectionStringBuilder = new DbConnectionStringBuilder();
            try
            {
                connectionStringBuilder.ConnectionString = connectionString;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Bad connection string.", "connectionString", ex);
            }
            connectionStringBuilder.Remove("datalayer");

            // SQL CE 4 performs better when there's always a connection open in the background
            ensureOpenBackgroundConnection(connectionStringBuilder.ConnectionString);

            SqlCeConnection conn = new SqlCeConnection(connectionStringBuilder.ConnectionString);
            conn.Open();

            return conn;

        }

        private static void ensureOpenBackgroundConnection(string connectionString)
        {
            lock (objLock)
            {
                if (_constantOpenConnection == null)
                {
                    _constantOpenConnection = new SqlCeConnection(connectionString);
                    _constantOpenConnection.Open();
                }
                else if (_constantOpenConnection.State != ConnectionState.Open)
                    _constantOpenConnection.Open();
            }
        }
    }
}

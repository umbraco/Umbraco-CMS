using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;

namespace SQLCE4Umbraco
{
    public static class SqlCeContextGuardian
    {
        private static SqlCeConnection _conn;
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

            lock (objLock)
            {
                if (_conn != null)
                    throw new InvalidOperationException("Already opened");
                _conn = new SqlCeConnection(connectionStringBuilder.ConnectionString);
                _conn.Open();
            }

            return _conn;
        }
    }
}

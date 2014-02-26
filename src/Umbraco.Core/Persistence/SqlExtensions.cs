using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Persistence
{
    internal static class SqlExtensions
    {
        public static bool IsConnectionAvailable(string connString)
        {
            using (var connection = new SqlConnection(connString))
            {
                return connection.IsAvailable();
            }
        }

        public static bool IsAvailable(this SqlConnection connection)
        {
            try
            {
                connection.Open();
                connection.Close();
            }
            catch (SqlException)
            {
                return false;
            }

            return true;
        }
    }
}
